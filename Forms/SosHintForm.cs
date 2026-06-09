using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace Tunetoon.Forms
{
    public class SosHintForm : Form
    {
        private static readonly string PrefPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Tunetoon", "sos_hint.json");

        private System.Windows.Forms.Timer _countdown;
        private int _secondsLeft = 12;
        private Button _okBtn;
        private CheckBox _dontShow;

        private static readonly string _b64 = "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";

        public static bool ShouldShow()
        {
            try
            {
                if (!File.Exists(PrefPath)) return true;
                var json = File.ReadAllText(PrefPath);
                var doc  = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("dontShow", out var v))
                    return !v.GetBoolean();
            }
            catch { }
            return true;
        }

        public static void ShowHint(Screen targetScreen = null)
        {
            var frm = new SosHintForm();
            var screen = targetScreen ?? Screen.PrimaryScreen;
            var wa = screen.WorkingArea;
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = new Point(
                wa.X + (wa.Width  - frm.Width)  / 2,
                wa.Y + (wa.Height - frm.Height) / 2);
            frm.Show();
        }

        public SosHintForm()
        {
            Text            = "Check SOS";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = false;
            Font            = new Font("Segoe UI", 9f);

            var picBox = new PictureBox
            {
                Image    = Image.FromStream(new MemoryStream(Convert.FromBase64String(_b64))),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size     = new Size(48, 48),
                Location = new Point(8, 10),
            };

            var lbl = new Label
            {
                Text     = "For Check SOS to work correctly:" + Environment.NewLine + Environment.NewLine +
                           "• At least one window must be in the Shopping layout." + Environment.NewLine + Environment.NewLine +
                           "• That window must not be overlapped by other windows." + Environment.NewLine + Environment.NewLine +
                           "TIP: After confirming the NPC, press Ctrl+V in the game chat.",
                Location = new Point(64, 8),
                Size     = new Size(330, 110),
                Font     = new Font("Segoe UI", 9f),
            };

            _dontShow = new CheckBox
            {
                Text     = "Don't show this message again",
                Location = new Point(64, 122),
                AutoSize = true,
                Font     = new Font("Segoe UI", 9f),
            };

            _okBtn = new Button
            {
                Text     = "OK (12)",
                Location = new Point(320, 118),
                Size     = new Size(76, 26),
            };
            _okBtn.Click += OkBtn_Click;

            AcceptButton = _okBtn;
            Controls.AddRange(new Control[] { picBox, lbl, _dontShow, _okBtn });
            ClientSize = new Size(408, 158);

            _countdown = new System.Windows.Forms.Timer { Interval = 1000 };
            _countdown.Tick += Countdown_Tick;
            _countdown.Start();
        }

        private void Countdown_Tick(object sender, EventArgs e)
        {
            _secondsLeft--;
            _okBtn.Text = _secondsLeft > 0 ? $"OK ({_secondsLeft})" : "OK";
            if (_secondsLeft <= 0)
                CloseAndSave();
        }

        private void OkBtn_Click(object sender, EventArgs e) => CloseAndSave();

        private void CloseAndSave()
        {
            _countdown.Stop();
            if (_dontShow.Checked)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(PrefPath)!);
                    File.WriteAllText(PrefPath,
                        JsonSerializer.Serialize(new { dontShow = true }));
                }
                catch { }
            }
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _countdown.Stop();
            base.OnFormClosing(e);
        }
    }
}
