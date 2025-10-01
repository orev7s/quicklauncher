using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickLauncher
{
    public class AppShortcut
    {
        public string Name { get; set; } = "";
        public List<string> ExePaths { get; set; } = new List<string>();
        
        // Keep for backwards compatibility with old settings files
        public string ExePath 
        { 
            get => ExePaths.FirstOrDefault() ?? "";
            set 
            {
                if (!string.IsNullOrWhiteSpace(value) && !ExePaths.Contains(value))
                {
                    ExePaths.Add(value);
                }
            }
        }
        
        public int Modifiers { get; set; } // Ctrl, Alt, Shift combination
        public int KeyCode { get; set; } // Virtual key code
        public string KeyDisplayName { get; set; } = "";
        
        public AppShortcut()
        {
        }

        public AppShortcut(string name, List<string> exePaths, int modifiers, int keyCode, string keyDisplayName)
        {
            Name = name;
            ExePaths = exePaths ?? new List<string>();
            Modifiers = modifiers;
            KeyCode = keyCode;
            KeyDisplayName = keyDisplayName;
        }
    }
}
