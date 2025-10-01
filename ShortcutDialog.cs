using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class ShortcutDialog : Form
    {
        private TextBox _nameTextBox;
        private ListBox _appsListBox;
        private Button _addAppButton;
        private Button _removeAppButton;
        private Button _editAppButton;
        private TextBox _shortcutTextBox;
        private NumericUpDown _delayNumeric;
        private Button _okButton;
        private Button _cancelButton;
        private bool _capturingHotkey = false;
        private int _capturedModifiers = 0;
        private int _capturedKeyCode = 0;
        private List<AppInfo> _appsList = new List<AppInfo>();
        private Settings? _allSettings;
        private AppShortcut? _originalShortcut;

        public AppShortcut Shortcut { get; private set; }

        public ShortcutDialog(AppShortcut? existingShortcut = null, Settings? allSettings = null)
        {
            _originalShortcut = existingShortcut;
            _allSettings = allSettings;
            
            if (existingShortcut != null)
            {
                Shortcut = new AppShortcut
                {
                    Name = existingShortcut.Name,
                    Apps = new System.Collections.Generic.List<AppInfo>(existingShortcut.Apps.Select(a => new AppInfo 
                    { 
                        ExePath = a.ExePath, 
                        Arguments = a.Arguments, 
                        RunAsAdmin = a.RunAsAdmin 
                    })),
                    Modifiers = existingShortcut.Modifiers,
                    KeyCode = existingShortcut.KeyCode,
                    KeyDisplayName = existingShortcut.KeyDisplayName,
                    LaunchDelay = existingShortcut.LaunchDelay
                };
                _appsList = new System.Collections.Generic.List<AppInfo>(Shortcut.Apps);
            }
            else
            {
                Shortcut = new AppShortcut();
            }

            InitializeComponent();
            LoadShortcutData();
        }

        private void InitializeComponent()
        {
            this.Text = "Add/Edit Shortcut";
            this.Size = new Size(520, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Name
            Label nameLabel = new Label
            {
                Text = "Name:",
                Location = new Point(10, 15),
                Size = new Size(100, 20)
            };
            this.Controls.Add(nameLabel);

            _nameTextBox = new TextBox
            {
                Location = new Point(120, 12),
                Size = new Size(370, 20)
            };
            this.Controls.Add(_nameTextBox);

            // Applications
            Label appsLabel = new Label
            {
                Text = "Applications:",
                Location = new Point(10, 45),
                Size = new Size(100, 20)
            };
            this.Controls.Add(appsLabel);

            _appsListBox = new ListBox
            {
                Location = new Point(120, 42),
                Size = new Size(370, 120)
            };
            this.Controls.Add(_appsListBox);

            _addAppButton = new Button
            {
                Text = "Add App...",
                Location = new Point(120, 170),
                Size = new Size(90, 25)
            };
            _addAppButton.Click += AddAppButton_Click;
            this.Controls.Add(_addAppButton);

            _editAppButton = new Button
            {
                Text = "Edit...",
                Location = new Point(220, 170),
                Size = new Size(90, 25)
            };
            _editAppButton.Click += EditAppButton_Click;
            this.Controls.Add(_editAppButton);

            _removeAppButton = new Button
            {
                Text = "Remove",
                Location = new Point(320, 170),
                Size = new Size(90, 25)
            };
            _removeAppButton.Click += RemoveAppButton_Click;
            this.Controls.Add(_removeAppButton);

            // Launch Delay
            Label delayLabel = new Label
            {
                Text = "Launch Delay:",
                Location = new Point(10, 210),
                Size = new Size(100, 20)
            };
            this.Controls.Add(delayLabel);

            _delayNumeric = new NumericUpDown
            {
                Location = new Point(120, 207),
                Size = new Size(100, 20),
                Minimum = 0,
                Maximum = 10000,
                Increment = 100,
                Value = 0
            };
            this.Controls.Add(_delayNumeric);

            Label delayInfo = new Label
            {
                Text = "ms (delay between launching apps)",
                Location = new Point(230, 210),
                Size = new Size(260, 20),
                ForeColor = Color.Gray
            };
            this.Controls.Add(delayInfo);

            // Shortcut
            Label shortcutLabel = new Label
            {
                Text = "Shortcut:",
                Location = new Point(10, 245),
                Size = new Size(100, 20)
            };
            this.Controls.Add(shortcutLabel);

            _shortcutTextBox = new TextBox
            {
                Location = new Point(120, 242),
                Size = new Size(370, 20),
                ReadOnly = true,
                PlaceholderText = "Click here and press your desired shortcut..."
            };
            _shortcutTextBox.Enter += ShortcutTextBox_Enter;
            _shortcutTextBox.Leave += ShortcutTextBox_Leave;
            _shortcutTextBox.KeyDown += ShortcutTextBox_KeyDown;
            _shortcutTextBox.KeyUp += ShortcutTextBox_KeyUp;
            this.Controls.Add(_shortcutTextBox);

            // Info label
            Label infoLabel = new Label
            {
                Text = "Use Ctrl, Alt, Shift, or Win + a key (e.g., Ctrl+Alt+A)",
                Location = new Point(120, 270),
                Size = new Size(370, 20),
                ForeColor = Color.Gray
            };
            this.Controls.Add(infoLabel);

            // Buttons
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(330, 360),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(415, 360),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadShortcutData()
        {
            _nameTextBox.Text = Shortcut.Name;
            _delayNumeric.Value = Shortcut.LaunchDelay;
            RefreshAppsList();
            _shortcutTextBox.Text = Shortcut.KeyDisplayName;
            _capturedModifiers = Shortcut.Modifiers;
            _capturedKeyCode = Shortcut.KeyCode;
        }

        private void RefreshAppsList()
        {
            _appsListBox.Items.Clear();
            foreach (var app in _appsList)
            {
                string display = Path.GetFileName(app.ExePath);
                if (!string.IsNullOrWhiteSpace(app.Arguments))
                    display += $" [{app.Arguments}]";
                if (app.RunAsAdmin)
                    display += " [Admin]";
                _appsListBox.Items.Add(display);
            }
        }

        private void AddAppButton_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";
                dialog.Title = "Select Application";
                dialog.Multiselect = true;
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var fileName in dialog.FileNames)
                    {
                        if (!_appsList.Any(a => a.ExePath == fileName))
                        {
                            var appInfo = new AppInfo { ExePath = fileName };
                            
                            // Show app details dialog
                            using (var detailsDialog = new AppDetailsDialog(appInfo))
                            {
                                if (detailsDialog.ShowDialog() == DialogResult.OK)
                                {
                                    _appsList.Add(detailsDialog.AppInfo);
                                }
                            }
                        }
                    }
                    
                    RefreshAppsList();
                    
                    if (string.IsNullOrWhiteSpace(_nameTextBox.Text) && dialog.FileNames.Length == 1)
                    {
                        _nameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileNames[0]);
                    }
                }
            }
        }

        private void EditAppButton_Click(object? sender, EventArgs e)
        {
            if (_appsListBox.SelectedIndex >= 0)
            {
                var app = _appsList[_appsListBox.SelectedIndex];
                using (var detailsDialog = new AppDetailsDialog(app))
                {
                    if (detailsDialog.ShowDialog() == DialogResult.OK)
                    {
                        _appsList[_appsListBox.SelectedIndex] = detailsDialog.AppInfo;
                        RefreshAppsList();
                    }
                }
            }
        }

        private void RemoveAppButton_Click(object? sender, EventArgs e)
        {
            if (_appsListBox.SelectedIndex >= 0)
            {
                _appsList.RemoveAt(_appsListBox.SelectedIndex);
                RefreshAppsList();
            }
        }

        private void ShortcutTextBox_Enter(object? sender, EventArgs e)
        {
            _capturingHotkey = true;
            _shortcutTextBox.BackColor = Color.LightYellow;
        }

        private void ShortcutTextBox_Leave(object? sender, EventArgs e)
        {
            _capturingHotkey = false;
            _shortcutTextBox.BackColor = SystemColors.Window;
        }

        private void ShortcutTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!_capturingHotkey)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            // Get modifiers
            uint modifiers = 0;
            if (e.Control) modifiers |= HotkeyManager.MOD_CONTROL;
            if (e.Alt) modifiers |= HotkeyManager.MOD_ALT;
            if (e.Shift) modifiers |= HotkeyManager.MOD_SHIFT;

            // Get the actual key (not the modifier)
            Keys key = e.KeyCode;
            
            // Skip if only modifier keys are pressed
            if (key == Keys.ControlKey || key == Keys.ShiftKey || key == Keys.Menu || key == Keys.LWin || key == Keys.RWin)
                return;

            // Require at least one modifier
            if (modifiers == 0)
            {
                _shortcutTextBox.Text = "Please use at least one modifier (Ctrl, Alt, Shift)";
                return;
            }

            _capturedModifiers = (int)modifiers;
            _capturedKeyCode = (int)key;

            // Build display name
            string displayName = "";
            if (e.Control) displayName += "Ctrl+";
            if (e.Alt) displayName += "Alt+";
            if (e.Shift) displayName += "Shift+";
            displayName += key.ToString();

            _shortcutTextBox.Text = displayName;
        }

        private void ShortcutTextBox_KeyUp(object? sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("Please enter a name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (_appsList.Count == 0)
            {
                MessageBox.Show("Please add at least one application.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Validate all paths exist
            foreach (var app in _appsList)
            {
                if (!File.Exists(app.ExePath))
                {
                    MessageBox.Show($"Application not found: {app.ExePath}", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }

            if (_capturedKeyCode == 0)
            {
                MessageBox.Show("Please set a keyboard shortcut.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Check for hotkey conflicts
            if (_allSettings != null)
            {
                foreach (var existingShortcut in _allSettings.Shortcuts)
                {
                    // Skip checking against itself when editing
                    if (_originalShortcut != null && existingShortcut == _originalShortcut)
                        continue;
                        
                    if (existingShortcut.Modifiers == _capturedModifiers && existingShortcut.KeyCode == _capturedKeyCode)
                    {
                        var result = MessageBox.Show(
                            $"The hotkey '{_shortcutTextBox.Text}' is already used by '{existingShortcut.Name}'.\n\nDo you want to use it anyway? (The other shortcut will stop working)",
                            "Hotkey Conflict",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);
                        
                        if (result == DialogResult.No)
                        {
                            this.DialogResult = DialogResult.None;
                            return;
                        }
                        break;
                    }
                }
            }

            // Save
            Shortcut.Name = _nameTextBox.Text;
            Shortcut.Apps.Clear();
            Shortcut.Apps.AddRange(_appsList);
            Shortcut.Modifiers = _capturedModifiers;
            Shortcut.KeyCode = _capturedKeyCode;
            Shortcut.KeyDisplayName = _shortcutTextBox.Text;
            Shortcut.LaunchDelay = (int)_delayNumeric.Value;
        }
    }
}
