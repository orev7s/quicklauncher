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
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;

        public MainForm()
        {
            InitializeComponent();
            _settings = SettingsManager.LoadSettings();
            _hotkeyManager = new HotkeyManager(this.Handle);
            
            LoadShortcuts();
            LoadClipboardShortcuts();
            RegisterAllHotkeys();
            
            _startWithWindowsCheckBox.Checked = _settings.StartWithWindows;
            
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
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
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
                Size = new Size(120, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true
            };
            _addButton.Click += AddButton_Click;
            this.Controls.Add(_addButton);

            _editButton = new Button
            {
                Text = "Edit",
                Location = new Point(140, 410),
                Size = new Size(90, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true
            };
            _editButton.Click += EditButton_Click;
            this.Controls.Add(_editButton);

            _removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(240, 410),
                Size = new Size(90, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                UseVisualStyleBackColor = true
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
                UseVisualStyleBackColor = true
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
                UseVisualStyleBackColor = true
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

            foreach (var shortcut in _settings.Shortcuts)
            {
                int hotkeyId = _hotkeyManager!.RegisterHotkey((uint)shortcut.Modifiers, (uint)shortcut.KeyCode);
                if (hotkeyId != -1)
                {
                    _hotkeyIdToShortcut[hotkeyId] = shortcut;
                }
            }

            foreach (var clipboardShortcut in _settings.ClipboardShortcuts)
            {
                int hotkeyId = _hotkeyManager!.RegisterHotkey((uint)clipboardShortcut.Modifiers, (uint)clipboardShortcut.KeyCode);
                if (hotkeyId != -1)
                {
                    _hotkeyIdToClipboardShortcut[hotkeyId] = clipboardShortcut;
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == HotkeyManager.WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                if (_hotkeyIdToShortcut.TryGetValue(hotkeyId, out AppShortcut? shortcut))
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

        private void LaunchApplication(AppShortcut shortcut)
        {
            for (int i = 0; i < shortcut.Apps.Count; i++)
            {
                var app = shortcut.Apps[i];
                try
                {
                    if (File.Exists(app.ExePath))
                    {
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
                        
                        Process.Start(startInfo);
                        
                        // Add delay between launches (except after the last app)
                        if (i < shortcut.Apps.Count - 1 && shortcut.LaunchDelay > 0)
                        {
                            System.Threading.Thread.Sleep(shortcut.LaunchDelay);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Application not found: {app.ExePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching application {app.ExePath}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PasteText(ClipboardShortcut clipboardShortcut)
        {
            try
            {
                // Set the text to clipboard
                Clipboard.SetText(clipboardShortcut.Content);
                
                // Small delay to ensure clipboard is set
                System.Threading.Thread.Sleep(50);
                
                // Simulate Ctrl+V to paste
                SendKeys.SendWait("^v");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pasting text: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _trayIcon?.Dispose();
                _trayMenu?.Dispose();
                _iconImageList?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
