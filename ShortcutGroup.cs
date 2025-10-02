using System;
using System.Collections.Generic;

namespace QuickLauncher
{
    public class ShortcutGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "General"; // For organizing groups
        public bool IsEnabled { get; set; } = true;
        
        // Shortcuts in this group
        public List<string> ShortcutIds { get; set; } = new List<string>();
        public List<string> ClipboardShortcutIds { get; set; } = new List<string>();
    }
}
