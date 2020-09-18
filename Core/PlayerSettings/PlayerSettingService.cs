using System;
using System.Linq;
using Pulse.Backend;

namespace Pulse.Core.PlayerSettings {
    public class PlayerSettingService {
        private readonly DataContext _context;

        public PlayerSettingService(DataContext context) {
            _context = context;
        }

        /// <summary>
        /// Fetches all settings for a given player. Uses default values from the SettingsModel constructor.
        /// </summary>
        /// <param name="playerId">The ID of the player to fetch settings.</param>
        /// <returns>The settings for the given player.</returns>
        public PlayerSettingsResponse Get(int playerId) {
            var settings = _context.PlayerSettings.Where(x => x.PlayerId == playerId);
            var model = new PlayerSettingsResponse();

            foreach (var setting in settings) {
                var prop = model.GetType().GetProperty(setting.Name);

                var typedValue = Convert.ChangeType(setting.Value, prop.PropertyType);

                prop.SetValue(model, typedValue, null);
            }

            return model;
        }

        /// <summary>
        /// Sets the specified player setting to the specified value.
        /// </summary>
        /// <param name="playerId">The ID of the player to receive the setting.</param>
        /// <param name="name">The name of the setting, e.g. "EmailNotifications"</param>
        /// <param name="value">The value of the setting, e.g. "1"</param>
        public void Set(int playerId, string name, string value) {
            var properties = typeof(PlayerSettingsResponse).GetProperties();
            if (!properties.Any(x => x.Name == name))
                throw new InvalidOperationException($"Setting not available: {name}");

            var row = _context.PlayerSettings.FirstOrDefault(x => x.PlayerId == playerId && x.Name == name);
            if (row == null) {
                row = new PlayerSetting() {
                PlayerId = playerId,
                Name = name,
                };
                _context.PlayerSettings.Add(row);
            }

            row.Value = value;
            row.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
        }
    }
}