using System;
using System.Collections.Generic;

namespace QuickLauncher
{
    public enum ProfileTriggerType
    {
        Manual,
        TimeOfDay,
        ConnectedMonitor,
        ActiveApplication,
        NetworkStatus,
        BatteryLevel
    }

    public class ProfileTrigger
    {
        public ProfileTriggerType Type { get; set; } = ProfileTriggerType.Manual;
        
        // Time-based triggers
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public List<DayOfWeek>? DaysOfWeek { get; set; }
        
        // Monitor-based triggers
        public string? MonitorName { get; set; }
        
        // Application-based triggers
        public List<string>? ProcessNames { get; set; }
        
        // Network-based triggers
        public bool? RequireNetwork { get; set; }
        
        // Battery-based triggers
        public int? MinBatteryLevel { get; set; }
        public int? MaxBatteryLevel { get; set; }
        public bool? RequireAC { get; set; }
    }

    public class Profile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsActive { get; set; } = false;
        public bool IsDefault { get; set; } = false;
        public int Priority { get; set; } = 0; // Higher priority profiles override lower ones
        
        // Auto-switching configuration
        public List<ProfileTrigger> Triggers { get; set; } = new List<ProfileTrigger>();
        
        // Shortcuts specific to this profile
        public List<string> EnabledShortcutIds { get; set; } = new List<string>();
        public List<string> EnabledClipboardShortcutIds { get; set; } = new List<string>();
        
        // Groups enabled in this profile
        public List<string> EnabledGroupIds { get; set; } = new List<string>();
    }
}
