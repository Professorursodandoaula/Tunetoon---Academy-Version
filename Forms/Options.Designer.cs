namespace Tunetoon.Forms
{
    partial class Options
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Options));

            // Existentes
            this.OkayButton          = new System.Windows.Forms.Button();
            this.RewrittenLabel      = new System.Windows.Forms.Label();
            this.ClashLabel          = new System.Windows.Forms.Label();
            this.SelectionCheckBox   = new System.Windows.Forms.CheckBox();
            this.GlobalEndCheckBox   = new System.Windows.Forms.CheckBox();
            this.RewrittenPath       = new System.Windows.Forms.TextBox();
            this.ClashPath           = new System.Windows.Forms.TextBox();
            this.RewrittenPathButton = new System.Windows.Forms.Button();
            this.ClashPathButton     = new System.Windows.Forms.Button();
            this.SkipUpdatesCheckBox = new System.Windows.Forms.CheckBox();
            this.EncryptAccsCheckBox = new System.Windows.Forms.CheckBox();

            // Multicontroller
            this.McGroupBox           = new System.Windows.Forms.GroupBox();
            this.lblActivate          = new System.Windows.Forms.Label();
            this.lblMirror            = new System.Windows.Forms.Label();
            this.lblGroup             = new System.Windows.Forms.Label();
            this.lblMouse             = new System.Windows.Forms.Label();
            this.lblAllGroups         = new System.Windows.Forms.Label();
            this.cmbActivate          = new System.Windows.Forms.TextBox();
            this.cmbMirror            = new System.Windows.Forms.TextBox();
            this.cmbGroup             = new System.Windows.Forms.TextBox();
            this.cmbMouse             = new System.Windows.Forms.TextBox();
            this.cmbAllGroups         = new System.Windows.Forms.TextBox();

            this.McGroupBox.SuspendLayout();
            this.MouseGroupBox    = new System.Windows.Forms.GroupBox();
            this.chkReplicateMouse = new System.Windows.Forms.CheckBox();
            this.lblMouseToggle   = new System.Windows.Forms.Label();
            this.cmbMouseToggle   = new System.Windows.Forms.TextBox();
            this.MouseGroupBox.SuspendLayout();

            // Tunetoon Shortcuts
            this.TunetoonGroupBox           = new System.Windows.Forms.GroupBox();
            this.lblShortcutMulticontroller  = new System.Windows.Forms.Label();
            this.lblShortcutMinimize         = new System.Windows.Forms.Label();
            this.TunetoonGroupBox.SuspendLayout();

            this.SuspendLayout();

            // ── OkayButton
            this.OkayButton.Location = new System.Drawing.Point(14, 540);
            this.OkayButton.Size     = new System.Drawing.Size(564, 27);
            this.OkayButton.Text     = "Save";
            this.OkayButton.Click   += new System.EventHandler(this.OkayButton_Click);

            // ── RewrittenLabel / Path / Button
            this.RewrittenLabel.Location = new System.Drawing.Point(10, 16);
            this.RewrittenLabel.AutoSize = true;
            this.RewrittenLabel.Text     = "Rewritten Path:";

            this.RewrittenPath.Location = new System.Drawing.Point(14, 35);
            this.RewrittenPath.Size     = new System.Drawing.Size(517, 23);

            this.RewrittenPathButton.Location = new System.Drawing.Point(539, 35);
            this.RewrittenPathButton.Size     = new System.Drawing.Size(38, 23);
            this.RewrittenPathButton.Text     = "...";
            this.RewrittenPathButton.Click   += new System.EventHandler(this.RewrittenPathButton_Click);

            // ── ClashLabel / Path / Button
            this.ClashLabel.Location = new System.Drawing.Point(10, 74);
            this.ClashLabel.AutoSize = true;
            this.ClashLabel.Text     = "Clash Path:";

            this.ClashPath.Location = new System.Drawing.Point(14, 92);
            this.ClashPath.Size     = new System.Drawing.Size(517, 23);

            this.ClashPathButton.Location = new System.Drawing.Point(539, 92);
            this.ClashPathButton.Size     = new System.Drawing.Size(38, 24);
            this.ClashPathButton.Text     = "...";
            this.ClashPathButton.Click   += new System.EventHandler(this.ClashPathButton_Click);

            // ── Checkboxes
            this.SkipUpdatesCheckBox.Location = new System.Drawing.Point(18, 136);
            this.SkipUpdatesCheckBox.AutoSize = true;
            this.SkipUpdatesCheckBox.Text     = "Skip game updates";

            this.EncryptAccsCheckBox.Location        = new System.Drawing.Point(18, 163);
            this.EncryptAccsCheckBox.AutoSize         = true;
            this.EncryptAccsCheckBox.Text             = "Encrypt accounts";
            this.EncryptAccsCheckBox.CheckedChanged  += new System.EventHandler(this.EncryptAccsCheckBox_CheckedChanged);

            this.SelectionCheckBox.Location = new System.Drawing.Point(398, 136);
            this.SelectionCheckBox.AutoSize = true;
            this.SelectionCheckBox.Text     = "End by selection";

            this.GlobalEndCheckBox.Location = new System.Drawing.Point(398, 163);
            this.GlobalEndCheckBox.AutoSize = true;
            this.GlobalEndCheckBox.Text     = "\"End All\" for all gameservers";

            // ── McGroupBox
            this.McGroupBox.Location = new System.Drawing.Point(14, 200);
            this.McGroupBox.Size     = new System.Drawing.Size(564, 178);
            this.McGroupBox.Text     = "Multicontroller Hotkeys";

            int lblX = 10, comboX = 200, rowH = 28, startY = 22;

            this.lblActivate.AutoSize = true;
            this.lblActivate.Location = new System.Drawing.Point(lblX, startY);
            this.lblActivate.Text     = "Activate / Cycle Mode:";

            this.lblMirror.AutoSize = true;
            this.lblMirror.Location = new System.Drawing.Point(lblX, startY + rowH);
            this.lblMirror.Text     = "Mirror Mode:";

            this.lblGroup.AutoSize = true;
            this.lblGroup.Location = new System.Drawing.Point(lblX, startY + rowH * 2);
            this.lblGroup.Text     = "Group Mode:";

            this.lblMouse.AutoSize = true;
            this.lblMouse.Location = new System.Drawing.Point(lblX, startY + rowH * 3);
            this.lblMouse.Text     = "Toggle Replicate Mouse:";

            this.lblAllGroups.AutoSize = true;
            this.lblAllGroups.Location = new System.Drawing.Point(lblX, startY + rowH * 4);
            this.lblAllGroups.Text     = "Control All Groups:";

            var comboSize = new System.Drawing.Size(150, 21);

            this.cmbActivate.Location  = new System.Drawing.Point(comboX, startY - 2);
            this.cmbActivate.Size      = comboSize;
            this.cmbActivate.ReadOnly  = true;
            this.cmbActivate.BackColor = System.Drawing.SystemColors.Window;
            this.cmbActivate.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.HotkeyBox_KeyDown);
            this.cmbActivate.GotFocus += (s, ev) => ((System.Windows.Forms.TextBox)s).Text = "(press a key...)";

            this.cmbMirror.Location  = new System.Drawing.Point(comboX, startY + rowH - 2);
            this.cmbMirror.Size      = comboSize;
            this.cmbMirror.ReadOnly  = true;
            this.cmbMirror.BackColor = System.Drawing.SystemColors.Window;
            this.cmbMirror.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.HotkeyBox_KeyDown);
            this.cmbMirror.GotFocus += (s, ev) => ((System.Windows.Forms.TextBox)s).Text = "(press a key...)";

            this.cmbGroup.Location  = new System.Drawing.Point(comboX, startY + rowH * 2 - 2);
            this.cmbGroup.Size      = comboSize;
            this.cmbGroup.ReadOnly  = true;
            this.cmbGroup.BackColor = System.Drawing.SystemColors.Window;
            this.cmbGroup.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.HotkeyBox_KeyDown);
            this.cmbGroup.GotFocus += (s, ev) => ((System.Windows.Forms.TextBox)s).Text = "(press a key...)";

            this.cmbMouse.Location  = new System.Drawing.Point(comboX, startY + rowH * 3 - 2);
            this.cmbMouse.Size      = comboSize;
            this.cmbMouse.ReadOnly  = true;
            this.cmbMouse.BackColor = System.Drawing.SystemColors.Window;
            this.cmbMouse.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.HotkeyBox_KeyDown);
            this.cmbMouse.GotFocus += (s, ev) => ((System.Windows.Forms.TextBox)s).Text = "(press a key...)";

            this.cmbAllGroups.Location  = new System.Drawing.Point(comboX, startY + rowH * 4 - 2);
            this.cmbAllGroups.Size      = comboSize;
            this.cmbAllGroups.ReadOnly  = true;
            this.cmbAllGroups.BackColor = System.Drawing.SystemColors.Window;
            this.cmbAllGroups.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.HotkeyBox_KeyDown);
            this.cmbAllGroups.GotFocus += (s, ev) => ((System.Windows.Forms.TextBox)s).Text = "(press a key...)";

            this.McGroupBox.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.lblActivate, this.cmbActivate,
                this.lblMirror,   this.cmbMirror,
                this.lblGroup,    this.cmbGroup,
                this.lblMouse,    this.cmbMouse,
                this.lblAllGroups,this.cmbAllGroups,
            });

            // ── MouseGroupBox
            this.MouseGroupBox.Location = new System.Drawing.Point(14, 388);
            this.MouseGroupBox.Size     = new System.Drawing.Size(564, 60);
            this.MouseGroupBox.Text     = "Mouse Replication";

            this.chkReplicateMouse.AutoSize = true;
            this.chkReplicateMouse.Location = new System.Drawing.Point(10, 22);
            this.chkReplicateMouse.Text     = "Enable mouse replication";

            this.lblMouseToggle.AutoSize = true;
            this.lblMouseToggle.Location = new System.Drawing.Point(210, 24);
            this.lblMouseToggle.Text     = "Toggle key:";

            this.cmbMouseToggle.Location  = new System.Drawing.Point(290, 21);
            this.cmbMouseToggle.Size      = new System.Drawing.Size(150, 21);
            this.cmbMouseToggle.ReadOnly  = true;
            this.cmbMouseToggle.BackColor = System.Drawing.SystemColors.Window;
            this.cmbMouseToggle.KeyDown  += new System.Windows.Forms.KeyEventHandler(this.HotkeyBox_KeyDown);
            this.cmbMouseToggle.GotFocus += (s, ev) => ((System.Windows.Forms.TextBox)s).Text = "(press a key...)";

            this.MouseGroupBox.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.chkReplicateMouse,
                this.lblMouseToggle,
                this.cmbMouseToggle,
            });

            this.MouseGroupBox.ResumeLayout(false);
            this.MouseGroupBox.PerformLayout();

            // ── TunetoonGroupBox (Tunetoon Shortcuts)
            this.TunetoonGroupBox.Location = new System.Drawing.Point(14, 458);
            this.TunetoonGroupBox.Size     = new System.Drawing.Size(564, 72);
            this.TunetoonGroupBox.Text     = "Tunetoon Shortcuts";

            this.lblShortcutMulticontroller.AutoSize = true;
            this.lblShortcutMulticontroller.Location = new System.Drawing.Point(10, 24);
            this.lblShortcutMulticontroller.Text     = "Open / Refresh Multicontroller:";

            var lblCtrlM = new System.Windows.Forms.Label();
            lblCtrlM.AutoSize = true;
            lblCtrlM.Location = new System.Drawing.Point(200, 24);
            lblCtrlM.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            lblCtrlM.Text     = "Ctrl + M";

            this.lblShortcutMinimize.AutoSize = true;
            this.lblShortcutMinimize.Location = new System.Drawing.Point(10, 48);
            this.lblShortcutMinimize.Text     = "Minimize / Restore All Windows:";

            var lblAltM = new System.Windows.Forms.Label();
            lblAltM.AutoSize = true;
            lblAltM.Location = new System.Drawing.Point(200, 48);
            lblAltM.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            lblAltM.Text     = "Alt + M";

            this.TunetoonGroupBox.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.lblShortcutMulticontroller, lblCtrlM,
                this.lblShortcutMinimize,        lblAltM,
            });

            this.TunetoonGroupBox.ResumeLayout(false);
            this.TunetoonGroupBox.PerformLayout();

            // ── Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(593, 580);
            this.FormBorderStyle     = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon                = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox         = false;
            this.MinimizeBox         = false;
            this.Name                = "Options";
            this.ShowInTaskbar       = false;
            this.StartPosition       = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text                = "Options";
            this.Load               += new System.EventHandler(this.Options_Load);

            this.McGroupBox.ResumeLayout(false);
            this.McGroupBox.PerformLayout();

            this.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                this.EncryptAccsCheckBox,
                this.SkipUpdatesCheckBox,
                this.ClashPathButton,
                this.RewrittenPathButton,
                this.ClashPath,
                this.RewrittenPath,
                this.GlobalEndCheckBox,
                this.SelectionCheckBox,
                this.ClashLabel,
                this.RewrittenLabel,
                this.McGroupBox,
                this.MouseGroupBox,
                this.TunetoonGroupBox,
                this.OkayButton,
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button OkayButton;
        private System.Windows.Forms.Label RewrittenLabel;
        private System.Windows.Forms.Label ClashLabel;
        private System.Windows.Forms.CheckBox SelectionCheckBox;
        private System.Windows.Forms.CheckBox GlobalEndCheckBox;
        private System.Windows.Forms.TextBox RewrittenPath;
        private System.Windows.Forms.TextBox ClashPath;
        private System.Windows.Forms.Button RewrittenPathButton;
        private System.Windows.Forms.Button ClashPathButton;
        private System.Windows.Forms.CheckBox SkipUpdatesCheckBox;
        private System.Windows.Forms.CheckBox EncryptAccsCheckBox;

        private System.Windows.Forms.GroupBox McGroupBox;
        private System.Windows.Forms.GroupBox MouseGroupBox;
        private System.Windows.Forms.GroupBox TunetoonGroupBox;
        private System.Windows.Forms.CheckBox chkReplicateMouse;
        private System.Windows.Forms.Label lblMouseToggle;
        private System.Windows.Forms.TextBox cmbMouseToggle;
        private System.Windows.Forms.Label lblActivate;
        private System.Windows.Forms.Label lblMirror;
        private System.Windows.Forms.Label lblGroup;
        private System.Windows.Forms.Label lblMouse;
        private System.Windows.Forms.Label lblAllGroups;
        private System.Windows.Forms.TextBox cmbActivate;
        private System.Windows.Forms.TextBox cmbMirror;
        private System.Windows.Forms.TextBox cmbGroup;
        private System.Windows.Forms.TextBox cmbMouse;
        private System.Windows.Forms.TextBox cmbAllGroups;
        private System.Windows.Forms.Label lblShortcutMulticontroller;
        private System.Windows.Forms.Label lblShortcutMinimize;
    }
}
