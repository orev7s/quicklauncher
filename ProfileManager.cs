using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace QuickLauncher
{
    public class ProfileManager
    {
        private Settings _settings;
        private Timer? _evaluationTimer;
        private string? _previousActiveProfileId;

        public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;

        public ProfileManager(Settings settings)
        {
            _settings = settings;
            _previousActiveProfileId = _settings.ActiveProfileId;
        }

        public void StartAutoSwitching()
        {
            if (_evaluationTimer != null)
                return;

            _evaluationTimer = new Timer();
            _evaluationTimer.Interval = 5000; // Check every 5 seconds
            _evaluationTimer.Tick += EvaluationTimer_Tick;
            _evaluationTimer.Start();

            Logger.Info("Profile auto-switching started");
        }

        public void StopAutoSwitching()
        {
            if (_evaluationTimer != null)
            {
                _evaluationTimer.Stop();
                _evaluationTimer.Dispose();
                _evaluationTimer = null;
                Logger.Info("Profile auto-switching stopped");
            }
        }

        private void EvaluationTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                EvaluateProfiles();
            }
            catch (Exception ex)
            {
                Logger.Error("Error evaluating profiles", ex);
            }
        }

        public void EvaluateProfiles()
        {
            if (_settings.Profiles.Count == 0)
                return;

            // Find the highest priority profile that matches current conditions
            Profile? bestMatch = null;
            int highestPriority = int.MinValue;

            foreach (var profile in _settings.Profiles.Where(p => p.Triggers.Count > 0))
            {
                if (EvaluateProfile(profile) && profile.Priority > highestPriority)
                {
                    bestMatch = profile;
                    highestPriority = profile.Priority;
                }
            }

            // Switch to the best matching profile
            if (bestMatch != null && bestMatch.Id != _settings.ActiveProfileId)
            {
                SwitchToProfile(bestMatch.Id);
            }
        }

        private bool EvaluateProfile(Profile profile)
        {
            // All triggers must match for the profile to activate
            foreach (var trigger in profile.Triggers)
            {
                if (!EvaluateTrigger(trigger))
                    return false;
            }
            return true;
        }

        private bool EvaluateTrigger(ProfileTrigger trigger)
        {
            switch (trigger.Type)
            {
                case ProfileTriggerType.TimeOfDay:
                    return EvaluateTimeTrigger(trigger);

                case ProfileTriggerType.ConnectedMonitor:
                    return EvaluateMonitorTrigger(trigger);

                case ProfileTriggerType.ActiveApplication:
                    return EvaluateApplicationTrigger(trigger);

                case ProfileTriggerType.NetworkStatus:
                    return EvaluateNetworkTrigger(trigger);

                case ProfileTriggerType.BatteryLevel:
                    return EvaluateBatteryTrigger(trigger);

                case ProfileTriggerType.Manual:
                default:
                    return false; // Manual profiles don't auto-switch
            }
        }

        private bool EvaluateTimeTrigger(ProfileTrigger trigger)
        {
            if (!trigger.StartTime.HasValue || !trigger.EndTime.HasValue)
                return false;

            TimeSpan now = DateTime.Now.TimeOfDay;
            DayOfWeek today = DateTime.Now.DayOfWeek;

            // Check day of week if specified
            if (trigger.DaysOfWeek != null && trigger.DaysOfWeek.Count > 0)
            {
                if (!trigger.DaysOfWeek.Contains(today))
                    return false;
            }

            // Check time range
            TimeSpan start = trigger.StartTime.Value;
            TimeSpan end = trigger.EndTime.Value;

            if (start <= end)
            {
                return now >= start && now <= end;
            }
            else
            {
                // Time range crosses midnight
                return now >= start || now <= end;
            }
        }

        private bool EvaluateMonitorTrigger(ProfileTrigger trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger.MonitorName))
                return false;

            try
            {
                Screen[] screens = Screen.AllScreens;
                return screens.Any(s => s.DeviceName.Contains(trigger.MonitorName, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private bool EvaluateApplicationTrigger(ProfileTrigger trigger)
        {
            if (trigger.ProcessNames == null || trigger.ProcessNames.Count == 0)
                return false;

            try
            {
                Process[] allProcesses = Process.GetProcesses();
                return trigger.ProcessNames.Any(processName =>
                    allProcesses.Any(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                );
            }
            catch
            {
                return false;
            }
        }

        private bool EvaluateNetworkTrigger(ProfileTrigger trigger)
        {
            if (!trigger.RequireNetwork.HasValue)
                return true;

            try
            {
                bool isConnected = NetworkInterface.GetIsNetworkAvailable();
                return isConnected == trigger.RequireNetwork.Value;
            }
            catch
            {
                return false;
            }
        }

        private bool EvaluateBatteryTrigger(ProfileTrigger trigger)
        {
            try
            {
                PowerStatus powerStatus = SystemInformation.PowerStatus;
                float batteryPercent = powerStatus.BatteryLifePercent * 100;

                // Check battery level range
                if (trigger.MinBatteryLevel.HasValue && batteryPercent < trigger.MinBatteryLevel.Value)
                    return false;

                if (trigger.MaxBatteryLevel.HasValue && batteryPercent > trigger.MaxBatteryLevel.Value)
                    return false;

                // Check AC power requirement
                if (trigger.RequireAC.HasValue)
                {
                    bool isOnAC = powerStatus.PowerLineStatus == PowerLineStatus.Online;
                    if (isOnAC != trigger.RequireAC.Value)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SwitchToProfile(string profileId)
        {
            var profile = _settings.Profiles.FirstOrDefault(p => p.Id == profileId);
            if (profile == null)
            {
                Logger.Warning($"Profile not found: {profileId}");
                return;
            }

            string? previousProfileId = _settings.ActiveProfileId;
            _settings.ActiveProfileId = profileId;
            SettingsManager.SaveSettings(_settings);

            Logger.Info($"Switched to profile: {profile.Name}");

            // Fire profile changed event
            ProfileChanged?.Invoke(this, new ProfileChangedEventArgs
            {
                PreviousProfileId = previousProfileId,
                NewProfileId = profileId,
                NewProfile = profile
            });
        }

        public Profile? GetActiveProfile()
        {
            if (string.IsNullOrWhiteSpace(_settings.ActiveProfileId))
                return null;

            return _settings.Profiles.FirstOrDefault(p => p.Id == _settings.ActiveProfileId);
        }

        public bool IsShortcutEnabledInActiveProfile(string shortcutId)
        {
            var activeProfile = GetActiveProfile();
            if (activeProfile == null)
                return true; // No profile active, all shortcuts enabled

            return activeProfile.EnabledShortcutIds.Contains(shortcutId);
        }

        public bool IsClipboardShortcutEnabledInActiveProfile(string clipboardShortcutId)
        {
            var activeProfile = GetActiveProfile();
            if (activeProfile == null)
                return true; // No profile active, all shortcuts enabled

            return activeProfile.EnabledClipboardShortcutIds.Contains(clipboardShortcutId);
        }
    }

    public class ProfileChangedEventArgs : EventArgs
    {
        public string? PreviousProfileId { get; set; }
        public string? NewProfileId { get; set; }
        public Profile? NewProfile { get; set; }
    }
}
