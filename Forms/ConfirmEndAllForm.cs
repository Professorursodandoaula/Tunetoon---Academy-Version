using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Tunetoon.Forms
{
    public class ConfirmEndAllForm : Form
    {
        public static bool Show(IWin32Window owner)
        {
            using var form = new ConfirmEndAllForm();
            return form.ShowDialog(owner) == DialogResult.Yes;
        }

        private ConfirmEndAllForm()
        {
            Text            = "End All";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            MinimizeBox     = false;

            string b64 = "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";
            var picBox = new PictureBox
            {
                Image    = Image.FromStream(new MemoryStream(Convert.FromBase64String(b64))),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size     = new Size(36, 36),
                Location = new Point(8, 8),
            };

            var lbl = new Label
            {
                Text      = "Close all Toons?",
                Location  = new Point(50, 14),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 9f),
            };

            var btnYes = new Button
            {
                Text         = "Yes",
                DialogResult = DialogResult.Yes,
                Location     = new Point(50, 36),
                Size         = new Size(50, 22),
            };

            var btnNo = new Button
            {
                Text         = "No",
                DialogResult = DialogResult.No,
                Location     = new Point(108, 36),
                Size         = new Size(50, 22),
            };

            AcceptButton = btnYes;
            CancelButton = btnNo;

            Controls.AddRange(new Control[] { picBox, lbl, btnYes, btnNo });

            ClientSize = new Size(168, 66);
        }
    }
}
