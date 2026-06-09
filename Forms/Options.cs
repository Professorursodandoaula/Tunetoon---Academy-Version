using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace Tunetoon.Forms
{
    public partial class Options : Form
    {
        private Launcher launcherWnd;
        private Config config;
        private bool endSelectionChecked;
        private bool canShowMasterPasswordForm = false;

        // Caminho do settings.json do Multicontroller
        private static readonly string McSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TTMulticontroller", "settings.json");

        public Options(Launcher launcherWnd, Config config)
        {
            this.launcherWnd = launcherWnd;
            this.config = config;
            InitializeComponent();
        }

        private void Options_Load(object sender, EventArgs e)
        {
            RewrittenPath.Text = config.RewrittenPath;
            ClashPath.Text     = config.ClashPath;

            SkipUpdatesCheckBox.Checked = config.SkipUpdates;
            SelectionCheckBox.Checked   = endSelectionChecked = config.SelectEndGames;
            GlobalEndCheckBox.Checked   = config.GlobalEndAll;
            EncryptAccsCheckBox.Checked = config.EncryptAccounts;

            canShowMasterPasswordForm = true;

            // TextBoxes capturam tecla via KeyDown — sem necessidade de populate

            LoadMcSettings();
        }

        // ── Hotkey combos ────────────────────────────────────────────────────

        // ── Captura de tecla ─────────────────────────────────────────────────

        // Armazena o keyCode capturado por TextBox
        private readonly System.Collections.Generic.Dictionary<System.Windows.Forms.TextBox, int>
            _capturedKeyCodes = new();

        private void HotkeyBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            var tb = (System.Windows.Forms.TextBox)sender;
            var key = e.KeyCode;
            // Ignora teclas modificadoras sozinhas
            if (key == System.Windows.Forms.Keys.ControlKey ||
                key == System.Windows.Forms.Keys.ShiftKey   ||
                key == System.Windows.Forms.Keys.Menu       ||
                key == System.Windows.Forms.Keys.LWin       ||
                key == System.Windows.Forms.Keys.RWin)
                return;
            _capturedKeyCodes[tb] = (int)key;
            tb.Text = key.ToString();
        }

        private void SetHotkeyBox(System.Windows.Forms.TextBox tb, int keyCode)
        {
            if (keyCode == 0)
            {
                tb.Text = "None";
                _capturedKeyCodes[tb] = 0;
                return;
            }
            var key = (System.Windows.Forms.Keys)keyCode;
            tb.Text = key.ToString();
            _capturedKeyCodes[tb] = keyCode;
        }

        private int GetHotkeyCode(System.Windows.Forms.TextBox tb)
        {
            return _capturedKeyCodes.TryGetValue(tb, out int code) ? code : 0;
        }

        // ── Leitura/escrita do settings.json do Multicontroller ──────────────

        private void LoadMcSettings()
        {
            try
            {
                if (!File.Exists(McSettingsPath)) return;
                var json = File.ReadAllText(McSettingsPath);
                var doc  = JsonDocument.Parse(json);
                var root = doc.RootElement;

                SetHotkeyBox(cmbActivate,   GetInt(root, "modeKeyCode"));
                SetHotkeyBox(cmbMirror,     GetInt(root, "mirrorModeKeyCode"));
                SetHotkeyBox(cmbGroup,      GetInt(root, "groupModeKeyCode"));
                SetHotkeyBox(cmbMouse,      GetInt(root, "replicateMouseKeyCode"));
                SetHotkeyBox(cmbAllGroups,  GetInt(root, "controlAllGroupsKeyCode"));
                SetHotkeyBox(cmbMouseToggle,GetInt(root, "replicateMouseKeyCode"));
                if (root.TryGetProperty("replicateMouse", out var rm))
                    chkReplicateMouse.Checked = rm.GetBoolean();
            }
            catch { }
        }

        private void SaveMcSettings()
        {
            try
            {
                // Lê o JSON existente para não apagar outras configurações
                Dictionary<string, object> settings;
                if (File.Exists(McSettingsPath))
                {
                    var json = File.ReadAllText(McSettingsPath);
                    settings = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                               ?? new Dictionary<string, object>();
                }
                else
                {
                    settings = new Dictionary<string, object>();
                }

                // Atualiza hotkeys e mouse replication
                settings["modeKeyCode"]             = GetHotkeyCode(cmbActivate);
                settings["mirrorModeKeyCode"]       = GetHotkeyCode(cmbMirror);
                settings["groupModeKeyCode"]        = GetHotkeyCode(cmbGroup);
                settings["replicateMouseKeyCode"]   = GetHotkeyCode(cmbMouseToggle);
                settings["controlAllGroupsKeyCode"] = GetHotkeyCode(cmbAllGroups);
                settings["replicateMouse"]          = chkReplicateMouse.Checked;

                Directory.CreateDirectory(Path.GetDirectoryName(McSettingsPath)!);
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(McSettingsPath, JsonSerializer.Serialize(settings, options));
            }
            catch { }
        }

        private static int GetInt(JsonElement root, string key)
        {
            if (root.TryGetProperty(key, out var prop) && prop.TryGetInt32(out int val))
                return val;
            return 0;
        }

        // ── Handlers existentes ───────────────────────────────────────────────

        private void RewrittenPathButton_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "Select Toontown Rewritten's folder.";
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                RewrittenPath.Text     = fbd.SelectedPath + "\\";
                RewrittenPath.ReadOnly = true;
            }
        }

        private void ClashPathButton_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "Select Corporate Clash's folder.";
            fbd.ShowDialog();
            if (fbd.SelectedPath != string.Empty)
            {
                ClashPath.Text     = fbd.SelectedPath + "\\";
                ClashPath.ReadOnly = true;
            }
        }

        private void OkayButton_Click(object sender, EventArgs e)
        {
            config.RewrittenPath    = RewrittenPath.Text;
            config.ClashPath        = ClashPath.Text;
            config.SkipUpdates      = SkipUpdatesCheckBox.Checked;
            config.SelectEndGames   = SelectionCheckBox.Checked;
            config.GlobalEndAll     = GlobalEndCheckBox.Checked;
            config.EncryptAccounts  = EncryptAccsCheckBox.Checked;

            if (endSelectionChecked != config.SelectEndGames)
                launcherWnd.SelectionOptionAltered();

            // Salva hotkeys do Multicontroller
            SaveMcSettings();

            Dispose();
        }

        private void masterPasswordFormCancelled()
        {
            EncryptAccsCheckBox.Checked = false;
        }

        private void EncryptAccsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (canShowMasterPasswordForm && EncryptAccsCheckBox.Checked)
            {
                MasterPasswordRegister passwordRegister = new MasterPasswordRegister();
                passwordRegister.formCancelled += masterPasswordFormCancelled;
                passwordRegister.ShowDialog();
            }
        }
    }
}
