using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tunetoon.Accounts;
using Tunetoon.DioExtreme;
using Tunetoon.Encrypt;
using Tunetoon.Game;
using Tunetoon.Grid;
using Tunetoon.Login;
using Tunetoon.Patcher;
using Tunetoon.Utilities;
using Tunetoon.MultiController;

namespace Tunetoon.Forms
{
    public partial class Launcher : Form
    {
        private DataHandler dataHandler = new DataHandler();

        private BindingList<RewrittenAccount> rewrittenAccountList = new BindingList<RewrittenAccount>();
        private BindingList<ClashAccount> clashAccountList = new BindingList<ClashAccount>();
        private dynamic currentAccountList;

        private BindingSource bindingSource = new BindingSource();

        // Patchers
        private RewrittenPatcher rewrittenPatcher;
        private ClashPatcher clashPatcher;
        private IPatcher gamePatcher;

        // Login handlers
        private RewrittenLoginHandler rewrittenLoginHandler = new RewrittenLoginHandler();
        private ClashLoginHandler clashLoginHandler = new ClashLoginHandler();
        private dynamic loginHandler;

        // Game handlers
        private RewrittenGameHandler rewrittenGameHandler;
        private ClashGameHandler clashGameHandler;
        private dynamic gameHandler;

        // UI handlers
        private RewrittenGridHandler rewrittenGridHandler = new RewrittenGridHandler();
        private ClashGridHandler clashGridHandler = new ClashGridHandler();
        private IGridHandler gridHandler;

        private AccountEdit accountEdit = new AccountEdit();


        private Config config;

        // Layout automático ao abrir (null = desativado, -1 = AutoGrid)
        public WindowManager.WindowPosition? AutoSnapLayout = null;
        public bool AutoSnapGrid = false;

        // Layouts salvos
        public List<WindowLayout> Layouts = new List<WindowLayout>();

        // Rastreia quais toons tiveram a priority editada nesta sessão
        private HashSet<string> _priorityEditedToons = new HashSet<string>();

        private void LoadLayouts()
        {
            try
            {
                if (System.IO.File.Exists("Layouts.json"))
                {
                    string json = System.IO.File.ReadAllText("Layouts.json");
                    var loaded = System.Text.Json.JsonSerializer.Deserialize<List<WindowLayout>>(json);
                    if (loaded != null) Layouts = loaded;
                }
            }
            catch { }
        }

        public Launcher()
        {
            config = dataHandler.LoadConfig("Config.json");
            dataHandler.LoadClashIngameToons(config);

            LoadLayouts();

            bindingSource.ListChanged += BindingSource_ListChanged;
            accountEdit.Edited += AccountEditComplete;

            InitializeComponent();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Redireciona setas para o SnakeGame quando ele existe no form
            if (keyData == Keys.Up || keyData == Keys.Down ||
                keyData == Keys.Left || keyData == Keys.Right)
            {
                foreach (Control c in Controls)
                {
                    if (c is Tunetoon.Forms.MazeGame maze)
                    {
                        maze.Focus();
                        var e = new KeyEventArgs(keyData);
                        maze.ProcessMazeKey(e);
                        return true;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            var accountDecryptor = new AccountDecryptor();
            accountDecryptor.OnPassedAuthentication += onPassedAuthentication;
            accountDecryptor.Authenticate(config);
        }


        public void onPassedAuthentication(BindingList<RewrittenAccount> rewrittenAccountList, BindingList<ClashAccount> clashAccountList)
        {
            this.rewrittenAccountList = rewrittenAccountList;
            this.clashAccountList = clashAccountList;


            dataHandler.FindClashIngameToons(clashAccountList);

            rewrittenPatcher = new RewrittenPatcher(config);
            clashPatcher = new ClashPatcher(config);
            rewrittenGameHandler = new RewrittenGameHandler(config);
            clashGameHandler = new ClashGameHandler(config);

            HandleConfig();

            accountGrid.AutoGenerateColumns = false;
            bindingSource.DataSource = currentAccountList;
            accountGrid.DataSource = bindingSource;
        }

        private void HandleConfig()
        {
            if (config.GameServer == Server.Rewritten)
            {
                currentAccountList = rewrittenAccountList;
                gamePatcher = rewrittenPatcher;
                loginHandler = rewrittenLoginHandler;
                gameHandler = rewrittenGameHandler;
                gridHandler = rewrittenGridHandler;
                rewrittenMenuItem.Checked = true;
                clashMenuItem.Checked = false;
                accountGrid.Columns[ToonSlots.Index].Visible = false;
            }
            else
            {
                currentAccountList = clashAccountList;
                gamePatcher = clashPatcher;
                loginHandler = clashLoginHandler;
                gameHandler = clashGameHandler;
                gridHandler = clashGridHandler;
                rewrittenMenuItem.Checked = false;
                clashMenuItem.Checked = true;
                accountGrid.Columns[ToonSlots.Index].Visible = true;
            }

            if (config.SelectEndGames)
            {
                endSelectedMenuItem.Visible = true;
                accountGrid.Columns[End.Index].ReadOnly = false;
            }
        }

        private void ReflectPatcherProgress(PatchProgress p)
        {
            Text = $"Tunetoon - {p.currentAction} Files {p.CurrentFilesProcessed}/{p.TotalFilesToProcess}";
        }

        private void ShowPatcherError(string text)
        {
            MessageBox.Show(text, "Game patcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void AssertPatcherError(string errorText)
        {
            if (gamePatcher.HasFailed())
            {
                throw new PatchException(errorText);
            }
        }

        private async Task RunPatcherAsync(Progress<PatchProgress> progress)
        {
            gamePatcher.Initialize(gamePatcher.GetGameDirectory());
            AssertPatcherError("Unable to create game directory. Check permissions or change the target directory.");

            gamePatcher.GetPatchManifest();
            AssertPatcherError("Could not retrieve patch manifest. The game has not been updated.");

            gamePatcher.CheckGameFiles(progress);

            await gamePatcher.DownloadGameFiles(progress);
            AssertPatcherError("An error occurred downloading update files. The game has not been updated.");

            gamePatcher.PatchGameFiles(progress);
            AssertPatcherError("An error occurred applying game patches. The game has not been updated.");
        }

        private async Task StartUpdate()
        {
            if (config.SkipUpdates || gameHandler.ActiveProcesses.Count > 0)
            {
                return;
            }

            LoginButton.Enabled = false;
            serverMenuItem.Enabled = false;
            LoginButton.Text = "Checking for updates...";

            try
            {
                var progress = new Progress<PatchProgress>(ReflectPatcherProgress);
                await Task.Run(() => RunPatcherAsync(progress));
            }
            catch (PatchException e)
            {
                ShowPatcherError(e.Message);
            }

            Text = "Tunetoon - Academy Version";
            LoginButton.Text = "Play";
            LoginButton.Enabled = true;

            serverMenuItem.Enabled = true;
        }

        private async void Launcher_Shown(object sender, EventArgs e)
        {
            // Posiciona o Launcher com espaço à esquerda para o Manage Layouts
            var screen = Screen.FromControl(this);
            int manageLayoutsWidth = 620;
            int desiredX = screen.WorkingArea.Left + manageLayoutsWidth + 20;
            int desiredY = screen.WorkingArea.Top + (screen.WorkingArea.Height - this.Height) / 2;
            this.Location = new Point(desiredX, desiredY);

            InstallKeyboardHook();

            if (config.GameServer == Server.Clash)
            {
                config.ClashUrls = ApiDataRetriever.LoadClashUrls();
            }
            await StartUpdate();
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            accountGrid.ClearGridSelections();
            UpdateLoginPriority();

            if (config.GameServer == Server.Rewritten && !Directory.Exists(config.RewrittenPath) ||
                config.GameServer == Server.Clash && !Directory.Exists(config.ClashPath))
            {
                MessageBox.Show("Game directory missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            serverMenuItem.Enabled = false;
            LoginButton.Enabled = false;

            try
            {
                // Captura quais contas vão logar e ordena por prioridade ANTES do login
                var toLogin = new List<dynamic>();
                foreach (var acc in currentAccountList)
                    if (acc != null && acc.CanLogin())
                        toLogin.Add(acc);

                // Ordenar por prioridade (1, 2, ... primeiro; sem prioridade por último)
                toLogin.Sort((a, b) =>
                {
                    int pa = (a.LoginPriority > 0) ? a.LoginPriority : int.MaxValue;
                    int pb = (b.LoginPriority > 0) ? b.LoginPriority : int.MaxValue;
                    return pa.CompareTo(pb);
                });

                await loginHandler.LoginAccounts(currentAccountList);

                // Iniciar jogos na ordem de prioridade com delay entre cada um
                // Contas priorizadas primeiro (sequencial), demais depois (paralelo)
                var toStartPrioritized = new List<dynamic>();
                var toStartNormal = new List<dynamic>();
                foreach (var acc in currentAccountList)
                {
                    if (acc != null && acc.LoggedIn)
                    {
                        if (acc.LoginPriority > 0)
                            toStartPrioritized.Add(acc);
                        else
                            toStartNormal.Add(acc);
                    }
                }
                toStartPrioritized.Sort((a, b) =>
                {
                    int pa = (int)a.LoginPriority, pb = (int)b.LoginPriority;
                    return pa.CompareTo(pb);
                });

                foreach (var acc in toStartPrioritized)
                {
                    gameHandler.StartGame(acc);
                    await Task.Delay(2000);
                }
                foreach (var acc in toStartNormal)
                    gameHandler.StartGame(acc);

                // Snap por conta individual — apenas contas que acabaram de logar
                foreach (var entry in gameHandler.ActiveProcesses)
                {
                    var acc = entry.Key;
                    var proc = entry.Value;

                    // Só aplica se a conta estava na lista de logins deste clique
                    bool justStarted = false;
                    foreach (var j in toLogin)
                        if (j.Toon == acc.Toon) { justStarted = true; break; }
                    if (!justStarted) continue;

                    if (acc.WindowSlot < 0) continue;

                    var pos = (WindowManager.WindowPosition)acc.WindowSlot;
                    int monitorIndex = acc.LoginMonitor;

                    _ = Task.Run(async () =>
                    {
                        for (int i = 0; i < 60; i++)
                        {
                            await Task.Delay(1000);
                            if (proc.HasExited) break;
                            try { proc.Refresh(); } catch { break; }
                            if (proc.MainWindowHandle != IntPtr.Zero)
                            {
                                await Task.Delay(2000);
                                var screens = Screen.AllScreens;
                                var target = monitorIndex < screens.Length ? screens[monitorIndex] : screens[0];
                                WindowManager.SnapWindow(proc.MainWindowHandle, pos, target);
                                // Aguarda renderização e traz Launcher para frente
                                await Task.Delay(800);
                                this.Invoke((Action)(() =>
                                {
                                    this.BringToFront();
                                    this.Activate();
                                }));
                                break;
                            }
                        }
                    });
                }

            }
            catch
            {
                MessageBox.Show("An error occured during the login process.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }


            serverMenuItem.Enabled = true;
            LoginButton.Enabled = true;
        }

        // Captura prioridade digitada na coluna Priority e salva na conta
        // Só atualiza se a célula tiver valor não nulo (evita apagar ao editar outras colunas)
        private void UpdateLoginPriority()
        {
            foreach (DataGridViewRow row in accountGrid.Rows)
            {
                if (row.IsNewRow) continue;
                dynamic acc = row.DataBoundItem;
                if (acc == null) continue;

                var cell = row.Cells["priorityColumn"];

                // Lê do EditingControl se esta célula está em edição ativa
                string val;
                if (accountGrid.IsCurrentCellInEditMode
                    && accountGrid.CurrentCell?.RowIndex == row.Index
                    && accountGrid.Columns[accountGrid.CurrentCell.ColumnIndex].Name == "priorityColumn"
                    && accountGrid.EditingControl is TextBox tb)
                    val = tb.Text.Trim();
                else if (cell.Value != null)
                    val = cell.Value.ToString().Trim();
                else
                    continue; // célula nunca editada — não sobrescreve

                int.TryParse(val, out int priority);
                acc.LoginPriority = priority;
            }

            if (config.EncryptAccounts)
            {
                dataHandler.SaveEncrypted(rewrittenAccountList, Constants.ENCRYPTED_REWRITTEN_ACCOUNT_FILE_NAME);
                dataHandler.SaveEncrypted(clashAccountList, Constants.ENCRYPTED_CLASH_ACCOUNT_FILE_NAME);
            }
            else
            {
                dataHandler.SaveSerialized(rewrittenAccountList, Constants.REWRITTEN_ACCOUNT_FILE_NAME);
                dataHandler.SaveSerialized(clashAccountList, Constants.CLASH_ACCOUNT_FILE_NAME);
            }
        }

        public void ChangeEndCellColor(int index, Color color)
        {
            if (index < 0)
            {
                return;
            }

            if (accountGrid.CurrentCell == null)
            {
                accountGrid.CurrentCell = accountGrid.Rows[index].Cells[End.Index];
            }

            accountGrid.BeginEdit(false);

            var chkCell = accountGrid.Rows[index].Cells[End.Index] as DataGridViewCheckBoxCell;
            chkCell.Style.BackColor = color;
            chkCell.Style.SelectionBackColor = color;

            if (!config.SelectEndGames)
            {
                chkCell.Value = chkCell.FalseValue;
            }

            accountGrid.EndEdit();
        }

        private void AccGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.Reset)
            {
                CheckLoggedIns(currentAccountList);
                LoadPriorityColumn();
            }

            gridHandler.DataBindingComplete(accountGrid);
        }

        private void LoadPriorityColumn()
        {
            foreach (DataGridViewRow row in accountGrid.Rows)
            {
                if (row.IsNewRow) continue;
                dynamic acc = row.DataBoundItem;
                if (acc == null) continue;
                row.Cells["priorityColumn"].Value = acc.LoginPriority > 0
                    ? acc.LoginPriority.ToString()
                    : "";
            }
        }

        private void BindingSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.NewIndex >= 0 && e.ListChangedType == ListChangedType.ItemChanged)
            {
                var account = currentAccountList[e.NewIndex];
                var color = account.LoggedIn ? Color.Green : Color.Red;
                ChangeEndCellColor(e.NewIndex, color);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing && e.CloseReason != CloseReason.WindowsShutDown)
            {
                return;
            }

            // Pede confirmação se houver toons ativos
            if (e.CloseReason == CloseReason.UserClosing &&
                gameHandler != null &&
                gameHandler.ActiveProcesses.Count > 0)
            {
                if (!ConfirmEndAllForm.Show(this))
                {
                    e.Cancel = true;
                    return;
                }
                // Confirmou — encerra todos os toons
                foreach (var acc in rewrittenAccountList)
                    rewrittenGameHandler.StopGame(acc);
                foreach (var acc in clashAccountList)
                    clashGameHandler.StopGame(acc);
                MultiController.MulticontrollerBridge.CloseAll();
            }

            UninstallKeyboardHook();
            UpdateLoginPriority();

            dataHandler.SaveConfig(config, "Config.json");

            if (config.EncryptAccounts)
            {
                dataHandler.SaveEncrypted(rewrittenAccountList, Constants.ENCRYPTED_REWRITTEN_ACCOUNT_FILE_NAME);
                dataHandler.SaveEncrypted(clashAccountList, Constants.ENCRYPTED_CLASH_ACCOUNT_FILE_NAME);
            }
            else
            {
                dataHandler.SaveSerialized(rewrittenAccountList, Constants.REWRITTEN_ACCOUNT_FILE_NAME);
                dataHandler.SaveSerialized(clashAccountList, Constants.CLASH_ACCOUNT_FILE_NAME);
            }

            base.OnFormClosing(e);
        }

        private void AccGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            gridHandler.UserDeletingRow(e.Row.DataBoundItem);
        }

        private void AccGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && accountGrid.Columns[e.ColumnIndex].Name == "priorityColumn")
            {
                // Registra que este toon foi editado
                if (e.RowIndex >= 0)
                {
                    string toon = accountGrid.Rows[e.RowIndex].Cells["Toon"].Value?.ToString();
                    if (!string.IsNullOrEmpty(toon))
                        _priorityEditedToons.Add(toon);
                }
                UpdateLoginPriority();
            }
            if (e.RowIndex < 0 || e.ColumnIndex != ToonSlots.Index)
            {
                return;
            }

            gridHandler.CellValueChanged(accountGrid, e.RowIndex);
        }

        private void AccGrid_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && accountGrid.Columns[e.ColumnIndex].Name == "priorityColumn")
            {
                // Registra que este toon foi editado
                if (e.RowIndex >= 0)
                {
                    string toon = accountGrid.Rows[e.RowIndex].Cells["Toon"].Value?.ToString();
                    if (!string.IsNullOrEmpty(toon))
                        _priorityEditedToons.Add(toon);
                }
                accountGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                UpdateLoginPriority();
            }
        }

        private void AccountEditComplete(dynamic account, int index)
        {
            currentAccountList[index] = account;
            if (account is ClashAccount && account.Authorized)
            {
                dataHandler.FindClashIngameToons(account);
            }
            bindingSource.ResetBindings(false);
        }

        private void AccGrid_OnCellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != End.Index && e.ColumnIndex != Toon.Index)
            {
                return;
            }

            if (e.ColumnIndex == End.Index && e.RowIndex == accountGrid.NewRowIndex)
            {
                return;
            }

            var account = currentAccountList[e.RowIndex];

            if (e.ColumnIndex == Toon.Index && !accountGrid.MoveMode)
            {
                accountEdit.StartEdit(account, e.RowIndex);
            }
            else
            {
                if (!config.SelectEndGames && account.LoggedIn)
                {
                    gameHandler.StopGame(account);
                }
            }
        }

        private void AccGrid_DragDrop(object sender, DragEventArgs e)
        {
            int rowIndexToDrop = accountGrid.RowIndexToDrop;

            if (!accountGrid.RowIndexValid(rowIndexToDrop))
            {
                return;
            }

            var accountToMove = currentAccountList[rowIndexToDrop];
            var color = accountToMove.LoggedIn ? Color.Green : Color.Red;
            ChangeEndCellColor(rowIndexToDrop, color);
        }

        private void EndSelected_Click(object sender, EventArgs e)
        {
            foreach (var account in currentAccountList)
            {
                if (account != null && account.EndWanted && account.LoggedIn)
                {
                    gameHandler.StopGame(account);
                    account.EndWanted = false;
                }
            }
        }

        private void EndAll_Click(object sender, EventArgs e)
        {
            if (!ConfirmEndAllForm.Show(this)) return;

            if (config.GlobalEndAll || config.GameServer == Server.Rewritten)
            {
                foreach (var acc in rewrittenAccountList)
                {
                    rewrittenGameHandler.StopGame(acc);
                }
            }

            if (config.GlobalEndAll || config.GameServer == Server.Clash)
            {
                foreach (var acc in clashAccountList)
                {
                    clashGameHandler.StopGame(acc);
                }
            }

            // Fecha todos os Multicontrollers ao encerrar os jogos
            MultiController.MulticontrollerBridge.CloseAll();
        }

        private void UntickAll_Click(object sender, EventArgs e)
        {
            foreach (var account in currentAccountList)
            {
                account.LoginWanted = false;
            }
            accountGrid.ClearGridSelections();
        }

        public void SelectionOptionAltered()
        {
            if (config.SelectEndGames)
            {
                endSelectedMenuItem.Visible = true;
                accountGrid.Columns[End.Index].ReadOnly = false;
            }
            else
            {
                endSelectedMenuItem.Visible = false;
                accountGrid.Columns[End.Index].ReadOnly = true;
                foreach (DataGridViewRow row in accountGrid.Rows)
                {
                    var chk = (DataGridViewCheckBoxCell)row.Cells[End.Index];
                    chk.Value = false;
                }
            }
        }

        private void TopMenu_Click(object sender, EventArgs e)
        {
            accountGrid.ClearGridSelections();
        }

        private async void Rewritten_Click(object sender, EventArgs e)
        {
            config.GameServer = Server.Rewritten;
            HandleConfig();

            bindingSource.DataSource = rewrittenAccountList;
            bindingSource.ResetBindings(false);

            await StartUpdate();
        }

        private async void Clash_Click(object sender, EventArgs e)
        {
            config.GameServer = Server.Clash;

            if (!config.ClashUrls.Initialized)
            {
                config.ClashUrls = ApiDataRetriever.LoadClashUrls();
            }

            HandleConfig();

            bindingSource.DataSource = clashAccountList;
            bindingSource.ResetBindings(false);

            await StartUpdate();
        }

        private void CheckLoggedIns(dynamic accountList)
        {
            for (int i = 0; i < accountList.Count; ++i)
            {
                if (accountList[i].LoggedIn)
                {
                    ChangeEndCellColor(i, Color.Green);
                }
            }
        }

        private void Options_Click(object sender, EventArgs e)
        {
            Options optionWnd = new Options(this, config);
            optionWnd.ShowDialog();
        }

        // Minimizar/Restaurar janelas do jogo
        private bool _windowsMinimized = false;

        private void MinimizeRestore_Click(object sender, EventArgs e)
        {
            var handles = new System.Collections.Generic.List<IntPtr>();
            foreach (var entry in gameHandler.ActiveProcesses)
                if (entry.Value.MainWindowHandle != IntPtr.Zero)
                    handles.Add(entry.Value.MainWindowHandle);

            if (_windowsMinimized)
            {
                WindowManager.RestoreAll(handles);
                _windowsMinimized = false;
                minimizeMenuItem.Text = "Minimize All";
            }
            else
            {
                WindowManager.MinimizeAll(handles);
                _windowsMinimized = true;
                minimizeMenuItem.Text = "Restore All";
            }
        }

        // Abre o Multicontroller já com os handles dos toons logados
        private void Multicontroller_Click(object sender, EventArgs e)
        {
            try
            {
                var toonToHandle = new Dictionary<string, (IntPtr handle, int windowSlot, int monitor)>();
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mc_debug.txt"),
                    $"Click: gameHandler={gameHandler?.GetType().Name} active={gameHandler?.ActiveProcesses?.Count}\n");

                foreach (var entry in gameHandler.ActiveProcesses)
                {
                    if (entry.Key.Toon != null)
                    {
                        toonToHandle[(string)entry.Key.Toon] = ((IntPtr)entry.Value.MainWindowHandle, (int)entry.Key.WindowSlot, (int)entry.Key.LoginMonitor);
                        System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mc_debug.txt"),
                            $"  toon={entry.Key.Toon} handle={entry.Value.MainWindowHandle}\n");
                    }
                }

                // Minimiza o launcher ao abrir o Multicontroller
                this.WindowState = System.Windows.Forms.FormWindowState.Minimized;

                System.Threading.Tasks.Task.Run(() =>
                {
                    try { MulticontrollerBridge.OpenWithHandles(toonToHandle); }
                    catch (Exception ex2)
                    {
                        System.IO.File.AppendAllText(
                            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mc_debug.txt"),
                            "TASK EXCEPTION: " + ex2.ToString() + Environment.NewLine);
                    }
                });
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mc_debug.txt"), $"EXCEPTION: {ex}\n");
            }
        }

        private void BossCalc_Click(object sender, EventArgs e)
        {
            var form = new BossCalcForm();
            form.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            form.Load += (s, e2) =>
            {
                var screen = Screen.FromControl(this);
                var wa = screen.WorkingArea;
                form.Location = new System.Drawing.Point(
                    wa.X + (wa.Width  - form.Width)  / 2,
                    wa.Y + (wa.Height - form.Height) / 2);
            };
            form.Show(this);
        }

        // Abre o painel de posicionamento de janelas
        private void Windows_Click(object sender, EventArgs e)
        {
            // Lista de todos os toons cadastrados
            var allToons = new List<string>();
            foreach (var acc in currentAccountList)
                if (acc != null && acc.Toon != null)
                    allToons.Add(acc.Toon);

            // Func que sempre busca os handles atuais no momento do Apply
            Dictionary<string, (IntPtr handle, int windowSlot, int monitor)> GetCurrentHandles()
            {
                var toonToHandle = new Dictionary<string, (IntPtr handle, int windowSlot, int monitor)>();
                foreach (var entry in gameHandler.ActiveProcesses)
                    if (entry.Key.Toon != null)
                        toonToHandle[(string)entry.Key.Toon] = ((IntPtr)entry.Value.MainWindowHandle, (int)entry.Key.WindowSlot, (int)entry.Key.LoginMonitor);
                return toonToHandle;
            }

            var form = new LayoutManagerForm(
                () => GetCurrentHandles(),
                () => allToons,
                Layouts);
            form.Show(this);
        }

        // ── Hotkeys de Layout ─────────────────────────────────────────────────

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _keyboardProc;
        private IntPtr _keyboardHook = IntPtr.Zero;

        private void InstallKeyboardHook()
        {
            _keyboardProc = KeyboardHookCallback;
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            _keyboardHook = SetWindowsHookEx(13, _keyboardProc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private void UninstallKeyboardHook()
        {
            if (_keyboardHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHook);
                _keyboardHook = IntPtr.Zero;
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int WM_KEYDOWN    = 0x0100;
            const int WM_SYSKEYDOWN = 0x0104;

            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = System.Runtime.InteropServices.Marshal.ReadInt32(lParam);
                string keyName = ((Keys)vkCode).ToString();

                // Alt+M → Minimize/Restore All
                if (keyName == "M" && (Control.ModifierKeys & Keys.Alt) != 0)
                {
                    BeginInvoke((Action)(() => MinimizeRestore_Click(this, EventArgs.Empty)));
                    return (IntPtr)1;
                }

                // Ctrl+M → Multicontroller
                if (keyName == "M" && (Control.ModifierKeys & Keys.Control) != 0)
                {
                    BeginInvoke((Action)(() => Multicontroller_Click(this, EventArgs.Empty)));
                    return (IntPtr)1;
                }

                // Só processa hotkeys de layout em WM_KEYDOWN
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    var layout = Layouts.FirstOrDefault(l => l.Hotkey == keyName);
                    if (layout != null)
                    {
                        BeginInvoke((Action)(() => ApplyLayout(layout)));
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        private void ApplyLayout(WindowLayout layout)
        {
var toonHandles = new Dictionary<string, IntPtr>();

            // Coleta handles de todos os handlers ativos
            foreach (var entry in rewrittenGameHandler.ActiveProcesses)
                if (entry.Key?.Toon != null && !entry.Value.HasExited)
                    toonHandles[entry.Key.Toon] = entry.Value.MainWindowHandle;

            var screens = Screen.AllScreens;
            foreach (var slot in layout.Slots)
            {
                if (slot.Position < 0) continue;
                if (!toonHandles.TryGetValue(slot.AccountToon, out IntPtr hWnd)) continue;
                if (hWnd == IntPtr.Zero) continue;
                var pos    = (WindowManager.WindowPosition)slot.Position;
                var screen = (slot.Monitor >= 0 && slot.Monitor < screens.Length)
                    ? screens[slot.Monitor] : Screen.PrimaryScreen;
                WindowManager.SnapWindow(hWnd, pos, screen);
            }
        }
        private System.Windows.Forms.Timer _sosTimer;
        private bool _sosActive = false;
        private SosResultForm _sosWindow = null;



        private void SosCheck_Click(object sender, EventArgs e)
        {
            if (_sosActive)
            {
                _sosTimer?.Stop();
                _sosActive = false;
                sosMenuItem.Text = "Check SOS";
                SosResultForm.ResetConfirmation();
                _sosWindow?.Close();
                _sosWindow = null;
            }
            else
            {
                if (_sosTimer == null)
                {
                    _sosTimer = new System.Windows.Forms.Timer { Interval = 600 };
                    _sosTimer.Tick += SosTimer_Tick;
                }
                _sosActive = true;
                sosMenuItem.Text = "SOS ON ✓";

                // Mostra hint se usuário não optou por não ver mais
                if (SosHintForm.ShouldShow())
                    SosHintForm.ShowHint(Screen.FromControl(this));

                System.Threading.Tasks.Task.Run(() => SosDetector.PreloadReferences());
                _sosTimer.Start();
            }
        }

        


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out SosWinRect lpRect);
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct SosWinRect { public int Left, Top, Right, Bottom; }

        private static bool IsShoppingWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return false;
            if (!GetClientRect(hWnd, out SosWinRect r)) return false;
            int w = r.Right - r.Left;
            int h = r.Bottom - r.Top;
            if (w <= 0 || h <= 0) return false;
            return (double)h / w > 2.5;
        }

        private void SosTimer_Tick(object sender, EventArgs e)
        {
            var results  = new System.Collections.Generic.List<(string, SosCard)>();
            bool anyFound = false;
            IntPtr gameHWnd = IntPtr.Zero;

            foreach (var entry in gameHandler.ActiveProcesses)
            {
                string toon = entry.Key.Toon ?? "Unknown";
                IntPtr hWnd = entry.Value.MainWindowHandle;

                // Só detecta em janelas com proporção Shopping (h/w > 2.5)
                if (!IsShoppingWindow(hWnd)) continue;

                SosCard card = SosDetector.Detect(hWnd);
                if (card != null)
                {
                    anyFound = true;
                    gameHWnd = hWnd;
                    results.Add((toon, card));
                    break;
                }
            }

            if (!anyFound) return;

            if (_sosWindow == null || _sosWindow.IsDisposed)
            {
                _sosWindow = new SosResultForm(results, gameHWnd);
                _sosWindow.FormClosed += (s, ev) => _sosWindow = null;
                _sosWindow.Show();
            }
            else
            {
                bool confirmed = _sosWindow.AddResult(results);
                if (confirmed)
                {
                    // Para o timer mas mantém a janela aberta por 12s (fechada pelo SosResultForm)
                    _sosTimer.Stop();
                    _sosActive = false;
                    sosMenuItem.Text = "Check SOS";
                    // Reagenda para fechar o menu após a janela fechar
                    _sosWindow.FormClosed += (s2, ev2) => { _sosWindow = null; };
                }
            }
        }

        private void MoveModeIntent(bool keyHeldDown)
        {
            if (keyHeldDown == false && config.SelectEndGames)
            {
                endSelectedMenuItem.Visible = true;
            }
            else
            {
                endSelectedMenuItem.Visible = false;
            }
            accountGrid.MoveMode = keyHeldDown;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (accountGrid.MoveMode)
            {
                MoveModeIntent(false);
            }
            base.OnDeactivate(e);
        }

        protected override bool ProcessKeyPreview(ref Message msg)
        {
            if ((Keys)msg.WParam != Keys.ControlKey)
            {
                return base.ProcessKeyPreview(ref msg);
            }

            const int KeyDown = 0x100;
            const int KeyUp = 0x101;

            if (!accountGrid.MoveMode && msg.Msg == KeyDown)
            {
                MoveModeIntent(true);
            }
            else if (accountGrid.MoveMode && msg.Msg == KeyUp)
            {
                MoveModeIntent(false);
            }

            return true;
        }
    }
}
