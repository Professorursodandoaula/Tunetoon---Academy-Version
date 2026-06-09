using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tunetoon
{
    public static class WindowManager
    {
        // ── Win32 P/Invoke ────────────────────────────────────────────────────

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        // Retorna a borda invisível (shadow) de cada lado da janela
        private static int GetInvisibleBorder(IntPtr hWnd)
        {
            try
            {
                DwmGetWindowAttribute(hWnd, DWMWA_EXTENDED_FRAME_BOUNDS, out RECT rf, Marshal.SizeOf(typeof(RECT)));
                GetWindowRect(hWnd, out RECT rw);
                return rf.Left - rw.Left;
            }
            catch { return 0; }
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // Flags para SetWindowPos
        private const uint SWP_NOZORDER   = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOACTIVATE = 0x0010;

        // ShowWindow commands
        private const int SW_RESTORE  = 9;
        private const int SW_MINIMIZE = 6;

        // Armazena posição/tamanho antes de minimizar para restaurar depois
        private static readonly Dictionary<IntPtr, RECT> _savedRects = new();

        // ── Posições disponíveis ──────────────────────────────────────────────

        public enum WindowPosition
        {
            // Metades
            EsquerdaMetade,
            DireitaMetade,
            CimaMetade,
            BaixoMetade,

            // Quartos (cantos)
            CantoCimaEsquerda,
            CantoCimaDireita,
            CantoBaixoEsquerda,
            CantoBaixoDireita,

            // Terços horizontais
            TercoEsquerda,
            TercoCentro,
            TercoDireita,

            // Grid 3x2 (6 zonas) — linha de cima
            Grid6CimaCol1,
            Grid6CimaCol2,
            Grid6CimaCol3,

            // Grid 3x2 (6 zonas) — linha de baixo
            Grid6BaixoCol1,
            Grid6BaixoCol2,
            Grid6BaixoCol3,

            // Grid 4x2 (8 zonas) — linha de cima
            Grid8CimaCol1,
            Grid8CimaCol2,
            Grid8CimaCol3,
            Grid8CimaCol4,

            // Grid 4x2 (8 zonas) — linha de baixo
            Grid8BaixoCol1,
            Grid8BaixoCol2,
            Grid8BaixoCol3,
            Grid8BaixoCol4,

            // Zonas personalizadas 2x2 (960x772 / 960x278)
            Custom4CimaEsquerda,
            Custom4CimaDireita,
            Custom4BaixoEsquerda,
            Custom4BaixoDireita,

            // Shopping — coluna central 312x1050
            Shopping,

            // Centro (sem redimensionar)
            CentrarNaTela,

            // Maximizar no monitor atual
            Maximizar,
        }

        // ── API pública ───────────────────────────────────────────────────────

        /// <summary>
        /// Move e redimensiona a janela de um processo para a posição desejada
        /// no monitor onde o Tunetoon está aberto (ou no monitor primário).
        /// </summary>
        /// <param name="hWnd">Handle da janela do jogo (Process.MainWindowHandle).</param>
        /// <param name="position">Posição desejada.</param>
        /// <param name="targetScreen">
        ///   Monitor alvo. Null = monitor primário.
        ///   Passe Screen.FromHandle(tunetoonForm.Handle) para usar o monitor do launcher.
        /// </param>
        public static void SnapWindow(IntPtr hWnd, WindowPosition position, Screen targetScreen = null)
        {
            if (hWnd == IntPtr.Zero) return;

            // Restaura a janela caso esteja minimizada/maximizada
            ShowWindow(hWnd, SW_RESTORE);

            Screen screen = targetScreen ?? Screen.PrimaryScreen;
            Rectangle wa = screen.WorkingArea; // respeita a barra de tarefas
            int fullH = screen.Bounds.Height;  // altura total para proporções

            Rectangle dest = CalculateRect(position, wa, fullH);

            // Compensa a borda invisível do Windows 10/11
            int border = GetInvisibleBorder(hWnd);
            SetWindowPos(
                hWnd,
                IntPtr.Zero,
                dest.X - border,
                dest.Y,
                dest.Width + border * 2,
                dest.Height + border,
                SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
        }

        /// <summary>
        /// Distribui automaticamente várias janelas em um grid (2x1, 2x2, 3x2…)
        /// de acordo com a quantidade de handles fornecidos.
        /// </summary>
        /// <param name="handles">Lista de handles (um por instância de jogo).</param>
        /// <param name="targetScreen">Monitor alvo. Null = monitor primário.</param>
        public static void AutoGrid(IList<IntPtr> handles, Screen targetScreen = null)
        {
            if (handles == null || handles.Count == 0) return;

            Screen screen = targetScreen ?? Screen.PrimaryScreen;
            Rectangle wa  = screen.WorkingArea;

            int count  = handles.Count;
            int cols   = (int)Math.Ceiling(Math.Sqrt(count));
            int rows   = (int)Math.Ceiling((double)count / cols);

            int cellW  = wa.Width  / cols;
            int cellH  = wa.Height / rows;

            for (int i = 0; i < count; i++)
            {
                IntPtr hWnd = handles[i];
                if (hWnd == IntPtr.Zero) continue;

                ShowWindow(hWnd, SW_RESTORE);

                int col = i % cols;
                int row = i / cols;

                int x = wa.X + col * cellW;
                int y = wa.Y + row * cellH;

                SetWindowPos(
                    hWnd,
                    IntPtr.Zero,
                    x, y, cellW, cellH,
                    SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
            }
        }

        /// <summary>
        /// Move uma janela para coordenadas e tamanho exatos (posicionamento livre).
        /// </summary>
        public static void SnapCustom(IntPtr hWnd, int x, int y, int width, int height)
        {
            if (hWnd == IntPtr.Zero) return;
            ShowWindow(hWnd, SW_RESTORE);
            SetWindowPos(
                hWnd,
                IntPtr.Zero,
                x, y, width, height,
                SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
        }

        // ── Minimizar / Restaurar ─────────────────────────────────────────────

        /// <summary>
        /// Minimiza todas as janelas do jogo, salvando a posição atual de cada uma.
        /// </summary>
        public static void MinimizeAll(IList<IntPtr> handles)
        {
            _savedRects.Clear();
            foreach (var hWnd in handles)
            {
                if (hWnd == IntPtr.Zero) continue;
                // Salva posição atual antes de minimizar
                if (GetWindowRect(hWnd, out RECT r))
                    _savedRects[hWnd] = r;
                ShowWindow(hWnd, SW_MINIMIZE);
            }
        }

        /// <summary>
        /// Restaura todas as janelas do jogo para a posição em que estavam antes de minimizar.
        /// Janelas que já estavam minimizadas antes do MinimizeAll são apenas restauradas
        /// sem reposicionamento, evitando o efeito ghost.
        /// </summary>
        public static void RestoreAll(IList<IntPtr> handles)
        {
            foreach (var hWnd in handles)
            {
                if (hWnd == IntPtr.Zero) continue;

                if (_savedRects.TryGetValue(hWnd, out RECT r))
                {
                    // Janela foi minimizada pelo MinimizeAll — restaura para posição salva
                    ShowWindow(hWnd, SW_RESTORE);
                    SetWindowPos(
                        hWnd, IntPtr.Zero,
                        r.Left, r.Top,
                        r.Right - r.Left, r.Bottom - r.Top,
                        SWP_NOZORDER | SWP_SHOWWINDOW | SWP_NOACTIVATE);
                }
                else
                {
                    // Janela já estava minimizada antes do MinimizeAll
                    // Só restaura sem reposicionar para evitar ghost
                    ShowWindow(hWnd, SW_RESTORE);
                }
            }
        }

        // ── Cálculo de retângulos ─────────────────────────────────────────────

        private static Rectangle CalculateRect(WindowPosition pos, Rectangle wa, int fullH = 0)
        {
            int x = wa.X,  y  = wa.Y;
            int w = wa.Width, h = wa.Height;
            if (fullH == 0) fullH = h;
            int hw = w / 2, hh = h / 2;
            int tw = w / 3;

            return pos switch
            {
                WindowPosition.EsquerdaMetade       => new Rectangle(x,          y,      hw,  h),
                WindowPosition.DireitaMetade         => new Rectangle(x + hw,     y,      hw,  h),
                WindowPosition.CimaMetade            => new Rectangle(x,          y,      w,   hh),
                WindowPosition.BaixoMetade           => new Rectangle(x,          y + hh, w,   hh),

                WindowPosition.CantoCimaEsquerda     => new Rectangle(x,          y,      hw,  hh),
                WindowPosition.CantoCimaDireita      => new Rectangle(x + hw,     y,      hw,  hh),
                WindowPosition.CantoBaixoEsquerda    => new Rectangle(x,          y + hh, hw,  hh),
                WindowPosition.CantoBaixoDireita     => new Rectangle(x + hw,     y + hh, hw,  hh),

                WindowPosition.TercoEsquerda         => new Rectangle(x,          y,      tw,  h),
                WindowPosition.TercoCentro           => new Rectangle(x + tw,     y,      tw,  h),
                WindowPosition.TercoDireita          => new Rectangle(x + tw * 2, y,      tw,  h),

                // Grid 3x2 — proporções de 1920x1080 (640x424 por célula)
                WindowPosition.Grid6CimaCol1         => new Rectangle(x,               y,                       w/3,     (int)(fullH*0.393)),
                WindowPosition.Grid6CimaCol2         => new Rectangle(x + w/3,         y,                       w/3,     (int)(fullH*0.393)),
                WindowPosition.Grid6CimaCol3         => new Rectangle(x + w/3*2,       y,                       w/3,     (int)(fullH*0.393)),
                WindowPosition.Grid6BaixoCol1        => new Rectangle(x,               y+(int)(fullH*0.393),    w/3,     (int)(fullH*0.393)),
                WindowPosition.Grid6BaixoCol2        => new Rectangle(x + w/3,         y+(int)(fullH*0.393),    w/3,     (int)(fullH*0.393)),
                WindowPosition.Grid6BaixoCol3        => new Rectangle(x + w/3*2,       y+(int)(fullH*0.393),    w/3,     (int)(fullH*0.393)),

                // Grid 4x2 — proporções de 1920x1080 (480x423 por célula)
                WindowPosition.Grid8CimaCol1         => new Rectangle(x,               y,                       w/4,     (int)(fullH*0.392)),
                WindowPosition.Grid8CimaCol2         => new Rectangle(x + w/4,         y,                       w/4,     (int)(fullH*0.392)),
                WindowPosition.Grid8CimaCol3         => new Rectangle(x + w/4*2,       y,                       w/4,     (int)(fullH*0.392)),
                WindowPosition.Grid8CimaCol4         => new Rectangle(x + w/4*3,       y,                       w/4,     (int)(fullH*0.392)),
                WindowPosition.Grid8BaixoCol1        => new Rectangle(x,               y+(int)(fullH*0.392),    w/4,     (int)(fullH*0.392)),
                WindowPosition.Grid8BaixoCol2        => new Rectangle(x + w/4,         y+(int)(fullH*0.392),    w/4,     (int)(fullH*0.392)),
                WindowPosition.Grid8BaixoCol3        => new Rectangle(x + w/4*2,       y+(int)(fullH*0.392),    w/4,     (int)(fullH*0.392)),
                WindowPosition.Grid8BaixoCol4        => new Rectangle(x + w/4*3,       y+(int)(fullH*0.392),    w/4,     (int)(fullH*0.392)),

                // Custom4 — proporções de 1920x1080 (960x772 cima / 960x278 baixo)
                WindowPosition.Custom4CimaEsquerda   => new Rectangle(x,               y,                       w/2,     (int)(fullH*0.715)),
                WindowPosition.Custom4CimaDireita    => new Rectangle(x + w/2,         y,                       w/2,     (int)(fullH*0.715)),
                WindowPosition.Custom4BaixoEsquerda  => new Rectangle(x,               y+(int)(fullH*0.715),    w/2,     (int)(fullH*0.257)),
                WindowPosition.Custom4BaixoDireita   => new Rectangle(x + w/2,         y+(int)(fullH*0.715),    w/2,     (int)(fullH*0.257)),

                // Shopping — proporções de 1920x1080 (312x1050, centralizado)
                WindowPosition.Shopping              => new Rectangle(x+(int)(w*0.419),y,                       (int)(w*0.1625),(int)(fullH*0.972)),

                WindowPosition.CentrarNaTela         => new Rectangle(
                                                            x + (w - 800) / 2,
                                                            y + (h - 600) / 2,
                                                            800, 600),

                WindowPosition.Maximizar             => new Rectangle(x, y, w, h),

                _ => new Rectangle(x, y, w, h)
            };
        }
    }
}
