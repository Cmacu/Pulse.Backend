using System;

namespace Pulse.Ranking.Leaderboard {
    public class LeaderboardLogModel {
        public string Username { get; set; }
        public double LeaderboardRating { get; set; }
        public int TotalDecay { get; set; }
        public int Rank { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}