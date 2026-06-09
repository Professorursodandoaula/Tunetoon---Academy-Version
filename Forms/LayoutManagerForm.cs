using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Tunetoon.Forms
{
    // Representa a posição de uma conta em um layout
    public class AccountSlot
    {
        public string AccountToon { get; set; }
        public int Position { get; set; } = -1;
        public int Monitor { get; set; } = 0; // 0 = monitor primário, 1 = monitor 2, etc.
    }

    // Representa um layout completo
    public class WindowLayout
    {
        public string Name { get; set; }
        public List<AccountSlot> Slots { get; set; } = new List<AccountSlot>();
        public string Hotkey { get; set; } = ""; // ex: "F1", "F2", etc.
    }

    public class LayoutManagerForm : Form
    {
        private readonly Func<Dictionary<string, (IntPtr handle, int windowSlot, int monitor)>> _getToonHandles;
        private readonly Func<IList<string>> _getToonNames;
        private readonly List<WindowLayout> _layouts;
        private const string LayoutsFile = "Layouts.json";

        private ListBox _layoutList;
        private DataGridView _slotGrid;
        private TextBox _nameBox;

        public LayoutManagerForm(
            Func<Dictionary<string, (IntPtr handle, int windowSlot, int monitor)>> getToonHandles,
            Func<IList<string>> getToonNames,
            List<WindowLayout> layouts)
        {
            _getToonHandles = getToonHandles;
            _getToonNames   = getToonNames;
            _layouts        = layouts;
            LoadLayoutsFromFile();
            BuildUI();
        }

        // ── Persistência ──────────────────────────────────────────────────────

        private void LoadLayoutsFromFile()
        {
            try
            {
                if (!File.Exists(LayoutsFile)) return;
                string json = File.ReadAllText(LayoutsFile);
                var loaded = JsonSerializer.Deserialize<List<WindowLayout>>(json);
                if (loaded != null)
                {
                    _layouts.Clear();
                    _layouts.AddRange(loaded);
                }
            }
            catch { }
        }

        private void SaveLayoutsToFile()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(LayoutsFile, JsonSerializer.Serialize(_layouts, options));
            }
            catch { }
        }

        // ── UI ───────────────────────────────────────────────────────────────

        private ComboBox _hotkeyBox;

        private void BuildUI()
        {
            Text            = "Manage Layouts";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition   = FormStartPosition.Manual;
            Size            = new Size(620, 460);
            BackColor       = Color.FromArgb(30, 30, 30);
            ForeColor       = Color.White;
            Font            = new Font("Segoe UI", 9f);
            MaximizeBox     = false;

            var lblLayouts = MakeLabel("Saved layouts:");
            lblLayouts.Location = new Point(10, 10);

            _layoutList = new ListBox
            {
                Location    = new Point(10, 30),
                Size        = new Size(170, 278),
                BackColor   = Color.FromArgb(45, 45, 45),
                ForeColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 9f),
            };
            _layoutList.SelectedIndexChanged += LayoutList_SelectedIndexChanged;

            var btnUp = MakeButton("▲", 82, Color.FromArgb(50, 50, 70));
            btnUp.Location = new Point(10, 314);
            btnUp.Click += (_, __) =>
            {
                int idx = _layoutList.SelectedIndex;
                if (idx <= 0) return;
                var tmp = _layouts[idx];
                _layouts[idx] = _layouts[idx - 1];
                _layouts[idx - 1] = tmp;
                SaveLayoutsToFile();
                RefreshLayoutList();
                _layoutList.SelectedIndex = idx - 1;
            };

            var btnDown = MakeButton("▼", 82, Color.FromArgb(50, 50, 70));
            btnDown.Location = new Point(98, 314);
            btnDown.Click += (_, __) =>
            {
                int idx = _layoutList.SelectedIndex;
                if (idx < 0 || idx >= _layouts.Count - 1) return;
                var tmp = _layouts[idx];
                _layouts[idx] = _layouts[idx + 1];
                _layouts[idx + 1] = tmp;
                SaveLayoutsToFile();
                RefreshLayoutList();
                _layoutList.SelectedIndex = idx + 1;
            };

            var btnApply = MakeButton("▶ Apply", 170, Color.FromArgb(30, 100, 30));
            btnApply.Location = new Point(10, 348);
            btnApply.Click += BtnApply_Click;

            var btnDelete = MakeButton("✕ Delete", 170, Color.FromArgb(100, 30, 30));
            btnDelete.Location = new Point(10, 380);
            btnDelete.Click += BtnDelete_Click;

            var lblName = MakeLabel("Layout name:");
            lblName.Location = new Point(195, 10);

            _nameBox = new TextBox
            {
                Location    = new Point(195, 30),
                Size        = new Size(130, 22),
                BackColor   = Color.FromArgb(45, 45, 45),
                ForeColor   = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 9f),
            };

            var lblHotkey = MakeLabel("Hotkey:");
            lblHotkey.Location = new Point(335, 10);

            _hotkeyBox = new ComboBox
            {
                Location      = new Point(335, 30),
                Size          = new Size(80, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor     = Color.FromArgb(45, 45, 45),
                ForeColor     = Color.White,
                FlatStyle     = FlatStyle.Flat,
                Font          = new Font("Segoe UI", 9f),
            };
            _hotkeyBox.Items.Add("None");
            for (int i = 1; i <= 12; i++)
                _hotkeyBox.Items.Add($"F{i}");
            _hotkeyBox.SelectedIndex = 0;

            var lblSlots = MakeLabel("Position of each account:");
            lblSlots.Location = new Point(195, 62);

            _slotGrid = new DataGridView
            {
                Location              = new Point(195, 82),
                Size                  = new Size(400, 258),
                BackgroundColor       = Color.FromArgb(45, 45, 45),
                ForeColor             = Color.White,
                GridColor             = Color.FromArgb(60, 60, 60),
                BorderStyle           = BorderStyle.None,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible     = false,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            };

            _slotGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Account",
                Name       = "Toon",
                ReadOnly   = true,
            });

            var monCol = new DataGridViewComboBoxColumn
            {
                HeaderText = "Monitor",
                Name       = "Monitor",
                FlatStyle  = FlatStyle.Flat,
                Width      = 90,
            };
            for (int i = 0; i < Screen.AllScreens.Length; i++)
                monCol.Items.Add($"Monitor {i + 1}");
            _slotGrid.Columns.Add(monCol);

            var posCol = new DataGridViewComboBoxColumn
            {
                HeaderText = "Position",
                Name       = "Position",
                FlatStyle  = FlatStyle.Flat,
            };
            posCol.Items.Add("No position");
            foreach (var name in AccountEdit.SlotNames.Skip(1))
                posCol.Items.Add(name);
            _slotGrid.Columns.Add(posCol);

            _slotGrid.DefaultCellStyle.BackColor              = Color.FromArgb(45, 45, 45);
            _slotGrid.DefaultCellStyle.ForeColor              = Color.White;
            _slotGrid.DefaultCellStyle.SelectionBackColor     = Color.FromArgb(60, 90, 130);
            _slotGrid.DefaultCellStyle.SelectionForeColor     = Color.White;
            _slotGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            _slotGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _slotGrid.EnableHeadersVisualStyles               = false;

            var btnNew = MakeButton("+ New layout", 130, Color.FromArgb(45, 70, 100));
            btnNew.Location = new Point(195, 348);
            btnNew.Click += BtnNew_Click;

            var btnSave = MakeButton("💾 Save layout", 150, Color.FromArgb(50, 100, 50));
            btnSave.Location = new Point(335, 348);
            btnSave.Click += BtnSave_Click;

            var btnRefresh = MakeButton("↻ Refresh accounts", 180, Color.FromArgb(60, 60, 80));
            btnRefresh.Location = new Point(195, 380);
            btnRefresh.Click += (_, __) => RefreshSlotGrid(_layoutList.SelectedIndex >= 0 ? _layouts[_layoutList.SelectedIndex] : null);

            Controls.AddRange(new Control[]
            {
                lblLayouts, _layoutList, btnUp, btnDown, btnApply, btnDelete,
                lblName, _nameBox, lblHotkey, _hotkeyBox, lblSlots, _slotGrid,
                btnNew, btnSave, btnRefresh
            });

            RefreshLayoutList();
            RefreshSlotGrid(null);

            // Posiciona colada à esquerda do Launcher (Owner), mesma altura
            this.Shown += (s, ev) =>
            {
                if (Owner != null)
                {
                    var screen = Screen.FromControl(Owner);
                    int x = Owner.Left - this.Width;
                    if (x < screen.WorkingArea.Left)
                        x = Owner.Right;
                    this.Location = new Point(x, Owner.Top);
                }
            };
        }

        private void RefreshLayoutList()
        {
            _layoutList.Items.Clear();
            foreach (var layout in _layouts)
                _layoutList.Items.Add(layout.Name);
        }

        private void RefreshSlotGrid(WindowLayout layout)
        {
            _slotGrid.Rows.Clear();
            var toons = _getToonNames();

            foreach (var toon in toons)
            {
                int rowIdx = _slotGrid.Rows.Add();
                _slotGrid.Rows[rowIdx].Cells["Toon"].Value = toon;

                string pos = "No position";
                string mon = "Monitor 1";

                if (layout != null)
                {
                    var slot = layout.Slots.FirstOrDefault(s => s.AccountToon == toon);
                    if (slot != null)
                    {
                        if (slot.Position >= 0 && slot.Position < AccountEdit.SlotNames.Length - 1)
                            pos = AccountEdit.SlotNames[slot.Position + 1];
                        int monIdx = slot.Monitor;
                        if (monIdx >= 0 && monIdx < Screen.AllScreens.Length)
                            mon = $"Monitor {monIdx + 1}";
                    }
                }

                _slotGrid.Rows[rowIdx].Cells["Position"].Value = pos;
                _slotGrid.Rows[rowIdx].Cells["Monitor"].Value  = mon;
            }
        }

        private void LayoutList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = _layoutList.SelectedIndex;
            if (idx < 0 || idx >= _layouts.Count) return;
            var layout = _layouts[idx];
            _nameBox.Text = layout.Name;
            RefreshSlotGrid(layout);

            // Carrega hotkey
            string hk = layout.Hotkey ?? "";
            int hkIdx = _hotkeyBox.Items.IndexOf(hk);
            _hotkeyBox.SelectedIndex = hkIdx >= 0 ? hkIdx : 0;
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            _nameBox.Text = "New Layout";
            _layoutList.SelectedIndex = -1;
            RefreshSlotGrid(null);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string name = _nameBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Enter a name for the layout.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var layout = _layouts.FirstOrDefault(l => l.Name == name);
            if (layout == null)
            {
                layout = new WindowLayout { Name = name };
                _layouts.Add(layout);
            }

            layout.Slots.Clear();
            foreach (DataGridViewRow row in _slotGrid.Rows)
            {
                string toon    = row.Cells["Toon"].Value?.ToString();
                string posName = row.Cells["Position"].Value?.ToString() ?? "No position";
                string monName = row.Cells["Monitor"].Value?.ToString() ?? "Monitor 1";
                int posIdx     = Array.IndexOf(AccountEdit.SlotNames, posName) - 1;
                int monIdx     = 0;
                if (monName.StartsWith("Monitor ") && int.TryParse(monName.Substring(8), out int m))
                    monIdx = m - 1;

                layout.Slots.Add(new AccountSlot
                {
                    AccountToon = toon,
                    Position    = posIdx,
                    Monitor     = monIdx,
                });
            }

            // Salva hotkey
            string hk = _hotkeyBox.SelectedIndex > 0 ? _hotkeyBox.SelectedItem.ToString() : "";
            // Verifica conflito com outro layout
            foreach (var other in _layouts)
                if (other != layout && !string.IsNullOrEmpty(hk) && other.Hotkey == hk)
                    other.Hotkey = "";
            layout.Hotkey = hk;

            SaveLayoutsToFile();
            RefreshLayoutList();
            _layoutList.SelectedItem = name;
            MessageBox.Show("Layout saved!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            int idx = _layoutList.SelectedIndex;
            if (idx < 0) return;
            _layouts.RemoveAt(idx);
            SaveLayoutsToFile();
            RefreshLayoutList();
            RefreshSlotGrid(null);
            _nameBox.Text = "";
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            int idx = _layoutList.SelectedIndex;
            if (idx < 0)
            {
                MessageBox.Show("Select a layout first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var layout      = _layouts[idx];
            var toonHandles = _getToonHandles();
            var screens     = Screen.AllScreens;

            foreach (var slot in layout.Slots)
            {
                if (slot.Position < 0) continue;
                if (!toonHandles.TryGetValue(slot.AccountToon, out var entry)) continue;
                if (entry.handle == IntPtr.Zero) continue;

                var pos    = (WindowManager.WindowPosition)slot.Position;
                var screen = (slot.Monitor >= 0 && slot.Monitor < screens.Length)
                    ? screens[slot.Monitor]
                    : Screen.PrimaryScreen;

                WindowManager.SnapWindow(entry.handle, pos, screen);
            }

            // Abre o Multicontroller já com os handles distribuídos pelos grupos
            MultiController.MulticontrollerBridge.OpenWithHandles(toonHandles);
        }

        private static Button MakeButton(string text, int width, Color back) =>
            new Button
            {
                Text      = text,
                Width     = width,
                Height    = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Cursor    = Cursors.Hand,
            };

        private static Label MakeLabel(string text) =>
            new Label { Text = text, AutoSize = true, ForeColor = Color.LightGray };
    }
}
