namespace Pulse.Core.PlayerSettings {
    public class PlayerSettingsModel {
        public bool EmailNotifications { get; set; }

        public PlayerSettingsModel() {
            EmailNotifications = true;
        }
    }
}