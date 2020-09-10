using System;
using Pulse.Core.Players;

namespace Pulse.Ranking.Rating {
    public class PlayerBadge {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public BadgeType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}