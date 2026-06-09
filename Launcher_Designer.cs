using System.Windows.Forms;
using Tunetoon.Grid;

namespace Tunetoon.Forms
{
    partial class Launcher
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcher));
            this.TopMenu            = new System.Windows.Forms.MenuStrip();
            this.serverMenuItem     = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerMenuStrip    = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.rewrittenMenuItem  = new System.Windows.Forms.ToolStripMenuItem();
            this.clashMenuItem      = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsMenuItem    = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsMenuItem    = new System.Windows.Forms.ToolStripMenuItem();
            this.sosMenuItem        = new System.Windows.Forms.ToolStripMenuItem();
            this.bossCalcMenuItem   = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeMenuItem    = new System.Windows.Forms.ToolStripMenuItem();
            this.endSelectedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.endAllMenuItem     = new System.Windows.Forms.ToolStripMenuItem();
            this.untickAllMenuItem  = new System.Windows.Forms.ToolStripMenuItem();
            this.multicontrollerMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.priorityColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LoginButton        = new System.Windows.Forms.Button();
            this.accountGrid        = new Tunetoon.Grid.AccountGrid();
            this.Login              = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Toon               = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ToonSlots          = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.End                = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.mazeGame = new Tunetoon.Forms.MazeGame();
            this.TopMenu.SuspendLayout();
            this.ServerMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.accountGrid)).BeginInit();
            this.SuspendLayout();
            // TopMenu
            this.TopMenu.AllowMerge = false;
            this.TopMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.TopMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.serverMenuItem,
            this.untickAllMenuItem,
            this.windowsMenuItem,
            this.bossCalcMenuItem,
            this.sosMenuItem,
            this.multicontrollerMenuItem,
            this.optionsMenuItem,
            this.minimizeMenuItem,
            this.endSelectedMenuItem,
            this.endAllMenuItem});
            this.TopMenu.Location = new System.Drawing.Point(0, 0);
            this.TopMenu.Name = "TopMenu";
            this.TopMenu.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.TopMenu.Size = new System.Drawing.Size(660, 24);
            this.TopMenu.TabIndex = 0;
            this.TopMenu.Text = "TopMenu";
            this.TopMenu.Click += new System.EventHandler(this.TopMenu_Click);
            // serverMenuItem
            this.serverMenuItem.DropDown = this.ServerMenuStrip;
            this.serverMenuItem.Name = "serverMenuItem";
            this.serverMenuItem.Size = new System.Drawing.Size(51, 20);
            this.serverMenuItem.Text = "Server";
            // ServerMenuStrip
            this.ServerMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rewrittenMenuItem,
            this.clashMenuItem});
            this.ServerMenuStrip.Name = "ServerMenuStrip";
            this.ServerMenuStrip.OwnerItem = this.serverMenuItem;
            this.ServerMenuStrip.Size = new System.Drawing.Size(125, 48);
            // rewrittenMenuItem
            this.rewrittenMenuItem.Checked = true;
            this.rewrittenMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rewrittenMenuItem.Name = "rewrittenMenuItem";
            this.rewrittenMenuItem.Size = new System.Drawing.Size(124, 22);
            this.rewrittenMenuItem.Text = "Rewritten";
            this.rewrittenMenuItem.Click += new System.EventHandler(this.Rewritten_Click);
            // clashMenuItem
            this.clashMenuItem.Name = "clashMenuItem";
            this.clashMenuItem.Size = new System.Drawing.Size(124, 22);
            this.clashMenuItem.Text = "Clash";
            this.clashMenuItem.Click += new System.EventHandler(this.Clash_Click);
            // optionsMenuItem
            this.optionsMenuItem.Name = "optionsMenuItem";
            this.optionsMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsMenuItem.Text = "Options";
            this.optionsMenuItem.Click += new System.EventHandler(this.Options_Click);
            // windowsMenuItem
            this.windowsMenuItem.Name = "windowsMenuItem";
            this.windowsMenuItem.Size = new System.Drawing.Size(65, 20);
            this.windowsMenuItem.Text = "Windows";
            this.windowsMenuItem.Click += new System.EventHandler(this.Windows_Click);
            // sosMenuItem
            this.sosMenuItem.Name = "sosMenuItem";
            this.sosMenuItem.Size = new System.Drawing.Size(70, 20);
            this.sosMenuItem.Text = "Check SOS";
            this.sosMenuItem.Click += new System.EventHandler(this.SosCheck_Click);
            // bossCalcMenuItem
            this.bossCalcMenuItem.Name = "bossCalcMenuItem";
            this.bossCalcMenuItem.Size = new System.Drawing.Size(90, 20);
            this.bossCalcMenuItem.Text = "Boss Calc";
            this.bossCalcMenuItem.Click += new System.EventHandler(this.BossCalc_Click);
            // minimizeMenuItem
            this.minimizeMenuItem.Name = "minimizeMenuItem";
            this.minimizeMenuItem.Size = new System.Drawing.Size(90, 20);
            this.minimizeMenuItem.Text = "Minimize All";
            this.minimizeMenuItem.Click += new System.EventHandler(this.MinimizeRestore_Click);
            // endSelectedMenuItem
            this.endSelectedMenuItem.Name = "endSelectedMenuItem";
            this.endSelectedMenuItem.Size = new System.Drawing.Size(86, 20);
            this.endSelectedMenuItem.Text = "End Selected";
            this.endSelectedMenuItem.Visible = false;
            this.endSelectedMenuItem.Click += new System.EventHandler(this.EndSelected_Click);
            // endAllMenuItem
            this.endAllMenuItem.Name = "endAllMenuItem";
            this.endAllMenuItem.Size = new System.Drawing.Size(56, 20);
            this.endAllMenuItem.Text = "End All";
            this.endAllMenuItem.Click += new System.EventHandler(this.EndAll_Click);
            // untickAllMenuItem
            this.untickAllMenuItem.Name = "untickAllMenuItem";
            this.untickAllMenuItem.Size = new System.Drawing.Size(70, 20);
            this.untickAllMenuItem.Text = "Untick All";
            this.untickAllMenuItem.Click += new System.EventHandler(this.UntickAll_Click);
            // multicontrollerMenuItem
            this.multicontrollerMenuItem.Name = "multicontrollerMenuItem";
            this.multicontrollerMenuItem.Size = new System.Drawing.Size(100, 20);
            this.multicontrollerMenuItem.Text = "Multicontroller";
            this.multicontrollerMenuItem.Click += new System.EventHandler(this.Multicontroller_Click);
            // LoginButton
            this.LoginButton.Location = new System.Drawing.Point(14, 327);
            this.LoginButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(310, 29);
            this.LoginButton.TabIndex = 2;
            this.LoginButton.Text = "Play";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // accountGrid
            this.accountGrid.AllowDrop = true;
            this.accountGrid.AllowUserToResizeColumns = false;
            this.accountGrid.AllowUserToResizeRows = false;
            this.accountGrid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.accountGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.accountGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Login, this.Toon, this.ToonSlots, this.End, this.priorityColumn});
            this.accountGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.accountGrid.Location = new System.Drawing.Point(14, 31);
            this.accountGrid.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.accountGrid.Name = "accountGrid";
            this.accountGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.accountGrid.ShowCellToolTips = false;
            this.accountGrid.Size = new System.Drawing.Size(310, 288);
            this.accountGrid.TabIndex = 3;
            this.accountGrid.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.AccGrid_OnCellMouseUp);
            this.accountGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.AccGrid_CellValueChanged);
            this.accountGrid.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.AccGrid_CellLeave);
            this.accountGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.AccGrid_DataBindingComplete);
            this.accountGrid.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.AccGrid_UserDeletingRow);
            this.accountGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.AccGrid_DragDrop);
            // Login
            this.Login.DataPropertyName = "LoginWanted";
            this.Login.HeaderText = "Login?";
            this.Login.Name = "Login";
            this.Login.Width = 50;
            // Toon
            this.Toon.DataPropertyName = "Toon";
            this.Toon.HeaderText = "Toon";
            this.Toon.Name = "Toon";
            this.Toon.ReadOnly = true;
            // ToonSlots
            this.ToonSlots.HeaderText = "Jump to";
            this.ToonSlots.Name = "ToonSlots";
            this.ToonSlots.Width = 125;
            // End
            this.End.DataPropertyName = "EndWanted";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Red;
            dataGridViewCellStyle1.NullValue = false;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Red;
            this.End.DefaultCellStyle = dataGridViewCellStyle1;
            this.End.HeaderText = "End?";
            this.End.Name = "End";
            this.End.ReadOnly = true;
            this.End.Width = 50;
            // priorityColumn
            this.priorityColumn.HeaderText = "Priority";
            this.priorityColumn.Name = "priorityColumn";
            this.priorityColumn.Width = 55;
            this.priorityColumn.ReadOnly = false;
            this.priorityColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.priorityColumn.ToolTipText = "Login order priority (1=first). Leave blank for no priority.";
            // snakeGame
            this.mazeGame.Location  = new System.Drawing.Point(330, 31);
            this.mazeGame.Size      = new System.Drawing.Size(330, 331);
            this.mazeGame.Name      = "mazeGame";
            this.mazeGame.TabIndex  = 20;
            this.mazeGame.TabStop   = true;

            // Launcher
            this.AcceptButton = this.LoginButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 362);
            this.Controls.Add(this.mazeGame);
            this.Controls.Add(this.accountGrid);
            this.Controls.Add(this.LoginButton);
            this.Controls.Add(this.TopMenu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.TopMenu;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "Launcher";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tunetoon - Academy Version";
            this.Load += new System.EventHandler(this.Launcher_Load);
            this.Shown += new System.EventHandler(this.Launcher_Shown);
            this.TopMenu.ResumeLayout(false);
            this.TopMenu.PerformLayout();
            this.ServerMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.accountGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip TopMenu;
        private System.Windows.Forms.Button LoginButton;
        private AccountGrid accountGrid;
        private ToolStripMenuItem endAllMenuItem;
        private ToolStripMenuItem untickAllMenuItem;
        private ToolStripMenuItem optionsMenuItem;
        private ToolStripMenuItem windowsMenuItem;
        private ToolStripMenuItem sosMenuItem;
        private ToolStripMenuItem bossCalcMenuItem;
        private ToolStripMenuItem minimizeMenuItem;
        private ToolStripMenuItem endSelectedMenuItem;
        private ToolStripMenuItem serverMenuItem;
        private ContextMenuStrip ServerMenuStrip;
        private ToolStripMenuItem rewrittenMenuItem;
        private ToolStripMenuItem clashMenuItem;
        private DataGridViewCheckBoxColumn Login;
        private DataGridViewTextBoxColumn Toon;
        private DataGridViewComboBoxColumn ToonSlots;
        private DataGridViewCheckBoxColumn End;
        private System.Windows.Forms.ToolStripMenuItem multicontrollerMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn priorityColumn;
        private Tunetoon.Forms.MazeGame mazeGame;
    }
}
