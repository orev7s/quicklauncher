using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickLauncher
{
    public static class WindowManager
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOZORDER = 0x0004;

        public static async Task<Process?> WaitForProcess(string processName, int maxWaitTime)
        {
            Logger.Debug($"Waiting for process: {processName}, max wait: {maxWaitTime}ms");
            
            int elapsed = 0;
            int pollInterval = 100;

            while (elapsed < maxWaitTime)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    Logger.Info($"Process {processName} found after {elapsed}ms");
                    return processes[0];
                }

                await Task.Delay(pollInterval);
                elapsed += pollInterval;
            }

            Logger.Warning($"Process {processName} not found within {maxWaitTime}ms");
            return null;
        }

        public static async Task<IntPtr> WaitForWindow(string processName, int maxWaitTime)
        {
            Process? process = await WaitForProcess(processName, maxWaitTime);
            if (process == null)
                return IntPtr.Zero;

            int elapsed = 0;
            int pollInterval = 100;

            while (elapsed < maxWaitTime)
            {
                IntPtr handle = GetMainWindowHandle(process);
                if (handle != IntPtr.Zero && IsWindowVisible(handle))
                {
                    Logger.Info($"Window for {processName} found after {elapsed}ms");
                    return handle;
                }

                await Task.Delay(pollInterval);
                elapsed += pollInterval;
            }

            Logger.Warning($"Window for {processName} not found within {maxWaitTime}ms");
            return IntPtr.Zero;
        }

        public static bool IsProcessRunning(string processName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool BringProcessToFront(string processName)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    IntPtr handle = GetMainWindowHandle(processes[0]);
                    if (handle != IntPtr.Zero)
                    {
                        ShowWindow(handle, SW_RESTORE);
                        SetForegroundWindow(handle);
                        Logger.Info($"Brought {processName} to front");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to bring {processName} to front", ex);
            }
            return false;
        }

        public static void PositionWindow(Process process, WindowLayout layout)
        {
            try
            {
                IntPtr handle = GetMainWindowHandle(process);
                if (handle == IntPtr.Zero)
                {
                    Logger.Warning($"Cannot position window: handle not found for process {process.ProcessName}");
                    return;
                }

                // Get target monitor bounds
                Screen targetScreen = GetTargetScreen(layout.MonitorIndex);
                
                // Adjust coordinates relative to monitor
                int x = targetScreen.Bounds.X + layout.X;
                int y = targetScreen.Bounds.Y + layout.Y;

                // Apply window state first
                switch (layout.State)
                {
                    case WindowState.Maximized:
                        ShowWindow(handle, SW_MAXIMIZE);
                        Logger.Info($"Maximized window for {process.ProcessName}");
                        return; // Don't set position for maximized window
                        
                    case WindowState.Minimized:
                        ShowWindow(handle, SW_MINIMIZE);
                        Logger.Info($"Minimized window for {process.ProcessName}");
                        return;
                        
                    case WindowState.Fullscreen:
                        // Set to monitor bounds for fullscreen
                        x = targetScreen.Bounds.X;
                        y = targetScreen.Bounds.Y;
                        layout.Width = targetScreen.Bounds.Width;
                        layout.Height = targetScreen.Bounds.Height;
                        break;
                }

                // Restore window if it was minimized
                ShowWindow(handle, SW_RESTORE);

                // Set position and size
                SetWindowPos(handle, IntPtr.Zero, x, y, layout.Width, layout.Height, 
                    SWP_NOZORDER | SWP_SHOWWINDOW);
                
                Logger.Info($"Positioned window for {process.ProcessName} at ({x}, {y}, {layout.Width}, {layout.Height})");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to position window for {process.ProcessName}", ex);
            }
        }

        private static IntPtr GetMainWindowHandle(Process process)
        {
            // First try the MainWindowHandle property
            if (process.MainWindowHandle != IntPtr.Zero)
                return process.MainWindowHandle;

            // If that doesn't work, enumerate windows to find one for this process
            IntPtr foundHandle = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                if (processId == process.Id && IsWindowVisible(hWnd))
                {
                    foundHandle = hWnd;
                    return false; // Stop enumeration
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            return foundHandle;
        }

        private static Screen GetTargetScreen(int monitorIndex)
        {
            Screen[] screens = Screen.AllScreens;
            if (monitorIndex >= 0 && monitorIndex < screens.Length)
                return screens[monitorIndex];
            
            return Screen.PrimaryScreen ?? screens[0];
        }
    }
}
