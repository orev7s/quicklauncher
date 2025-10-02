using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class SettingsDialog : Form
    {
        private Settings _settings;
        private CheckBox _darkModeCheckBox;
        private CheckBox _showNotificationsCheckBox;
        private CheckBox _enableLoggingCheckBox;
        private ComboBox _logLevelComboBox;
        private Button _manageProfilesButton;
        private Button _manageGroupsButton;
        private Button _viewLogsButton;
        private TextBox _commandPaletteHotkeyTextBox;
        private Button _okButton;
        private Button _cancelButton;
        private bool _capturingHotkey = false;
        private int _capturedModifiers = 0;
        private int _capturedKeyCode = 0;

        public SettingsDialog(Settings settings)
        {
            _settings = settings;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int y = 20;

            // Dark Mode
            _darkModeCheckBox = new CheckBox
            {
                Text = "Dark Mode (restart required)",
                Location = new Point(20, y),
                Size = new Size(300, 25)
            };
            this.Controls.Add(_darkModeCheckBox);
            y += 35;

            // Show Notifications
            _showNotificationsCheckBox = new CheckBox
            {
                Text = "Show toast notifications",
                Location = new Point(20, y),
                Size = new Size(300, 25)
            };
            this.Controls.Add(_showNotificationsCheckBox);
            y += 35;

            // Enable Logging
            _enableLoggingCheckBox = new CheckBox
            {
                Text = "Enable logging",
                Location = new Point(20, y),
                Size = new Size(300, 25)
            };
            this.Controls.Add(_enableLoggingCheckBox);
            y += 35;

            // Log Level
            Label logLevelLabel = new Label
            {
                Text = "Log Level:",
                Location = new Point(20, y + 3),
                Size = new Size(100, 20)
            };
            this.Controls.Add(logLevelLabel);

            _logLevelComboBox = new ComboBox
            {
                Location = new Point(130, y),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _logLevelComboBox.Items.AddRange(new object[] { "Debug", "Info", "Warning", "Error" });
            this.Controls.Add(_logLevelComboBox);
            y += 40;

            // Command Palette Hotkey
            Label hotkeyLabel = new Label
            {
                Text = "Command Palette Hotkey:",
                Location = new Point(20, y + 3),
                Size = new Size(180, 20)
            };
            this.Controls.Add(hotkeyLabel);

            _commandPaletteHotkeyTextBox = new TextBox
            {
                Location = new Point(210, y),
                Size = new Size(250, 25),
                ReadOnly = true,
                PlaceholderText = "Click and press hotkey..."
            };
            _commandPaletteHotkeyTextBox.Enter += HotkeyTextBox_Enter;
            _commandPaletteHotkeyTextBox.Leave += HotkeyTextBox_Leave;
            _commandPaletteHotkeyTextBox.KeyDown += HotkeyTextBox_KeyDown;
            this.Controls.Add(_commandPaletteHotkeyTextBox);
            y += 40;

            // Management Buttons
            _manageProfilesButton = new Button
            {
                Text = "Manage Profiles...",
                Location = new Point(20, y),
                Size = new Size(150, 35)
            };
            _manageProfilesButton.Click += ManageProfilesButton_Click;
            this.Controls.Add(_manageProfilesButton);

            _manageGroupsButton = new Button
            {
                Text = "Manage Groups...",
                Location = new Point(180, y),
                Size = new Size(150, 35)
            };
            _manageGroupsButton.Click += ManageGroupsButton_Click;
            this.Controls.Add(_manageGroupsButton);

            _viewLogsButton = new Button
            {
                Text = "View Logs...",
                Location = new Point(340, y),
                Size = new Size(120, 35)
            };
            _viewLogsButton.Click += ViewLogsButton_Click;
            this.Controls.Add(_viewLogsButton);
            y += 50;

            // OK/Cancel buttons
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(300, y + 20),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(385, y + 20),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadSettings()
        {
            _darkModeCheckBox.Checked = _settings.DarkMode;
            _showNotificationsCheckBox.Checked = _settings.ShowNotifications;
            _enableLoggingCheckBox.Checked = _settings.EnableLogging;
            _logLevelComboBox.SelectedItem = _settings.LogLevel.ToString();
            _commandPaletteHotkeyTextBox.Text = _settings.CommandPaletteHotkey;
            _capturedModifiers = _settings.CommandPaletteHotkeyModifiers;
            _capturedKeyCode = _settings.CommandPaletteHotkeyKeyCode;
        }

        private void HotkeyTextBox_Enter(object? sender, EventArgs e)
        {
            _capturingHotkey = true;
            _commandPaletteHotkeyTextBox.BackColor = Color.LightYellow;
        }

        private void HotkeyTextBox_Leave(object? sender, EventArgs e)
        {
            _capturingHotkey = false;
            _commandPaletteHotkeyTextBox.BackColor = SystemColors.Window;
        }

        private void HotkeyTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!_capturingHotkey)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            uint modifiers = 0;
            if (e.Control) modifiers |= HotkeyManager.MOD_CONTROL;
            if (e.Alt) modifiers |= HotkeyManager.MOD_ALT;
            if (e.Shift) modifiers |= HotkeyManager.MOD_SHIFT;

            Keys key = e.KeyCode;
            if (key == Keys.ControlKey || key == Keys.ShiftKey || key == Keys.Menu)
                return;

            if (modifiers == 0)
            {
                _commandPaletteHotkeyTextBox.Text = "Please use at least one modifier";
                return;
            }

            _capturedModifiers = (int)modifiers;
            _capturedKeyCode = (int)key;

            string displayName = "";
            if (e.Control) displayName += "Ctrl+";
            if (e.Alt) displayName += "Alt+";
            if (e.Shift) displayName += "Shift+";
            displayName += key.ToString();

            _commandPaletteHotkeyTextBox.Text = displayName;
        }

        private void ManageProfilesButton_Click(object? sender, EventArgs e)
        {
            using (var dialog = new ProfilesDialog(_settings))
            {
                dialog.ShowDialog();
            }
        }

        private void ManageGroupsButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Groups management dialog coming soon!", "Groups", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ViewLogsButton_Click(object? sender, EventArgs e)
        {
            try
            {
                string logPath = Logger.GetLogPath();
                string logDir = System.IO.Path.GetDirectoryName(logPath) ?? "";
                if (System.IO.Directory.Exists(logDir))
                {
                    System.Diagnostics.Process.Start("explorer.exe", logDir);
                }
                else
                {
                    MessageBox.Show("Log directory not found.", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            _settings.DarkMode = _darkModeCheckBox.Checked;
            _settings.ShowNotifications = _showNotificationsCheckBox.Checked;
            _settings.EnableLogging = _enableLoggingCheckBox.Checked;
            
            if (Enum.TryParse<LogLevel>(_logLevelComboBox.SelectedItem?.ToString(), out var logLevel))
            {
                _settings.LogLevel = logLevel;
            }

            _settings.CommandPaletteHotkey = _commandPaletteHotkeyTextBox.Text;
            _settings.CommandPaletteHotkeyModifiers = _capturedModifiers;
            _settings.CommandPaletteHotkeyKeyCode = _capturedKeyCode;

            SettingsManager.SaveSettings(_settings);
            Logger.IsEnabled = _settings.EnableLogging;
            Logger.MinimumLevel = _settings.LogLevel;
        }
    }
}
