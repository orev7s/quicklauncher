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

        // UI Controls
        private ListView _shortcutsListView;
        private ImageList _iconImageList;
        private Button _addButton;
        private Button _editButton;
        private Button _removeButton;
        private Button _importButton;
        private Button _exportButton;
        private CheckBox _startWithWindowsCheckBox;
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;

        public MainForm()
        {
            InitializeComponent();
            _settings = SettingsManager.LoadSettings();
            _hotkeyManager = new HotkeyManager(this.Handle);
            
            LoadShortcuts();
            RegisterAllHotkeys();
            
            _startWithWindowsCheckBox.Checked = _settings.StartWithWindows;
            
            SetupTrayIcon();
        }

        private void InitializeComponent()
        {
            this.Text = "QuickLauncher - Global App Shortcuts";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);
            this.FormClosing += MainForm_FormClosing;

            // ImageList for icons
            _iconImageList = new ImageList
            {
                ImageSize = new Size(16, 16),
                ColorDepth = ColorDepth.Depth32Bit
            };

            // ListView for shortcuts
            _shortcutsListView = new ListView
            {
                Location = new Point(10, 10),
                Size = new Size(660, 350),
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
            this.Controls.Add(_shortcutsListView);

            // Buttons
            _addButton = new Button
            {
                Text = "Add Shortcut",
                Location = new Point(10, 370),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _addButton.Click += AddButton_Click;
            this.Controls.Add(_addButton);

            _editButton = new Button
            {
                Text = "Edit",
                Location = new Point(120, 370),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _editButton.Click += EditButton_Click;
            this.Controls.Add(_editButton);

            _removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(230, 370),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _removeButton.Click += RemoveButton_Click;
            this.Controls.Add(_removeButton);

            _importButton = new Button
            {
                Text = "Import...",
                Location = new Point(350, 370),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _importButton.Click += ImportButton_Click;
            this.Controls.Add(_importButton);

            _exportButton = new Button
            {
                Text = "Export...",
                Location = new Point(460, 370),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _exportButton.Click += ExportButton_Click;
            this.Controls.Add(_exportButton);

            // Settings
            _startWithWindowsCheckBox = new CheckBox
            {
                Text = "Start with Windows",
                Location = new Point(10, 410),
                Size = new Size(200, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _startWithWindowsCheckBox.CheckedChanged += StartWithWindowsCheckBox_CheckedChanged;
            this.Controls.Add(_startWithWindowsCheckBox);

            // Info label
            Label infoLabel = new Label
            {
                Text = "Add applications with keyboard shortcuts. The shortcuts will work globally.",
                Location = new Point(10, 440),
                Size = new Size(660, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(infoLabel);
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

        private void RegisterAllHotkeys()
        {
            _hotkeyManager?.UnregisterAll();
            _hotkeyIdToShortcut.Clear();

            foreach (var shortcut in _settings.Shortcuts)
            {
                int hotkeyId = _hotkeyManager!.RegisterHotkey((uint)shortcut.Modifiers, (uint)shortcut.KeyCode);
                if (hotkeyId != -1)
                {
                    _hotkeyIdToShortcut[hotkeyId] = shortcut;
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

        private void AddButton_Click(object? sender, EventArgs e)
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

        private void EditButton_Click(object? sender, EventArgs e)
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

        private void RemoveButton_Click(object? sender, EventArgs e)
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
