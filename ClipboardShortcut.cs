using System;
using System.Collections.Generic;

namespace QuickLauncher
{
    public class ClipboardShortcut
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
        public int Modifiers { get; set; } // Ctrl, Alt, Shift combination
        public int KeyCode { get; set; } // Virtual key code
        public string KeyDisplayName { get; set; } = "";
        
        // Enhanced features
        public bool UseTemplateVariables { get; set; } = false;
        public bool PreserveFormatting { get; set; } = false;
        public string Category { get; set; } = "General";
        public bool IsEnabled { get; set; } = true;
        
        // Multiple snippets cycling
        public List<string> AlternativeContents { get; set; } = new List<string>();
        public int CurrentAlternativeIndex { get; set; } = 0;
        
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
