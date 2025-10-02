using System;
using System.Drawing;

namespace QuickLauncher
{
    public class WindowLayout
    {
        public string ProcessName { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MonitorIndex { get; set; } = 0; // Which monitor to place the window on
        public WindowState State { get; set; } = WindowState.Normal;
        public bool RememberLayout { get; set; } = false;
    }

    public enum WindowState
    {
        Normal,
        Maximized,
        Minimized,
        Fullscreen
    }

    public enum LaunchBehavior
    {
        AlwaysLaunch,
        LaunchIfNotRunning,
        BringToFrontIfRunning,
        LaunchNewInstance
    }

    public enum DelayType
    {
        Fixed,
        WaitForProcess,
        WaitForWindow,
        WaitForNetwork
    }

    public class LaunchCondition
    {
        public bool CheckIfRunning { get; set; } = false;
        public LaunchBehavior Behavior { get; set; } = LaunchBehavior.AlwaysLaunch;
        
        // Additional conditions
        public bool RequireNetwork { get; set; } = false;
        public int? MinBatteryLevel { get; set; }
        public bool RequireAC { get; set; } = false;
        
        // Custom condition (process must be running/not running)
        public string? RequiredProcess { get; set; }
        public bool RequiredProcessMustBeRunning { get; set; } = true;
    }
}
