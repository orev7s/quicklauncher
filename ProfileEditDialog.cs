using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class ProfileEditDialog : Form
    {
        private Settings _settings;
        private TextBox _nameTextBox;
        private TextBox _descriptionTextBox;
        private NumericUpDown _priorityNumeric;
        private Button _okButton;
        private Button _cancelButton;

        public Profile Profile { get; private set; }

        public ProfileEditDialog(Profile profile, Settings settings)
        {
            Profile = profile;
            _settings = settings;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Edit Profile";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

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

            Label descLabel = new Label
            {
                Text = "Description:",
                Location = new Point(10, 45),
                Size = new Size(100, 20)
            };
            this.Controls.Add(descLabel);

            _descriptionTextBox = new TextBox
            {
                Location = new Point(120, 42),
                Size = new Size(350, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(_descriptionTextBox);

            Label priorityLabel = new Label
            {
                Text = "Priority:",
                Location = new Point(10, 135),
                Size = new Size(100, 20)
            };
            this.Controls.Add(priorityLabel);

            _priorityNumeric = new NumericUpDown
            {
                Location = new Point(120, 132),
                Size = new Size(100, 20),
                Minimum = 0,
                Maximum = 100
            };
            this.Controls.Add(_priorityNumeric);

            Label priorityInfo = new Label
            {
                Text = "Higher priority profiles override lower ones when conditions match",
                Location = new Point(120, 157),
                Size = new Size(350, 40),
                ForeColor = Color.Gray
            };
            this.Controls.Add(priorityInfo);

            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(310, 220),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(395, 220),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadData()
        {
            _nameTextBox.Text = Profile.Name;
            _descriptionTextBox.Text = Profile.Description;
            _priorityNumeric.Value = Profile.Priority;
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("Please enter a profile name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            Profile.Name = _nameTextBox.Text;
            Profile.Description = _descriptionTextBox.Text;
            Profile.Priority = (int)_priorityNumeric.Value;
        }
    }
}
