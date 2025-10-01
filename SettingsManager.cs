using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace QuickLauncher
{
    public class Settings
    {
        public List<AppShortcut> Shortcuts { get; set; } = new List<AppShortcut>();
        public bool StartWithWindows { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuickLauncher",
            "settings.json"
        );

        public static Settings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading settings: {ex.Message}");
            }

            return new Settings();
        }

        public static void SaveSettings(Settings settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error saving settings: {ex.Message}");
            }
        }
    }
}
