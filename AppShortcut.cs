using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickLauncher
{
    public class AppInfo
    {
        public string ExePath { get; set; } = "";
        public string Arguments { get; set; } = "";
        public bool RunAsAdmin { get; set; } = false;
        
        // Window positioning
        public WindowLayout? WindowLayout { get; set; }
        
        // Launch conditions
        public LaunchCondition LaunchCondition { get; set; } = new LaunchCondition();
    }

    public class AppShortcut
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public List<AppInfo> Apps { get; set; } = new List<AppInfo>();
        public List<string> ExePaths 
        { 
            get => Apps.Select(a => a.ExePath).ToList();
            set 
            {
                // Backwards compatibility: convert old ExePaths to Apps
                if (value != null && Apps.Count == 0)
                {
                    foreach (var path in value)
                    {
                        Apps.Add(new AppInfo { ExePath = path });
                    }
                }
            }
        }
        
        // Keep for backwards compatibility with old settings files
        public string ExePath 
        { 
            get => Apps.FirstOrDefault()?.ExePath ?? "";
            set 
            {
                if (!string.IsNullOrWhiteSpace(value) && !Apps.Any(a => a.ExePath == value))
                {
                    Apps.Add(new AppInfo { ExePath = value });
                }
            }
        }
        
        public int Modifiers { get; set; } // Ctrl, Alt, Shift combination
        public int KeyCode { get; set; } // Virtual key code
        public string KeyDisplayName { get; set; } = "";
        public int LaunchDelay { get; set; } = 0; // Delay in milliseconds between launching apps
        public DelayType DelayType { get; set; } = DelayType.Fixed;
        public int MaxWaitTime { get; set; } = 10000; // Max time to wait for process/window (ms)
        public bool LaunchInParallel { get; set; } = false; // Launch all apps simultaneously
        
        // Grouping and organization
        public string Category { get; set; } = "General";
        public bool IsEnabled { get; set; } = true;
        
        public AppShortcut()
        {
        }

        public AppShortcut(string name, List<AppInfo> apps, int modifiers, int keyCode, string keyDisplayName, int launchDelay = 0)
        {
            Name = name;
            Apps = apps ?? new List<AppInfo>();
            Modifiers = modifiers;
            KeyCode = keyCode;
            KeyDisplayName = keyDisplayName;
            LaunchDelay = launchDelay;
        }
    }
}
