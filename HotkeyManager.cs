using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WM_HOTKEY = 0x0312;

        // Modifier keys
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        private IntPtr _windowHandle;
        private int _currentId = 1;

        public HotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public int RegisterHotkey(uint modifiers, uint key)
        {
            int id = _currentId++;
            if (!RegisterHotKey(_windowHandle, id, modifiers, key))
            {
                return -1; // Failed to register
            }
            return id;
        }

        public void UnregisterHotkey(int id)
        {
            UnregisterHotKey(_windowHandle, id);
        }

        public void UnregisterAll()
        {
            for (int i = 1; i < _currentId; i++)
            {
                UnregisterHotKey(_windowHandle, i);
            }
            _currentId = 1;
        }
    }
}
