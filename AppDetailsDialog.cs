using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class AppDetailsDialog : Form
    {
        private TextBox _argumentsTextBox;
        private CheckBox _runAsAdminCheckBox;
        private Button _okButton;
        private Button _cancelButton;

        public AppInfo AppInfo { get; private set; }

        public AppDetailsDialog(AppInfo appInfo)
        {
            AppInfo = new AppInfo
            {
                ExePath = appInfo.ExePath,
                Arguments = appInfo.Arguments,
                RunAsAdmin = appInfo.RunAsAdmin
            };

            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Application Details";
            this.Size = new Size(500, 220);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Application Path (read-only)
            Label pathLabel = new Label
            {
                Text = "Application:",
                Location = new Point(10, 15),
                Size = new Size(100, 20)
            };
            this.Controls.Add(pathLabel);

            Label pathValue = new Label
            {
                Text = Path.GetFileName(AppInfo.ExePath),
                Location = new Point(120, 15),
                Size = new Size(350, 20),
                AutoEllipsis = true
            };
            this.Controls.Add(pathValue);

            // Arguments
            Label argsLabel = new Label
            {
                Text = "Arguments:",
                Location = new Point(10, 45),
                Size = new Size(100, 20)
            };
            this.Controls.Add(argsLabel);

            _argumentsTextBox = new TextBox
            {
                Location = new Point(120, 42),
                Size = new Size(350, 20),
                PlaceholderText = "Optional command-line arguments..."
            };
            this.Controls.Add(_argumentsTextBox);

            Label argsInfo = new Label
            {
                Text = "Example: --fullscreen, /config=myconfig.ini",
                Location = new Point(120, 67),
                Size = new Size(350, 20),
                ForeColor = Color.Gray
            };
            this.Controls.Add(argsInfo);

            // Run as Admin
            _runAsAdminCheckBox = new CheckBox
            {
                Text = "Run as Administrator",
                Location = new Point(120, 95),
                Size = new Size(350, 20)
            };
            this.Controls.Add(_runAsAdminCheckBox);

            Label adminInfo = new Label
            {
                Text = "⚠️ You will see a UAC prompt when launching this app",
                Location = new Point(120, 120),
                Size = new Size(350, 20),
                ForeColor = Color.DarkOrange,
                Visible = false
            };
            _runAsAdminCheckBox.CheckedChanged += (s, e) => adminInfo.Visible = _runAsAdminCheckBox.Checked;
            this.Controls.Add(adminInfo);

            // Buttons
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(310, 145),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(395, 145),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadData()
        {
            _argumentsTextBox.Text = AppInfo.Arguments;
            _runAsAdminCheckBox.Checked = AppInfo.RunAsAdmin;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            AppInfo.Arguments = _argumentsTextBox.Text;
            AppInfo.RunAsAdmin = _runAsAdminCheckBox.Checked;
        }
    }
}
