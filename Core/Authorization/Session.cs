using System;
using Pulse.Core.Players;

namespace Pulse.Core.Authorization {
    public class Session {
        public int Id { get; set; }
        public Player Player { get; set; }
        public int PlayerId { get; set; }
        public string RefreshToken { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}