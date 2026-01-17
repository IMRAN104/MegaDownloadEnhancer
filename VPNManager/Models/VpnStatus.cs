using System;

namespace VPNManager.Models
{
    public class VpnStatus
    {
        public bool IsConnected { get; set; }
        public string VpnName { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public DateTime? LastStateChange { get; set; }
        public string CurrentState { get; set; } = "Disconnected";
        public string LastError { get; set; } = string.Empty;
        public int CycleCount { get; set; }
        public TimeSpan? TimeUntilNextToggle { get; set; }
    }
}
