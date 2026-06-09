using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tunetoon.Forms
{
    public class WindowSnapForm : Form
    {
        private readonly Func<IList<IntPtr>> _getHandles;
        private readonly Launcher _launcher;

        public WindowSnapForm(Func<IList<IntPtr>> getHandlesFunc, Launcher launcher = null)
        {
            _getHandles = getHandlesFunc ?? throw new ArgumentNullException(nameof(getHandlesFunc));
            _launcher = launcher;
            BuildUI();
        }

        private void BuildUI()
        {
            Text            = "Posicionamento de Janelas";
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition   = FormStartPosition.CenterParent;
            AutoSize        = true;
            AutoSizeMode    = AutoSizeMode.GrowAndShrink;
            Padding         = new Padding(10);
            BackColor       = Color.FromArgb(30, 30, 30);
            ForeColor       = Color.White;
            Font            = new Font("Segoe UI", 9f);

            var layout = new FlowLayoutPanel
            {
                FlowDirection  = FlowDirection.TopDown,
                AutoSize       = true,
                AutoSizeMode   = AutoSizeMode.GrowAndShrink,
                WrapContents   = false,
                BackColor      = Color.Transparent,
                Padding        = new Padding(0),
            };

            // Seletor de instância
            var lblInst = MakeLabel("Aplicar em:");
            var cboInst = new ComboBox
            {
                Width         = 280,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor     = Color.FromArgb(45, 45, 45),
                ForeColor     = Color.White,
                FlatStyle     = FlatStyle.Flat,
            };
            cboInst.Items.Add("Todas as instâncias");
            cboInst.SelectedIndex = 0;

            var btnRefresh = MakeButton("↻ Atualizar lista", 100, Color.FromArgb(60, 60, 80));
            btnRefresh.Click += (_, __) => RefreshInstances(cboInst);

            var rowInst = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, BackColor = Color.Transparent };
            rowInst.Controls.Add(cboInst);
            rowInst.Controls.Add(btnRefresh);

            // Seletor de monitor
            var lblMon = MakeLabel("Monitor:");
            var cboMon = new ComboBox
            {
                Width         = 280,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor     = Color.FromArgb(45, 45, 45),
                ForeColor     = Color.White,
                FlatStyle     = FlatStyle.Flat,
            };
            int idx = 1;
            foreach (Screen s in Screen.AllScreens)
                cboMon.Items.Add($"Monitor {idx++}: {s.Bounds.Width}x{s.Bounds.Height}" + (s.Primary ? " (primário)" : ""));
            cboMon.SelectedIndex = 0;

            // Grid de posições
            var lblPos = MakeLabel("Posição:");
            var grid = new TableLayoutPanel
            {
                ColumnCount = 3, RowCount = 4,
                AutoSize = true, BackColor = Color.Transparent,
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));

            grid.Controls.Add(SnapBtn("◧ Esq. ½",   WindowManager.WindowPosition.EsquerdaMetade,    cboInst, cboMon), 0, 0);
            grid.Controls.Add(SnapBtn("◨ Dir. ½",   WindowManager.WindowPosition.DireitaMetade,     cboInst, cboMon), 1, 0);
            grid.Controls.Add(SnapBtn("▣ Máx.",     WindowManager.WindowPosition.Maximizar,          cboInst, cboMon), 2, 0);
            grid.Controls.Add(SnapBtn("⬆ Cima ½",  WindowManager.WindowPosition.CimaMetade,         cboInst, cboMon), 0, 1);
            grid.Controls.Add(SnapBtn("⬇ Baixo ½", WindowManager.WindowPosition.BaixoMetade,        cboInst, cboMon), 1, 1);
            grid.Controls.Add(SnapBtn("⊕ Centro",   WindowManager.WindowPosition.CentrarNaTela,      cboInst, cboMon), 2, 1);
            grid.Controls.Add(SnapBtn("↖ C.Cima E", WindowManager.WindowPosition.CantoCimaEsquerda,  cboInst, cboMon), 0, 2);
            grid.Controls.Add(SnapBtn("↗ C.Cima D", WindowManager.WindowPosition.CantoCimaDireita,   cboInst, cboMon), 1, 2);
            grid.Controls.Add(SnapBtn("⊞ AutoGrid", null,                                            cboInst, cboMon, autoGrid: true), 2, 2);
            grid.Controls.Add(SnapBtn("↙ C.Baixo E",WindowManager.WindowPosition.CantoBaixoEsquerda, cboInst, cboMon), 0, 3);
            grid.Controls.Add(SnapBtn("↘ C.Baixo D",WindowManager.WindowPosition.CantoBaixoDireita,  cboInst, cboMon), 1, 3);

            // Terços
            var lblThirds = MakeLabel("Terços horizontais:");
            var rowThirds = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, BackColor = Color.Transparent };
            rowThirds.Controls.Add(SnapBtn("⅓ Esq.",   WindowManager.WindowPosition.TercoEsquerda, cboInst, cboMon));
            rowThirds.Controls.Add(SnapBtn("⅓ Centro", WindowManager.WindowPosition.TercoCentro,  cboInst, cboMon));
            rowThirds.Controls.Add(SnapBtn("⅓ Dir.",   WindowManager.WindowPosition.TercoDireita,  cboInst, cboMon));

            // Layout padrão
            var lblDefault = MakeLabel("Layout automático ao abrir o jogo:");
            var rowDefault = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, BackColor = Color.Transparent };

            var cboDefault = new ComboBox
            {
                Width = 180, DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            };
            cboDefault.Items.Add("Desativado");
            cboDefault.Items.Add("AutoGrid");
            cboDefault.Items.Add("Esq. ½");
            cboDefault.Items.Add("Dir. ½");
            cboDefault.Items.Add("Cima ½");
            cboDefault.Items.Add("Baixo ½");
            cboDefault.Items.Add("Canto Cima Esq.");
            cboDefault.Items.Add("Canto Cima Dir.");
            cboDefault.Items.Add("Canto Baixo Esq.");
            cboDefault.Items.Add("Canto Baixo Dir.");
            cboDefault.Items.Add("⅓ Esq.");
            cboDefault.Items.Add("⅓ Centro");
            cboDefault.Items.Add("⅓ Dir.");
            cboDefault.Items.Add("Maximizar");

            // Reflete o estado atual
            if (_launcher != null)
            {
                if (_launcher.AutoSnapGrid) cboDefault.SelectedIndex = 1;
                else if (_launcher.AutoSnapLayout.HasValue)
                    cboDefault.SelectedIndex = (int)_launcher.AutoSnapLayout.Value + 2;
                else cboDefault.SelectedIndex = 0;
            }
            else cboDefault.SelectedIndex = 0;

            var btnSaveDefault = MakeButton("Salvar", 70, Color.FromArgb(50, 100, 50));
            btnSaveDefault.Click += (_, __) =>
            {
                if (_launcher == null) return;
                int sel = cboDefault.SelectedIndex;
                if (sel == 0) { _launcher.AutoSnapLayout = null; _launcher.AutoSnapGrid = false; }
                else if (sel == 1) { _launcher.AutoSnapGrid = true; _launcher.AutoSnapLayout = null; }
                else { _launcher.AutoSnapGrid = false; _launcher.AutoSnapLayout = (WindowManager.WindowPosition)(sel - 2); }
                MessageBox.Show("Layout padrão salvo!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            rowDefault.Controls.Add(cboDefault);
            rowDefault.Controls.Add(btnSaveDefault);

            // Posição personalizada
            var lblCustom = MakeLabel("Posição personalizada (X, Y, Largura, Altura):");
            var rowCustom = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, BackColor = Color.Transparent };
            var txtX = MakeNumBox(0, 50); var txtY = MakeNumBox(0, 50);
            var txtW = MakeNumBox(800, 60); var txtH = MakeNumBox(600, 60);
            var btnCustom = MakeButton("Aplicar", 70, Color.FromArgb(50, 80, 50));
            btnCustom.Click += (_, __) =>
            {
                int x = (int)txtX.Value, y = (int)txtY.Value, w = (int)txtW.Value, h = (int)txtH.Value;
                foreach (IntPtr hWnd in GetTargetHandles(cboInst))
                    WindowManager.SnapCustom(hWnd, x, y, w, h);
            };
            rowCustom.Controls.AddRange(new Control[] { MakeLabel("X"), txtX, MakeLabel("Y"), txtY, MakeLabel("W"), txtW, MakeLabel("H"), txtH, btnCustom });

            layout.Controls.Add(lblInst);
            layout.Controls.Add(rowInst);
            layout.Controls.Add(lblMon);
            layout.Controls.Add(cboMon);
            layout.Controls.Add(lblPos);
            layout.Controls.Add(grid);
            layout.Controls.Add(lblThirds);
            layout.Controls.Add(rowThirds);
            layout.Controls.Add(lblDefault);
            layout.Controls.Add(rowDefault);
            layout.Controls.Add(lblCustom);
            layout.Controls.Add(rowCustom);

            Controls.Add(layout);
        }

        private void RefreshInstances(ComboBox cbo)
        {
            cbo.Items.Clear();
            cbo.Items.Add("Todas as instâncias");
            IList<IntPtr> handles = _getHandles();
            for (int i = 0; i < handles.Count; i++)
                cbo.Items.Add($"Instância {i + 1}  (hWnd: 0x{handles[i]:X8})");
            cbo.SelectedIndex = 0;
        }

        private IList<IntPtr> GetTargetHandles(ComboBox cbo)
        {
            IList<IntPtr> all = _getHandles();
            if (cbo.SelectedIndex <= 0) return all;
            int i = cbo.SelectedIndex - 1;
            return i < all.Count ? new List<IntPtr> { all[i] } : new List<IntPtr>();
        }

        private Screen GetTargetScreen(ComboBox cboMon)
        {
            int i = cboMon.SelectedIndex;
            Screen[] screens = Screen.AllScreens;
            return (i >= 0 && i < screens.Length) ? screens[i] : Screen.PrimaryScreen;
        }

        private Button SnapBtn(string label, WindowManager.WindowPosition? pos, ComboBox cboInst, ComboBox cboMon, bool autoGrid = false)
        {
            var btn = MakeButton(label, 90, Color.FromArgb(45, 70, 100));
            btn.Click += (_, __) =>
            {
                Screen screen = GetTargetScreen(cboMon);
                IList<IntPtr> handles = GetTargetHandles(cboInst);
                if (autoGrid) { WindowManager.AutoGrid(handles, screen); return; }
                if (pos.HasValue)
                    foreach (IntPtr hWnd in handles)
                        WindowManager.SnapWindow(hWnd, pos.Value, screen);
            };
            return btn;
        }

        private Button SnapBtn(string label, WindowManager.WindowPosition pos, ComboBox cboInst, ComboBox cboMon)
            => SnapBtn(label, pos, cboInst, cboMon, false);

        private static Button MakeButton(string text, int width, Color back) =>
            new Button { Text = text, Width = width, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = Color.White, Margin = new Padding(2), Cursor = Cursors.Hand };

        private static Label MakeLabel(string text) =>
            new Label { Text = text, AutoSize = true, ForeColor = Color.LightGray, Margin = new Padding(2, 8, 2, 2) };

        private static NumericUpDown MakeNumBox(int value, int width) =>
            new NumericUpDown { Minimum = 0, Maximum = 9999, Value = value, Width = width, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, Margin = new Padding(2) };
    }
}
