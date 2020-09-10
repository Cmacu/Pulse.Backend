using System;
using System.Collections.Generic;

namespace Pulse.Matchmaker.Matcher {
    public class SeekModel : IComparable<SeekModel> {
        public string Player { get; }
        public double Rating { get; }
        public IReadOnlyList<string> RecentOpponents;
        public DateTime JoinedAt { get; set; }

        public Boolean IsMatched { get; set; }

        public SeekModel(
            string player,
            double rating,
            IReadOnlyList<string> recentOpponents,
            DateTime? joinedAt = null
        ) {
            this.Player = player;
            this.Rating = rating;
            this.RecentOpponents = recentOpponents;
            this.JoinedAt = joinedAt ?? DateTime.UtcNow;
            this.IsMatched = false;
        }

        public int CompareTo(SeekModel other) {
            if (this.Player == other.Player) {
                return 0;
            }
            if (this.Rating >= other.Rating) {
                return 1;
            }
            return -1;
        }
    }
}