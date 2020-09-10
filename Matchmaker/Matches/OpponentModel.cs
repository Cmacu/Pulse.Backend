using System.Collections.Generic;
using Pulse.Ranking.Rating;

namespace Pulse.Matchmaker.Matches {
    public class OpponentModel {
        public int Id { get; set; }
        public string Avatar { get; set; }
        public string Username { get; set; }
        public int Position { get; set; }
        public Division Division { get; set; }
        public int Level { get; set; }
        public int Score { get; set; }
        public bool IsWin { get; set; }
        public string Country { get; set; }
        public bool IsResigned { get; set; }
        public bool IsExpired { get; set; }
        public List<string> Cards { get; set; }
        public double RatingDelta { get; set; }
        public int DecayValue { get; set; }
    }
}