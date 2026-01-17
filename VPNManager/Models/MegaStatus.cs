using System;

namespace VPNManager.Models
{
    public class MegaStatus
    {
        public bool IsRunning { get; set; }
        public string ProcessName { get; set; } = "MEGAsync";
        public int? ProcessId { get; set; }
        public bool IsSyncing { get; set; }
        public string Status { get; set; } = "Not Running";
        public DateTime LastChecked { get; set; } = DateTime.Now;
    }
}
