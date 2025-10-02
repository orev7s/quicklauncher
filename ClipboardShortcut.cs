using System;

namespace QuickLauncher
{
    public class ClipboardShortcut
    {
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
        public int Modifiers { get; set; } // Ctrl, Alt, Shift combination
        public int KeyCode { get; set; } // Virtual key code
        public string KeyDisplayName { get; set; } = "";
        
        public ClipboardShortcut()
        {
        }

        public ClipboardShortcut(string name, string content, int modifiers, int keyCode, string keyDisplayName)
        {
            Name = name;
            Content = content;
            Modifiers = modifiers;
            KeyCode = keyCode;
            KeyDisplayName = keyDisplayName;
        }
    }
}
