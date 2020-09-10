using System;
using Pulse.Core.Players;

namespace Pulse.Core.PlayerSettings {
    public class PlayerSetting {
        public int Id { get; set; }
        public Player Player { get; set; }
        public int PlayerId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}