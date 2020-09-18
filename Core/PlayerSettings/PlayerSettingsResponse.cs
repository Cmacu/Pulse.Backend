namespace Pulse.Core.PlayerSettings {
    public class PlayerSettingsResponse {
        public bool EmailNotifications { get; set; }

        public PlayerSettingsResponse() {
            EmailNotifications = true;
        }
    }
}