using System;
using System.Collections.Generic;

namespace Pulse.Matchmaker.Matches {
    public class Match {
        public int Id { get; set; }
        public string Name { get; set; }
        public MatchStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<MatchPlayer> MatchPlayers { get; set; } = new List<MatchPlayer>();
        public DateTime UpdatedAt { get; set; }
    }
}