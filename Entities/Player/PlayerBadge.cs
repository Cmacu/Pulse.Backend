using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace Pulse.Entities.Player
{
    public class PlayerBadge
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public BadgeType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}