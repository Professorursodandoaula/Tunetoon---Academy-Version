using System;
using System.Drawing;
using System.Windows.Forms;
using Tunetoon.Accounts;

namespace Tunetoon.Forms
{
    public delegate void EditComplete(dynamic account, int index);
    public partial class AccountEdit : Form
    {
        public event EditComplete Edited;

        private dynamic account;
        private int index;

        // Nomes dos slots disponíveis
        public static readonly string[] SlotNames = new string[]
        {
            "No position",
            "Left ½",
            "Right ½",
            "Top ½",
            "Bottom ½",
            "Top Left Corner",
            "Top Right Corner",
            "Bottom Left Corner",
            "Bottom Right Corner",
            "⅓ Left",
            "⅓ Center",
            "⅓ Right",
            "Grid6 Top Col.1",
            "Grid6 Top Col.2",
            "Grid6 Top Col.3",
            "Grid6 Bottom Col.1",
            "Grid6 Bottom Col.2",
            "Grid6 Bottom Col.3",
            "Grid8 Top Col.1",
            "Grid8 Top Col.2",
            "Grid8 Top Col.3",
            "Grid8 Top Col.4",
            "Grid8 Bottom Col.1",
            "Grid8 Bottom Col.2",
            "Grid8 Bottom Col.3",
            "Grid8 Bottom Col.4",
            "Custom4 Top Left",
            "Custom4 Top Right",
            "Custom4 Bottom Left",
            "Custom4 Bottom Right",
            "Shopping",
            "Maximize",
        };

        public AccountEdit()
        {
            InitializeComponent();
            AddWindowSlotSelector();
        }

        private ComboBox slotCombo;
        private ComboBox monitorCombo;

        private void AddWindowSlotSelector()
        {
            // Move o label Password para garantir que não sobreponha
            foreach (Control c in Controls)
            {
                if (c is Label lbl && lbl.Text == "Password:")
                    lbl.Location = new System.Drawing.Point(lbl.Left, 82);
                if (c is TextBox tb && tb == PasswordBox)
                    tb.Location = new System.Drawing.Point(tb.Left, 82);
            }

            var lblSlot = new Label
            {
                Text     = "Window position:",
                AutoSize = true,
                Location = new System.Drawing.Point(16, 115),
            };

            slotCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width         = 242,
                Location      = new System.Drawing.Point(16, 133),
            };
            foreach (var name in SlotNames)
                slotCombo.Items.Add(name);
            slotCombo.SelectedIndex = 0;

            SaveButton.Location = new System.Drawing.Point(SaveButton.Left, 163);
            ClientSize = new System.Drawing.Size(ClientSize.Width, 200);

            Controls.Add(lblSlot);
            Controls.Add(slotCombo);
            slotCombo.BringToFront();
            lblSlot.BringToFront();

            // Monitor selector
            var lblMonitor = new Label
            {
                Text     = "Monitor:",
                AutoSize = true,
                Location = new System.Drawing.Point(16, 163),
            };

            monitorCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width         = 242,
                Location      = new System.Drawing.Point(16, 181),
            };
            monitorCombo.Items.Add("Monitor 1");
            monitorCombo.Items.Add("Monitor 2");
            monitorCombo.SelectedIndex = 0;

            SaveButton.Location = new System.Drawing.Point(SaveButton.Left, 213);
            ClientSize = new System.Drawing.Size(ClientSize.Width, 250);

            Controls.Add(lblMonitor);
            Controls.Add(monitorCombo);
            monitorCombo.BringToFront();
            lblMonitor.BringToFront();
        }

        public void StartEdit(dynamic account, int index)
        {
            FriendlyBox.Text = account.Toon;
            UsernameBox.Text = account.Username;
            PasswordBox.Text = account.Password;

            // Carrega slot salvo
            int slot = account.WindowSlot;
            slotCombo.SelectedIndex = (slot >= 0 && slot < SlotNames.Length - 1) ? slot + 1 : 0;

            // Carrega monitor salvo
            int monitor = account.LoginMonitor;
            monitorCombo.SelectedIndex = (monitor == 1) ? 1 : 0;

            this.account = account;
            this.index = index;

            ShowDialog();
        }

        private void SaveSlot(dynamic account)
        {
            int sel = slotCombo.SelectedIndex;
            account.WindowSlot = sel <= 0 ? -1 : sel - 1;
            account.LoginMonitor = monitorCombo.SelectedIndex;
        }

        private void HandleAccount(RewrittenAccount account)
        {
            account.Username = UsernameBox.Text;
            account.Password = PasswordBox.Text;
            SaveSlot(account);
            Edited(account, index);
            Close();
        }

        private void HandleAccount(ClashAccount account)
        {
            if (account.Authorized && account.Username == UsernameBox.Text && account.Password == PasswordBox.Text)
            {
                SaveSlot(account);
                Edited(account, index);
                Close();
                return;
            }

            account.Username = UsernameBox.Text;
            account.Password = PasswordBox.Text;

            SaveButton.Text = "Authorizing...";
            SaveButton.Enabled = false;

            var clashAuthorization = new ClashAuthorization();
            clashAuthorization.AddAccount(account);

            SaveButton.Text = "Save";
            SaveButton.Enabled = true;

            if (clashAuthorization.LastReason != 0)
            {
                MessageBox.Show(clashAuthorization.LastMessage, "Server response", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveSlot(account);
            Edited(account, index);
            Close();
        }

        private void DoneButton_Click(object sender, EventArgs e)
        {
            account.Toon = FriendlyBox.Text;
            HandleAccount(account);
        }
    }
}
