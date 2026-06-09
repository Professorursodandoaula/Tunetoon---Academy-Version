using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using TTMulti;
using TTMulti.Forms;

namespace Tunetoon.MultiController
{
    public static class MulticontrollerBridge
    {
        [DllImport("shell32.dll")]
        private static extern int SetCurrentProcessExplicitAppUserModelID(
            [MarshalAs(UnmanagedType.LPWStr)] string AppID);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length, flags, showCmd;
            public System.Drawing.Point ptMinPosition, ptMaxPosition;
            public RECT rcNormalPosition;
        }

        // Uma entrada por monitor ativo
        private static readonly List<(Thread thread, MulticontrollerWnd wnd)> _instances
            = new List<(Thread, MulticontrollerWnd)>();

        private static string DebugPath => System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mc_debug.txt");
        private static void Log(string msg) => System.IO.File.AppendAllText(DebugPath, msg + "\n");

        // ── Mapeamento WindowPosition → (grupo 1-based, isLeft) ──────────────
        // Grupo 1-based: ímpares ficam na coluna esquerda, pares na direita do Multicontroller
        // Internamente o Multicontroller usa índice 0-based, então grupo 1 = índice 0, grupo 2 = índice 1

        private static readonly Dictionary<int, (int group1based, bool isLeft)> SlotToGroupMap
            = new Dictionary<int, (int, bool)>
        {
            // WindowPosition enum values (baseados na ordem do enum em WindowManager.cs):
            // EsquerdaMetade=0, DireitaMetade=1, CimaMetade=2, BaixoMetade=3,
            // CantoCimaEsquerda=4, CantoCimaDireita=5, CantoBaixoEsquerda=6, CantoBaixoDireita=7,
            // TercoEsquerda=8, TercoCentro=9, TercoDireita=10,
            // Grid6CimaCol1=11, Grid6CimaCol2=12, Grid6CimaCol3=13,
            // Grid6BaixoCol1=14, Grid6BaixoCol2=15, Grid6BaixoCol3=16,
            // Grid8CimaCol1=17, Grid8CimaCol2=18, Grid8CimaCol3=19, Grid8CimaCol4=20,
            // Grid8BaixoCol1=21, Grid8BaixoCol2=22, Grid8BaixoCol3=23, Grid8BaixoCol4=24,
            // Custom4CimaEsquerda=25, Custom4CimaDireita=26, Custom4BaixoEsquerda=27, Custom4BaixoDireita=28,
            // Shopping=29, CentrarNaTela=30, Maximizar=31

            // Cantos
            { 4,  (2, false) },  // CantoCimaEsquerda   → Group 2, Right
            { 5,  (2, false) },  // CantoCimaDireita     → Group 2, Right
            { 6,  (1, false) },  // CantoBaixoEsquerda   → Group 1, Right
            { 7,  (1, false) },  // CantoBaixoDireita    → Group 1, Right

            // Grid 3x2
            { 11, (2, true)  },  // Grid6CimaCol1        → Group 2, Left
            { 12, (1, false) },  // Grid6CimaCol2        → Group 1, Right
            { 13, (1, false) },  // Grid6CimaCol3        → Group 1, Right
            { 14, (2, true)  },  // Grid6BaixoCol1       → Group 2, Left
            { 15, (1, false) },  // Grid6BaixoCol2       → Group 1, Right
            { 16, (1, false) },  // Grid6BaixoCol3       → Group 1, Right

            // Grid 4x2
            { 17, (1, false) },  // Grid8CimaCol1        → Group 1, Right
            { 18, (1, false) },  // Grid8CimaCol2        → Group 1, Right
            { 19, (2, false) },  // Grid8CimaCol3        → Group 2, Right
            { 20, (2, false) },  // Grid8CimaCol4        → Group 2, Right
            { 21, (1, false) },  // Grid8BaixoCol1       → Group 1, Right
            { 22, (1, false) },  // Grid8BaixoCol2       → Group 1, Right
            { 23, (2, false) },  // Grid8BaixoCol3       → Group 2, Right
            { 24, (2, false) },  // Grid8BaixoCol4       → Group 2, Right
        };

        // Padrão para slots não mapeados: Group 1, Right
        private static (int group1based, bool isLeft) DefaultMapping => (1, false);

        // ── API pública ───────────────────────────────────────────────────────

        public static void OpenWithHandles(Dictionary<string, (IntPtr handle, int windowSlot, int monitor)> toonHandles)
        {
            try
            {
                Log($"OpenWithHandles called, count={toonHandles?.Count}");

                if (toonHandles == null || toonHandles.Count == 0)
                    return;

                // Fecha tudo que estiver aberto
                CloseAll();

                // Agrupa por monitor (usa loginMonitor)
                var byMonitor = GroupByMonitor(toonHandles);
                Log($"monitors={byMonitor.Count}");

                // Abre monitor 0 primeiro, depois os demais em ordem
                foreach (var monitorIdx in byMonitor.Keys.OrderBy(k => k))
                {
                    var assignments = BuildAssignmentsBySlot(byMonitor[monitorIdx]);
                    if (assignments.Count == 0) continue;
                    LaunchInstance(assignments, monitorIdx);
                }

                Log("Done");
            }
            catch (Exception ex)
            {
                Log($"EXCEPTION: {ex}");
            }
        }

        public static void CloseAll()
        {
            foreach (var (thread, wnd) in _instances.ToList())
            {
                try
                {
                    if (wnd != null && !wnd.IsDisposed)
                        wnd.Invoke((Action)(() => wnd.Close()));
                    thread?.Join(2000);
                }
                catch { }
            }
            _instances.Clear();
        }

        // ── Agrupamento por monitor ───────────────────────────────────────────

        private static Dictionary<int, List<(string name, IntPtr handle, int windowSlot)>> GroupByMonitor(
            Dictionary<string, (IntPtr handle, int windowSlot, int monitor)> toonHandles)
        {
            var result = new Dictionary<int, List<(string, IntPtr, int)>>();
            foreach (var kvp in toonHandles)
            {
                if (kvp.Value.handle == IntPtr.Zero) continue;
                int monitorIdx = kvp.Value.monitor;
                if (!result.ContainsKey(monitorIdx))
                    result[monitorIdx] = new List<(string, IntPtr, int)>();
                result[monitorIdx].Add((kvp.Key, kvp.Value.handle, kvp.Value.windowSlot));
            }
            return result;
        }

        // ── Distribuição por WindowSlot → mapeamento de grupo/lado ───────────

        private static List<(int groupIndex, bool isLeft, IntPtr handle)> BuildAssignmentsBySlot(
            List<(string name, IntPtr handle, int windowSlot)> windows)
        {
            var result = new List<(int groupIndex, bool isLeft, IntPtr handle)>();

            // Conta quantas janelas vão para cada grupo/lado para evitar duplicatas no mesmo slot
            // (ex: dois toons no mesmo windowSlot → ambos vão como Right no mesmo grupo)
            var groupCounters = new Dictionary<int, int>(); // group1based → count

            foreach (var w in windows)
            {
                (int group1based, bool isLeft) mapping;

                if (w.windowSlot >= 0 && SlotToGroupMap.TryGetValue(w.windowSlot, out mapping))
                {
                    Log($"{w.name}: slot={w.windowSlot} group={mapping.group1based} isLeft={mapping.isLeft} handle={w.handle}");
                }
                else
                {
                    mapping = DefaultMapping;
                    Log($"{w.name}: slot={w.windowSlot} → default group={mapping.group1based} isLeft={mapping.isLeft} handle={w.handle}");
                }

                // Converte grupo 1-based para índice 0-based
                int groupIdx = mapping.group1based - 1;
                result.Add((groupIdx, mapping.isLeft, w.handle));
            }

            return result;
        }

        // ── Lançamento de instância ───────────────────────────────────────────

        private static void LaunchInstance(List<(int groupIndex, bool isLeft, IntPtr handle)> assignments, int monitorIdx)
        {
            var mcInstance = new TTMulti.Multicontroller();

            var ready = new ManualResetEventSlim(false);
            MulticontrollerWnd wnd = null;

            var thread = new Thread(() =>
            {
                wnd = new MulticontrollerWnd(mcInstance);
                wnd.ShowInTaskbar = true;

                wnd.HandleCreated += (hs, he) =>
                {
                    var appId = "Tunetoon.Multicontroller." + System.Guid.NewGuid().ToString("N")[..8];
                    SetCurrentProcessExplicitAppUserModelID(appId);
                };

                wnd.Shown += (s, e) =>
                {
                    // 1. Assign janelas
                    mcInstance.AssignWindowsInstance(assignments);

                    // 2. Ativa Mirror Mode com Mouse Replication
                    mcInstance.ActivateMirrorWithMouse();

                    // 3. Ativa o Multicontroller
                    wnd.ActivateAfterAssign();

                    // 4. Posiciona a janela do Multicontroller no monitor correto
                    var screens = Screen.AllScreens.OrderBy(sc => sc.Bounds.X).ToArray();
                    var screen  = monitorIdx < screens.Length ? screens[monitorIdx] : Screen.PrimaryScreen;
                    var wa      = screen.WorkingArea;
                    int fullH   = screen.Bounds.Height;

                    // Posiciona abaixo do Grid6 Bottom Col2, centralizado no monitor
                    int col2X     = wa.X + wa.Width / 3;
                    int col2W     = wa.Width / 3;
                    int grid6BotY = wa.Y + (int)(fullH * 0.393) * 2;

                    int wndX = col2X + (col2W - wnd.Width) / 2;
                    int wndY = grid6BotY;

                    if (wndX < wa.Left) wndX = wa.Left;
                    if (wndY + wnd.Height > wa.Bottom) wndY = wa.Bottom - wnd.Height;

                    wnd.Location = new System.Drawing.Point(wndX, wndY);

                    ready.Set();
                };

                Application.Run(new ApplicationContext(wnd));
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Name = $"MulticontrollerUIThread_Monitor{monitorIdx}";
            thread.Start();

            ready.Wait();
            _instances.Add((thread, wnd));
        }
    }
}
