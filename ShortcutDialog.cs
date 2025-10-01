using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class ShortcutDialog : Form
    {
        private TextBox _nameTextBox;
        private ListBox _appsListBox;
        private Button _addAppButton;
        private Button _removeAppButton;
        private TextBox _shortcutTextBox;
        private Button _okButton;
        private Button _cancelButton;
        private bool _capturingHotkey = false;
        private int _capturedModifiers = 0;
        private int _capturedKeyCode = 0;

        public AppShortcut Shortcut { get; private set; }

        public ShortcutDialog(AppShortcut? existingShortcut = null)
        {
            if (existingShortcut != null)
            {
                Shortcut = new AppShortcut
                {
                    Name = existingShortcut.Name,
                    ExePaths = new System.Collections.Generic.List<string>(existingShortcut.ExePaths),
                    Modifiers = existingShortcut.Modifiers,
                    KeyCode = existingShortcut.KeyCode,
                    KeyDisplayName = existingShortcut.KeyDisplayName
                };
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
            this.Size = new Size(500, 380);
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
                Size = new Size(350, 20)
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
                Size = new Size(350, 120)
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

            _removeAppButton = new Button
            {
                Text = "Remove",
                Location = new Point(220, 170),
                Size = new Size(90, 25)
            };
            _removeAppButton.Click += RemoveAppButton_Click;
            this.Controls.Add(_removeAppButton);

            // Shortcut
            Label shortcutLabel = new Label
            {
                Text = "Shortcut:",
                Location = new Point(10, 210),
                Size = new Size(100, 20)
            };
            this.Controls.Add(shortcutLabel);

            _shortcutTextBox = new TextBox
            {
                Location = new Point(120, 207),
                Size = new Size(350, 20),
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
                Location = new Point(120, 235),
                Size = new Size(350, 20),
                ForeColor = Color.Gray
            };
            this.Controls.Add(infoLabel);

            // Buttons
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(310, 300),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(395, 300),
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
            _appsListBox.Items.Clear();
            foreach (var exePath in Shortcut.ExePaths)
            {
                _appsListBox.Items.Add(exePath);
            }
            _shortcutTextBox.Text = Shortcut.KeyDisplayName;
            _capturedModifiers = Shortcut.Modifiers;
            _capturedKeyCode = Shortcut.KeyCode;
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
                        if (!_appsListBox.Items.Contains(fileName))
                        {
                            _appsListBox.Items.Add(fileName);
                        }
                    }
                    
                    if (string.IsNullOrWhiteSpace(_nameTextBox.Text) && dialog.FileNames.Length == 1)
                    {
                        _nameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileNames[0]);
                    }
                }
            }
        }

        private void RemoveAppButton_Click(object? sender, EventArgs e)
        {
            if (_appsListBox.SelectedIndex >= 0)
            {
                _appsListBox.Items.RemoveAt(_appsListBox.SelectedIndex);
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

            if (_appsListBox.Items.Count == 0)
            {
                MessageBox.Show("Please add at least one application.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Validate all paths exist
            foreach (var item in _appsListBox.Items)
            {
                string path = item.ToString()!;
                if (!File.Exists(path))
                {
                    MessageBox.Show($"Application not found: {path}", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            // Save
            Shortcut.Name = _nameTextBox.Text;
            Shortcut.ExePaths.Clear();
            foreach (var item in _appsListBox.Items)
            {
                Shortcut.ExePaths.Add(item.ToString()!);
            }
            Shortcut.Modifiers = _capturedModifiers;
            Shortcut.KeyCode = _capturedKeyCode;
            Shortcut.KeyDisplayName = _shortcutTextBox.Text;
        }
    }
}
