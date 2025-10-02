using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace QuickLauncher
{
    public class Settings
    {
        public List<AppShortcut> Shortcuts { get; set; } = new List<AppShortcut>();
        public List<ClipboardShortcut> ClipboardShortcuts { get; set; } = new List<ClipboardShortcut>();
        public bool StartWithWindows { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        
        // New features
        public List<Profile> Profiles { get; set; } = new List<Profile>();
        public string? ActiveProfileId { get; set; }
        public List<ShortcutGroup> Groups { get; set; } = new List<ShortcutGroup>();
        
        // UI settings
        public bool DarkMode { get; set; } = false;
        public bool ShowNotifications { get; set; } = true;
        public bool EnableLogging { get; set; } = true;
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        
        // Command palette settings
        public string CommandPaletteHotkey { get; set; } = "Ctrl+Shift+Space";
        public int CommandPaletteHotkeyModifiers { get; set; } = 0;
        public int CommandPaletteHotkeyKeyCode { get; set; } = 0;
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

        public static Settings LoadSettingsFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
        }

        public static void ExportSettings(Settings settings, string filePath)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
