using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class MainForm : Form
    {
        private Settings _settings;
        private HotkeyManager? _hotkeyManager;
        private Dictionary<int, AppShortcut> _hotkeyIdToShortcut = new Dictionary<int, AppShortcut>();
        private Dictionary<int, ClipboardShortcut> _hotkeyIdToClipboardShortcut = new Dictionary<int, ClipboardShortcut>();
        private ProfileManager? _profileManager;
        private int _commandPaletteHotkeyId = -1;

        // UI Controls
        private TabControl _tabControl;
        private ListView _shortcutsListView;
        private ImageList _iconImageList;
        private Button _addButton;
        private Button _editButton;
        private Button _removeButton;
        private Button _importButton;
        private Button _exportButton;
        private ListView _clipboardShortcutsListView;
        private Button _addClipboardButton;
        private Button _editClipboardButton;
        private Button _removeClipboardButton;
        private CheckBox _startWithWindowsCheckBox;
        private Button _settingsButton;
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;

        public MainForm()
        {
            InitializeComponent();
            _settings = SettingsManager.LoadSettings();
            _hotkeyManager = new HotkeyManager(this.Handle);
            
            // Initialize logging
            Logger.IsEnabled = _settings.EnableLogging;
            Logger.MinimumLevel = _settings.LogLevel;
            Logger.Info("QuickLauncher started");
            
            // Initialize profile manager
            _profileManager = new ProfileManager(_settings);
            _profileManager.ProfileChanged += ProfileManager_ProfileChanged;
            _profileManager.StartAutoSwitching();
            
            LoadShortcuts();
            LoadClipboardShortcuts();
            RegisterAllHotkeys();
            
            _startWithWindowsCheckBox.Checked = _settings.StartWithWindows;
            
            // Apply theme
            ApplyTheme();
            
            SetupTrayIcon();
        }

        private void InitializeComponent()
        {
            this.Text = "QuickLauncher - Global Shortcuts";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 550);
            this.FormClosing += MainForm_FormClosing;

            // TabControl
            _tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(660, 390),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                SelectedIndex = 0
            };
            _tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            this.Controls.Add(_tabControl);

            // Tab 1: App Shortcuts
            TabPage appShortcutsTab = new TabPage("App Shortcuts");
            _tabControl.TabPages.Add(appShortcutsTab);

            // ImageList for icons
            _iconImageList = new ImageList
            {
                ImageSize = new Size(16, 16),
                ColorDepth = ColorDepth.Depth32Bit
            };

            // ListView for app shortcuts
            _shortcutsListView = new ListView
            {
                Location = new Point(6, 6),
                Size = new Size(638, 330),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                SmallImageList = _iconImageList,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _shortcutsListView.Columns.Add("Name", 150);
            _shortcutsListView.Columns.Add("Applications", 350);
            _shortcutsListView.Columns.Add("Shortcut", 120);
            _shortcutsListView.DoubleClick += EditButton_Click;
            appShortcutsTab.Controls.Add(_shortcutsListView);

            // Tab 2: Paste Shortcuts
            TabPage pasteShortcutsTab = new TabPage("Paste Shortcuts");
            _tabControl.TabPages.Add(pasteShortcutsTab);

            // ListView for clipboard shortcuts
            _clipboardShortcutsListView = new ListView
            {
                Location = new Point(6, 6),
                Size = new Size(638, 330),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _clipboardShortcutsListView.Columns.Add("Name", 200);
            _clipboardShortcutsListView.Columns.Add("Content Preview", 300);
            _clipboardShortcutsListView.Columns.Add("Shortcut", 120);
            _clipboardShortcutsListView.DoubleClick += EditButton_Click;
            pasteShortcutsTab.Controls.Add(_clipboardShortcutsListView);

            // Buttons (below TabControl on main form)
            _addButton = new Button
            {
                Text = "Add Shortcut",
                Location = new Point(10, 410),
                Size = new Size(150, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };
            _addButton.Click += AddButton_Click;
            this.Controls.Add(_addButton);

            _editButton = new Button
            {
                Text = "Edit",
                Location = new Point(170, 410),
                Size = new Size(90, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _editButton.Click += EditButton_Click;
            this.Controls.Add(_editButton);

            _removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(270, 410),
                Size = new Size(90, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _removeButton.Click += RemoveButton_Click;
            this.Controls.Add(_removeButton);

            _importButton = new Button
            {
                Text = "Import...",
                Location = new Point(430, 410),
                Size = new Size(110, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _importButton.Click += ImportButton_Click;
            this.Controls.Add(_importButton);

            _exportButton = new Button
            {
                Text = "Export...",
                Location = new Point(550, 410),
                Size = new Size(110, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _exportButton.Click += ExportButton_Click;
            this.Controls.Add(_exportButton);

            // Settings
            _startWithWindowsCheckBox = new CheckBox
            {
                Text = "Start with Windows",
                Location = new Point(10, 455),
                Size = new Size(200, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            _startWithWindowsCheckBox.CheckedChanged += StartWithWindowsCheckBox_CheckedChanged;
            this.Controls.Add(_startWithWindowsCheckBox);
            
            // Settings Button
            _settingsButton = new Button
            {
                Text = "Settings...",
                Location = new Point(220, 455),
                Size = new Size(100, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };
            _settingsButton.Click += SettingsButton_Click;
            this.Controls.Add(_settingsButton);

            // Info label
            Label infoLabel = new Label
            {
                Text = "Create global keyboard shortcuts to launch apps or paste text. All shortcuts work system-wide.",
                Location = new Point(10, 485),
                Size = new Size(660, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            this.Controls.Add(infoLabel);
            
            // Initialize button visibility after all controls are added
            UpdateButtonVisibility();
        }

        private void SetupTrayIcon()
        {
            _trayMenu = new ContextMenuStrip();
            _trayMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
            _trayMenu.Items.Add("-");
            _trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());

            _trayIcon = new NotifyIcon
            {
                Text = "QuickLauncher",
                Visible = true,
                ContextMenuStrip = _trayMenu
            };
            
            // Create a simple icon (you can replace this with an actual icon file)
            _trayIcon.Icon = SystemIcons.Application;
            _trayIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_settings.MinimizeToTray && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                _hotkeyManager?.UnregisterAll();
                _trayIcon?.Dispose();
            }
        }

        private void LoadShortcuts()
        {
            _shortcutsListView.Items.Clear();
            _iconImageList.Images.Clear();
            
            int iconIndex = 0;
            foreach (var shortcut in _settings.Shortcuts)
            {
                string appsDisplay = shortcut.Apps.Count == 1 
                    ? shortcut.Apps[0].ExePath 
                    : $"{shortcut.Apps.Count} applications";
                
                var item = new ListViewItem(new[]
                {
                    shortcut.Name,
                    appsDisplay,
                    shortcut.KeyDisplayName
                });
                
                // Extract and set icon from first app
                if (shortcut.Apps.Count > 0 && File.Exists(shortcut.Apps[0].ExePath))
                {
                    try
                    {
                        using (Icon icon = Icon.ExtractAssociatedIcon(shortcut.Apps[0].ExePath)!)
                        {
                            _iconImageList.Images.Add(icon);
                            item.ImageIndex = iconIndex++;
                        }
                    }
                    catch
                    {
                        // If icon extraction fails, continue without icon
                    }
                }
                
                item.Tag = shortcut;
                _shortcutsListView.Items.Add(item);
            }
        }

        private void LoadClipboardShortcuts()
        {
            _clipboardShortcutsListView.Items.Clear();
            
            foreach (var clipboardShortcut in _settings.ClipboardShortcuts)
            {
                string contentPreview = clipboardShortcut.Content.Length > 50 
                    ? clipboardShortcut.Content.Substring(0, 50).Replace("\r\n", " ").Replace("\n", " ") + "..." 
                    : clipboardShortcut.Content.Replace("\r\n", " ").Replace("\n", " ");
                
                var item = new ListViewItem(new[]
                {
                    clipboardShortcut.Name,
                    contentPreview,
                    clipboardShortcut.KeyDisplayName
                });
                
                item.Tag = clipboardShortcut;
                _clipboardShortcutsListView.Items.Add(item);
            }
        }

        private void RegisterAllHotkeys()
        {
            _hotkeyManager?.UnregisterAll();
            _hotkeyIdToShortcut.Clear();
            _hotkeyIdToClipboardShortcut.Clear();
            
            Logger.Info("Registering hotkeys");
            
            // Register command palette hotkey if configured
            if (_settings.CommandPaletteHotkeyModifiers != 0 && _settings.CommandPaletteHotkeyKeyCode != 0)
            {
                _commandPaletteHotkeyId = _hotkeyManager!.RegisterHotkey(
                    (uint)_settings.CommandPaletteHotkeyModifiers,
                    (uint)_settings.CommandPaletteHotkeyKeyCode);
                    
                if (_commandPaletteHotkeyId != -1)
                {
                    Logger.Info($"Command palette hotkey registered: {_settings.CommandPaletteHotkey}");
                }
            }

            foreach (var shortcut in _settings.Shortcuts.Where(s => s.IsEnabled))
            {
                // Check if shortcut is enabled in current profile
                if (_profileManager != null && !_profileManager.IsShortcutEnabledInActiveProfile(shortcut.Id))
                    continue;
                    
                int hotkeyId = _hotkeyManager!.RegisterHotkey((uint)shortcut.Modifiers, (uint)shortcut.KeyCode);
                if (hotkeyId != -1)
                {
                    _hotkeyIdToShortcut[hotkeyId] = shortcut;
                    Logger.Debug($"Registered hotkey for {shortcut.Name}: {shortcut.KeyDisplayName}");
                }
                else
                {
                    Logger.Warning($"Failed to register hotkey for {shortcut.Name}: {shortcut.KeyDisplayName}");
                }
            }

            foreach (var clipboardShortcut in _settings.ClipboardShortcuts.Where(c => c.IsEnabled))
            {
                // Check if shortcut is enabled in current profile
                if (_profileManager != null && !_profileManager.IsClipboardShortcutEnabledInActiveProfile(clipboardShortcut.Id))
                    continue;
                    
                int hotkeyId = _hotkeyManager!.RegisterHotkey((uint)clipboardShortcut.Modifiers, (uint)clipboardShortcut.KeyCode);
                if (hotkeyId != -1)
                {
                    _hotkeyIdToClipboardShortcut[hotkeyId] = clipboardShortcut;
                    Logger.Debug($"Registered hotkey for {clipboardShortcut.Name}: {clipboardShortcut.KeyDisplayName}");
                }
                else
                {
                    Logger.Warning($"Failed to register hotkey for {clipboardShortcut.Name}: {clipboardShortcut.KeyDisplayName}");
                }
            }
            
            Logger.Info($"Registered {_hotkeyIdToShortcut.Count} app shortcuts and {_hotkeyIdToClipboardShortcut.Count} clipboard shortcuts");
        }


        private async void LaunchApplication(AppShortcut shortcut)
        {
            Logger.Info($"Launching shortcut: {shortcut.Name}");
            
            if (!shortcut.IsEnabled)
            {
                Logger.Warning($"Shortcut {shortcut.Name} is disabled");
                return;
            }
            
            // Check if enabled in active profile
            if (_profileManager != null && !_profileManager.IsShortcutEnabledInActiveProfile(shortcut.Id))
            {
                Logger.Info($"Shortcut {shortcut.Name} not enabled in active profile");
                if (_settings.ShowNotifications)
                    ToastNotification.Show($"{shortcut.Name} is not available in current profile");
                return;
            }
            
            if (shortcut.LaunchInParallel)
            {
                // Launch all apps simultaneously
                var tasks = shortcut.Apps.Select(app => System.Threading.Tasks.Task.Run(() => LaunchSingleApp(app, shortcut)));
                await System.Threading.Tasks.Task.WhenAll(tasks);
            }
            else
            {
                // Launch apps sequentially
                for (int i = 0; i < shortcut.Apps.Count; i++)
                {
                    var app = shortcut.Apps[i];
                    await LaunchSingleApp(app, shortcut);
                    
                    // Add delay between launches (except after the last app)
                    if (i < shortcut.Apps.Count - 1)
                    {
                        await ApplyDelay(app, shortcut);
                    }
                }
            }
            
            if (_settings.ShowNotifications)
                ToastNotification.Show($"Launched: {shortcut.Name}", 1500);
        }
        
        private async System.Threading.Tasks.Task LaunchSingleApp(AppInfo app, AppShortcut shortcut)
        {
            try
            {
                string processName = Path.GetFileNameWithoutExtension(app.ExePath);
                
                // Check launch conditions
                if (!CheckLaunchConditions(app.LaunchCondition))
                {
                    Logger.Info($"Launch conditions not met for {processName}");
                    return;
                }
                
                // Handle smart launching behavior
                if (app.LaunchCondition.CheckIfRunning && WindowManager.IsProcessRunning(processName))
                {
                    switch (app.LaunchCondition.Behavior)
                    {
                        case LaunchBehavior.LaunchIfNotRunning:
                            Logger.Info($"{processName} already running, skipping launch");
                            return;
                            
                        case LaunchBehavior.BringToFrontIfRunning:
                            Logger.Info($"Bringing {processName} to front");
                            WindowManager.BringProcessToFront(processName);
                            return;
                    }
                }
                
                if (!File.Exists(app.ExePath))
                {
                    string error = $"Application not found: {app.ExePath}";
                    Logger.Error(error);
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = app.ExePath,
                    Arguments = app.Arguments ?? "",
                    UseShellExecute = true
                };
                
                if (app.RunAsAdmin)
                {
                    startInfo.Verb = "runas";
                }
                
                Process? process = Process.Start(startInfo);
                Logger.Info($"Started {processName}");
                
                // Apply window positioning if configured
                if (process != null && app.WindowLayout != null)
                {
                    // Wait for window to appear
                    await System.Threading.Tasks.Task.Delay(500);
                    await WindowManager.WaitForWindow(processName, 5000);
                    
                    if (process.HasExited == false)
                    {
                        WindowManager.PositionWindow(process, app.WindowLayout);
                    }
                }
            }
            catch (Exception ex)
            {
                string error = $"Error launching {app.ExePath}: {ex.Message}";
                Logger.Error(error, ex);
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async System.Threading.Tasks.Task ApplyDelay(AppInfo app, AppShortcut shortcut)
        {
            if (shortcut.DelayType == DelayType.Fixed && shortcut.LaunchDelay > 0)
            {
                await System.Threading.Tasks.Task.Delay(shortcut.LaunchDelay);
            }
            else if (shortcut.DelayType == DelayType.WaitForProcess)
            {
                string processName = Path.GetFileNameWithoutExtension(app.ExePath);
                await WindowManager.WaitForProcess(processName, shortcut.MaxWaitTime);
            }
            else if (shortcut.DelayType == DelayType.WaitForWindow)
            {
                string processName = Path.GetFileNameWithoutExtension(app.ExePath);
                await WindowManager.WaitForWindow(processName, shortcut.MaxWaitTime);
            }
            else if (shortcut.DelayType == DelayType.WaitForNetwork)
            {
                int elapsed = 0;
                int pollInterval = 500;
                while (elapsed < shortcut.MaxWaitTime)
                {
                    if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                        break;
                    await System.Threading.Tasks.Task.Delay(pollInterval);
                    elapsed += pollInterval;
                }
            }
        }
        
        private bool CheckLaunchConditions(LaunchCondition condition)
        {
            // Check network requirement
            if (condition.RequireNetwork)
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    Logger.Warning("Network not available, launch condition not met");
                    return false;
                }
            }
            
            // Check battery level
            if (condition.MinBatteryLevel.HasValue)
            {
                PowerStatus powerStatus = SystemInformation.PowerStatus;
                float batteryPercent = powerStatus.BatteryLifePercent * 100;
                if (batteryPercent < condition.MinBatteryLevel.Value)
                {
                    Logger.Warning($"Battery level {batteryPercent}% below minimum {condition.MinBatteryLevel.Value}%");
                    return false;
                }
            }
            
            // Check AC power requirement
            if (condition.RequireAC)
            {
                PowerStatus powerStatus = SystemInformation.PowerStatus;
                if (powerStatus.PowerLineStatus != PowerLineStatus.Online)
                {
                    Logger.Warning("AC power not connected, launch condition not met");
                    return false;
                }
            }
            
            // Check required process
            if (!string.IsNullOrWhiteSpace(condition.RequiredProcess))
            {
                bool isRunning = WindowManager.IsProcessRunning(condition.RequiredProcess);
                if (isRunning != condition.RequiredProcessMustBeRunning)
                {
                    Logger.Warning($"Required process condition not met for {condition.RequiredProcess}");
                    return false;
                }
            }
            
            return true;
        }


        private void AddButton_Click(object? sender, EventArgs e)
        {
            if (_tabControl.SelectedIndex == 0) // App Shortcuts
            {
                using (var dialog = new ShortcutDialog(null, _settings))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _settings.Shortcuts.Add(dialog.Shortcut);
                        SettingsManager.SaveSettings(_settings);
                        LoadShortcuts();
                        RegisterAllHotkeys();
                    }
                }
            }
            else // Paste Shortcuts
            {
                using (var dialog = new ClipboardShortcutDialog(null, _settings))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _settings.ClipboardShortcuts.Add(dialog.Shortcut);
                        SettingsManager.SaveSettings(_settings);
                        LoadClipboardShortcuts();
                        RegisterAllHotkeys();
                    }
                }
            }
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            if (_tabControl.SelectedIndex == 0) // App Shortcuts
            {
                if (_shortcutsListView.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a shortcut to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selectedItem = _shortcutsListView.SelectedItems[0];
                var shortcut = (AppShortcut)selectedItem.Tag!;
                
                using (var dialog = new ShortcutDialog(shortcut, _settings))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        int index = _settings.Shortcuts.IndexOf(shortcut);
                        _settings.Shortcuts[index] = dialog.Shortcut;
                        SettingsManager.SaveSettings(_settings);
                        LoadShortcuts();
                        RegisterAllHotkeys();
                    }
                }
            }
            else // Paste Shortcuts
            {
                if (_clipboardShortcutsListView.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a paste shortcut to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selectedItem = _clipboardShortcutsListView.SelectedItems[0];
                var clipboardShortcut = (ClipboardShortcut)selectedItem.Tag!;
                
                using (var dialog = new ClipboardShortcutDialog(clipboardShortcut, _settings))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        int index = _settings.ClipboardShortcuts.IndexOf(clipboardShortcut);
                        _settings.ClipboardShortcuts[index] = dialog.Shortcut;
                        SettingsManager.SaveSettings(_settings);
                        LoadClipboardShortcuts();
                        RegisterAllHotkeys();
                    }
                }
            }
        }

        private void RemoveButton_Click(object? sender, EventArgs e)
        {
            if (_tabControl.SelectedIndex == 0) // App Shortcuts
            {
                if (_shortcutsListView.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a shortcut to remove.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selectedItem = _shortcutsListView.SelectedItems[0];
                var shortcut = (AppShortcut)selectedItem.Tag!;

                var result = MessageBox.Show($"Are you sure you want to remove '{shortcut.Name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _settings.Shortcuts.Remove(shortcut);
                    SettingsManager.SaveSettings(_settings);
                    LoadShortcuts();
                    RegisterAllHotkeys();
                }
            }
            else // Paste Shortcuts
            {
                if (_clipboardShortcutsListView.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Please select a paste shortcut to remove.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selectedItem = _clipboardShortcutsListView.SelectedItems[0];
                var clipboardShortcut = (ClipboardShortcut)selectedItem.Tag!;

                var result = MessageBox.Show($"Are you sure you want to remove '{clipboardShortcut.Name}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _settings.ClipboardShortcuts.Remove(clipboardShortcut);
                    SettingsManager.SaveSettings(_settings);
                    LoadClipboardShortcuts();
                    RegisterAllHotkeys();
                }
            }
        }


        private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
        {
            bool isAppShortcutsTab = _tabControl.SelectedIndex == 0;

            if (isAppShortcutsTab)
            {
                // App Shortcuts tab
                _addButton.Text = "Add Shortcut";
                _addButton.TextAlign = ContentAlignment.MiddleCenter;
                _addButton.Visible = true;
                _editButton.Visible = true;
                _removeButton.Visible = true;
                _importButton.Visible = true;
                _exportButton.Visible = true;
            }
            else
            {
                // Paste Shortcuts tab
                _addButton.Text = "Add Paste Shortcut";
                _addButton.TextAlign = ContentAlignment.MiddleCenter;
                _addButton.Visible = true;
                _editButton.Visible = true;
                _removeButton.Visible = true;
                _importButton.Visible = false;
                _exportButton.Visible = false;
            }
        }

        private void StartWithWindowsCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            _settings.StartWithWindows = _startWithWindowsCheckBox.Checked;
            StartupManager.SetStartup(_settings.StartWithWindows);
            SettingsManager.SaveSettings(_settings);
        }
        
        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            using (var dialog = new SettingsDialog(_settings))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Reload hotkeys with updated command palette hotkey
                    RegisterAllHotkeys();
                    
                    // Apply theme if changed
                    ApplyTheme();
                    
                    MessageBox.Show("Settings saved successfully!\n\nSome changes may require a restart.",
                        "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ImportButton_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                dialog.Title = "Import Shortcuts";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var importedSettings = SettingsManager.LoadSettingsFromFile(dialog.FileName);
                        
                        var result = MessageBox.Show(
                            $"Import {importedSettings.Shortcuts.Count} shortcuts?\n\nThis will replace your current shortcuts.",
                            "Confirm Import",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            _settings.Shortcuts = importedSettings.Shortcuts;
                            SettingsManager.SaveSettings(_settings);
                            LoadShortcuts();
                            RegisterAllHotkeys();
                            MessageBox.Show("Shortcuts imported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error importing shortcuts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                dialog.Title = "Export Shortcuts";
                dialog.FileName = "quicklauncher-shortcuts.json";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        SettingsManager.ExportSettings(_settings, dialog.FileName);
                        MessageBox.Show("Shortcuts exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting shortcuts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void PasteText(ClipboardShortcut clipboardShortcut)
        {
            try
            {
                Logger.Info($"Pasting: {clipboardShortcut.Name}");
                
                if (!clipboardShortcut.IsEnabled)
                {
                    Logger.Warning($"Clipboard shortcut {clipboardShortcut.Name} is disabled");
                    return;
                }
                
                // Check if enabled in active profile
                if (_profileManager != null && !_profileManager.IsClipboardShortcutEnabledInActiveProfile(clipboardShortcut.Id))
                {
                    Logger.Info($"Clipboard shortcut {clipboardShortcut.Name} not enabled in active profile");
                    if (_settings.ShowNotifications)
                        ToastNotification.Show($"{clipboardShortcut.Name} is not available in current profile");
                    return;
                }
                
                string content = clipboardShortcut.Content;
                
                // Process template variables if enabled
                if (clipboardShortcut.UseTemplateVariables)
                {
                    content = TemplateProcessor.ProcessTemplate(content);
                }
                
                // Handle alternative contents (cycling)
                if (clipboardShortcut.AlternativeContents.Count > 0)
                {
                    int index = clipboardShortcut.CurrentAlternativeIndex % (clipboardShortcut.AlternativeContents.Count + 1);
                    if (index > 0)
                    {
                        content = clipboardShortcut.AlternativeContents[index - 1];
                        if (clipboardShortcut.UseTemplateVariables)
                        {
                            content = TemplateProcessor.ProcessTemplate(content);
                        }
                    }
                    
                    // Update cycle index for next time
                    clipboardShortcut.CurrentAlternativeIndex = (index + 1) % (clipboardShortcut.AlternativeContents.Count + 1);
                    SettingsManager.SaveSettings(_settings);
                }
                
                // Set the text to clipboard
                if (clipboardShortcut.PreserveFormatting)
                {
                    // For HTML/Rich text, we'd need DataObject - for now just set as text
                    Clipboard.SetText(content, TextDataFormat.UnicodeText);
                }
                else
                {
                    Clipboard.SetText(content);
                }
                
                // Small delay to ensure clipboard is set
                System.Threading.Thread.Sleep(50);
                
                // Simulate Ctrl+V to paste
                SendKeys.SendWait("^v");
                
                if (_settings.ShowNotifications)
                    ToastNotification.Show($"Pasted: {clipboardShortcut.Name}", 1500);
            }
            catch (Exception ex)
            {
                string error = $"Error pasting text: {ex.Message}";
                Logger.Error(error, ex);
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotkeyManager.WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                
                // Check if it's the command palette hotkey
                if (hotkeyId == _commandPaletteHotkeyId)
                {
                    ShowCommandPalette();
                }
                else if (_hotkeyIdToShortcut.TryGetValue(hotkeyId, out AppShortcut? shortcut))
                {
                    LaunchApplication(shortcut);
                }
                else if (_hotkeyIdToClipboardShortcut.TryGetValue(hotkeyId, out ClipboardShortcut? clipboardShortcut))
                {
                    PasteText(clipboardShortcut);
                }
            }
            base.WndProc(ref m);
        }
        
        private void ShowCommandPalette()
        {
            try
            {
                using (var palette = new CommandPalette(_settings))
                {
                    if (palette.ShowDialog() == DialogResult.OK)
                    {
                        if (palette.SelectedAppShortcut != null)
                        {
                            LaunchApplication(palette.SelectedAppShortcut);
                        }
                        else if (palette.SelectedClipboardShortcut != null)
                        {
                            PasteText(palette.SelectedClipboardShortcut);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error showing command palette", ex);
            }
        }
        
        private void ProfileManager_ProfileChanged(object? sender, ProfileChangedEventArgs e)
        {
            Logger.Info($"Profile changed to: {e.NewProfile?.Name ?? "None"}");
            
            // Reload shortcuts to reflect profile changes
            LoadShortcuts();
            LoadClipboardShortcuts();
            RegisterAllHotkeys();
            
            if (_settings.ShowNotifications)
            {
                ToastNotification.Show($"Switched to profile: {e.NewProfile?.Name ?? "Default"}", 2000);
            }
        }
        
        private void ApplyTheme()
        {
            if (_settings.DarkMode)
            {
                // Modern dark theme with better contrast
                Color darkBg = Color.FromArgb(32, 32, 32);           // Main background
                Color darkControlBg = Color.FromArgb(45, 45, 48);    // Control background
                Color darkListBg = Color.FromArgb(30, 30, 30);       // List background
                Color darkBorder = Color.FromArgb(63, 63, 70);       // Borders
                Color brightText = Color.FromArgb(241, 241, 241);    // Primary text
                Color dimText = Color.FromArgb(204, 204, 204);       // Secondary text
                Color accentBlue = Color.FromArgb(0, 122, 204);      // Accent color
                
                this.BackColor = darkBg;
                this.ForeColor = brightText;
                
                // Tab control
                _tabControl.BackColor = darkBg;
                _tabControl.ForeColor = brightText;
                
                // ListViews with better visibility
                _shortcutsListView.BackColor = darkListBg;
                _shortcutsListView.ForeColor = brightText;
                _shortcutsListView.BorderStyle = BorderStyle.FixedSingle;
                
                _clipboardShortcutsListView.BackColor = darkListBg;
                _clipboardShortcutsListView.ForeColor = brightText;
                _clipboardShortcutsListView.BorderStyle = BorderStyle.FixedSingle;
                
                // Style all controls
                foreach (Control control in this.Controls)
                {
                    if (control is Button button)
                    {
                        button.BackColor = darkControlBg;
                        button.ForeColor = brightText;
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderColor = darkBorder;
                        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(62, 62, 64);
                        button.FlatAppearance.MouseDownBackColor = accentBlue;
                    }
                    else if (control is CheckBox checkbox)
                    {
                        checkbox.ForeColor = brightText;
                    }
                    else if (control is Label label)
                    {
                        label.ForeColor = dimText;
                    }
                }
            }
            else
            {
                // Reset to default light theme
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;
                
                _tabControl.BackColor = SystemColors.Control;
                _tabControl.ForeColor = SystemColors.ControlText;
                
                _shortcutsListView.BackColor = SystemColors.Window;
                _shortcutsListView.ForeColor = SystemColors.WindowText;
                _shortcutsListView.BorderStyle = BorderStyle.Fixed3D;
                
                _clipboardShortcutsListView.BackColor = SystemColors.Window;
                _clipboardShortcutsListView.ForeColor = SystemColors.WindowText;
                _clipboardShortcutsListView.BorderStyle = BorderStyle.Fixed3D;
                
                foreach (Control control in this.Controls)
                {
                    if (control is Button button)
                    {
                        button.BackColor = SystemColors.Control;
                        button.ForeColor = SystemColors.ControlText;
                        button.FlatStyle = FlatStyle.Standard;
                    }
                    else if (control is CheckBox checkbox)
                    {
                        checkbox.ForeColor = SystemColors.ControlText;
                    }
                    else if (control is Label label)
                    {
                        label.ForeColor = SystemColors.ControlText;
                    }
                }
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _profileManager?.StopAutoSwitching();
                _trayIcon?.Dispose();
                _trayMenu?.Dispose();
                _iconImageList?.Dispose();
                Logger.Info("QuickLauncher closed");
            }
            base.Dispose(disposing);
        }
    }
}
