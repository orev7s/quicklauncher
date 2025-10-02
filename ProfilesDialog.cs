using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class ProfilesDialog : Form
    {
        private Settings _settings;
        private ListBox _profilesListBox;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _activateButton;
        private Button _closeButton;
        private Label _activeProfileLabel;

        public ProfilesDialog(Settings settings)
        {
            _settings = settings;
            InitializeComponent();
            LoadProfiles();
        }

        private void InitializeComponent()
        {
            this.Text = "Manage Profiles";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(600, 450);

            _activeProfileLabel = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(570, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(_activeProfileLabel);

            _profilesListBox = new ListBox
            {
                Location = new Point(10, 45),
                Size = new Size(570, 290),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _profilesListBox.DoubleClick += EditButton_Click;
            this.Controls.Add(_profilesListBox);

            // Buttons
            _addButton = new Button
            {
                Text = "Add Profile",
                Location = new Point(10, 345),
                Size = new Size(110, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _addButton.Click += AddButton_Click;
            this.Controls.Add(_addButton);

            _editButton = new Button
            {
                Text = "Edit",
                Location = new Point(130, 345),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _editButton.Click += EditButton_Click;
            this.Controls.Add(_editButton);

            _deleteButton = new Button
            {
                Text = "Delete",
                Location = new Point(220, 345),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(_deleteButton);

            _activateButton = new Button
            {
                Text = "Activate",
                Location = new Point(310, 345),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            _activateButton.Click += ActivateButton_Click;
            this.Controls.Add(_activateButton);

            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(490, 345),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK
            };
            this.Controls.Add(_closeButton);
        }

        private void LoadProfiles()
        {
            _profilesListBox.Items.Clear();

            var activeProfile = _settings.Profiles.FirstOrDefault(p => p.Id == _settings.ActiveProfileId);
            _activeProfileLabel.Text = activeProfile != null 
                ? $"Active Profile: {activeProfile.Name}" 
                : "Active Profile: None (All shortcuts active)";

            foreach (var profile in _settings.Profiles)
            {
                string display = profile.Name;
                if (profile.Id == _settings.ActiveProfileId)
                    display += " [ACTIVE]";
                if (profile.Triggers.Count > 0)
                    display += $" ({profile.Triggers.Count} triggers)";

                _profilesListBox.Items.Add(new ListBoxItem { Display = display, Profile = profile });
            }
        }

        private void AddButton_Click(object? sender, EventArgs e)
        {
            var newProfile = new Profile
            {
                Name = "New Profile",
                Priority = _settings.Profiles.Count
            };

            using (var dialog = new ProfileEditDialog(newProfile, _settings))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _settings.Profiles.Add(dialog.Profile);
                    SettingsManager.SaveSettings(_settings);
                    LoadProfiles();
                }
            }
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            if (_profilesListBox.SelectedItem == null) return;

            var item = (ListBoxItem)_profilesListBox.SelectedItem;
            using (var dialog = new ProfileEditDialog(item.Profile, _settings))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    SettingsManager.SaveSettings(_settings);
                    LoadProfiles();
                }
            }
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            if (_profilesListBox.SelectedItem == null) return;

            var item = (ListBoxItem)_profilesListBox.SelectedItem;
            var result = MessageBox.Show(
                $"Delete profile '{item.Profile.Name}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (_settings.ActiveProfileId == item.Profile.Id)
                    _settings.ActiveProfileId = null;

                _settings.Profiles.Remove(item.Profile);
                SettingsManager.SaveSettings(_settings);
                LoadProfiles();
            }
        }

        private void ActivateButton_Click(object? sender, EventArgs e)
        {
            if (_profilesListBox.SelectedItem == null) return;

            var item = (ListBoxItem)_profilesListBox.SelectedItem;
            _settings.ActiveProfileId = item.Profile.Id;
            SettingsManager.SaveSettings(_settings);
            LoadProfiles();

            MessageBox.Show($"Activated profile: {item.Profile.Name}\n\nRestart QuickLauncher for changes to take effect.",
                "Profile Activated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private class ListBoxItem
        {
            public string Display { get; set; } = "";
            public Profile Profile { get; set; } = new Profile();
            public override string ToString() => Display;
        }
    }
}
