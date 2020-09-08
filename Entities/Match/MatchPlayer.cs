using System.Collections.Generic;
using Pulse.Entities.Player;

namespace Pulse.Entities.Match
{
    public class MatchPlayer
    {
        public int Id { get; set; }
        public Match Match { get; set; }
        public int MatchId { get; set; }
        public MatchStatus Status { get; set; }

        public Player.Player Player { get; set; }
        public int PlayerId { get; set; }

        public int Position { get; set; }
        public int Score { get; set; }
        public bool IsWin { get; set; }

        public double OldRatingMean { get; set; }
        public double OldRatingDeviation { get; set; }
        public double NewRatingMean { get; set; }
        public double NewRatingDeviation { get; set; }
        public double RatingDelta { get; set; }
        public int DecayDays { get; set; }
        public int DecayValue { get; set; }
    }
}