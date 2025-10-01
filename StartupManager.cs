using System;
using Microsoft.Win32;
using System.Windows.Forms;

namespace QuickLauncher
{
    public static class StartupManager
    {
        private const string AppName = "QuickLauncher";
        private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public static void SetStartup(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true)!)
                {
                    if (enable)
                    {
                        string exePath = Application.ExecutablePath;
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                    else
                    {
                        key.DeleteValue(AppName, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool IsStartupEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false)!)
                {
                    object? value = key?.GetValue(AppName);
                    return value != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
