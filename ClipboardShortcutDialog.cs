using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class ClipboardShortcutDialog : Form
    {
        private TextBox _nameTextBox;
        private TextBox _contentTextBox;
        private CheckBox _useTemplateVariablesCheckBox;
        private TextBox _shortcutTextBox;
        private Button _okButton;
        private Button _cancelButton;
        private bool _capturingHotkey = false;
        private int _capturedModifiers = 0;
        private int _capturedKeyCode = 0;
        private Settings? _allSettings;
        private ClipboardShortcut? _originalShortcut;

        public ClipboardShortcut Shortcut { get; private set; }

        public ClipboardShortcutDialog(ClipboardShortcut? existingShortcut = null, Settings? allSettings = null)
        {
            _originalShortcut = existingShortcut;
            _allSettings = allSettings;
            
            if (existingShortcut != null)
            {
                Shortcut = new ClipboardShortcut
                {
                    Name = existingShortcut.Name,
                    Content = existingShortcut.Content,
                    Modifiers = existingShortcut.Modifiers,
                    KeyCode = existingShortcut.KeyCode,
                    KeyDisplayName = existingShortcut.KeyDisplayName
                };
            }
            else
            {
                Shortcut = new ClipboardShortcut();
            }

            InitializeComponent();
            LoadShortcutData();
        }

        private void InitializeComponent()
        {
            this.Text = "Add/Edit Paste Shortcut";
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
                Size = new Size(370, 20),
                PlaceholderText = "E.g., 'Email Signature', 'Code Snippet'"
            };
            this.Controls.Add(_nameTextBox);

            // Content
            Label contentLabel = new Label
            {
                Text = "Content:",
                Location = new Point(10, 45),
                Size = new Size(100, 20)
            };
            this.Controls.Add(contentLabel);

            _contentTextBox = new TextBox
            {
                Location = new Point(120, 42),
                Size = new Size(370, 180),
                Multiline = true,
                AcceptsReturn = true,
                AcceptsTab = true,
                ScrollBars = ScrollBars.Vertical,
                PlaceholderText = "Enter the text you want to paste..."
            };
            this.Controls.Add(_contentTextBox);

            Label contentInfo = new Label
            {
                Text = "This text will be pasted when you press the shortcut",
                Location = new Point(120, 227),
                Size = new Size(370, 20),
                ForeColor = Color.Gray
            };
            this.Controls.Add(contentInfo);
            
            // Use Template Variables
            _useTemplateVariablesCheckBox = new CheckBox
            {
                Text = "Use template variables ({{date}}, {{username}}, etc.)",
                Location = new Point(120, 248),
                Size = new Size(370, 20)
            };
            this.Controls.Add(_useTemplateVariablesCheckBox);

            // Shortcut
            Label shortcutLabel = new Label
            {
                Text = "Shortcut:",
                Location = new Point(10, 280),
                Size = new Size(100, 20)
            };
            this.Controls.Add(shortcutLabel);

            _shortcutTextBox = new TextBox
            {
                Location = new Point(120, 277),
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
                Text = "Use Ctrl, Alt, Shift, or Win + a key (e.g., Ctrl+Alt+V)",
                Location = new Point(120, 305),
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
            _contentTextBox.Text = Shortcut.Content;
            _useTemplateVariablesCheckBox.Checked = Shortcut.UseTemplateVariables;
            _shortcutTextBox.Text = Shortcut.KeyDisplayName;
            _capturedModifiers = Shortcut.Modifiers;
            _capturedKeyCode = Shortcut.KeyCode;
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

            if (string.IsNullOrWhiteSpace(_contentTextBox.Text))
            {
                MessageBox.Show("Please enter content to paste.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (_capturedKeyCode == 0)
            {
                MessageBox.Show("Please set a keyboard shortcut.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            // Check for hotkey conflicts with app shortcuts
            if (_allSettings != null)
            {
                foreach (var existingShortcut in _allSettings.Shortcuts)
                {
                    if (existingShortcut.Modifiers == _capturedModifiers && existingShortcut.KeyCode == _capturedKeyCode)
                    {
                        MessageBox.Show(
                            $"The hotkey '{_shortcutTextBox.Text}' is already used by app shortcut '{existingShortcut.Name}'.\n\nPlease choose a different hotkey.",
                            "Hotkey Conflict",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                }

                foreach (var existingClipboardShortcut in _allSettings.ClipboardShortcuts)
                {
                    // Skip checking against itself when editing
                    if (_originalShortcut != null && existingClipboardShortcut == _originalShortcut)
                        continue;
                        
                    if (existingClipboardShortcut.Modifiers == _capturedModifiers && existingClipboardShortcut.KeyCode == _capturedKeyCode)
                    {
                        var result = MessageBox.Show(
                            $"The hotkey '{_shortcutTextBox.Text}' is already used by paste shortcut '{existingClipboardShortcut.Name}'.\n\nDo you want to use it anyway? (The other shortcut will stop working)",
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
            Shortcut.Content = _contentTextBox.Text;
            Shortcut.UseTemplateVariables = _useTemplateVariablesCheckBox.Checked;
            Shortcut.Modifiers = _capturedModifiers;
            Shortcut.KeyCode = _capturedKeyCode;
            Shortcut.KeyDisplayName = _shortcutTextBox.Text;
        }
    }
}
