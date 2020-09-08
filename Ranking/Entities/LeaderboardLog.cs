using System;
using Pulse.Core.Entities;

namespace Pulse.Rating.Entities
{
    public class LeaderboardLog
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public double ConservativeRating { get; set; }
        public int TotalDecay { get; set; }
        public int Rank { get; set; }
        public int? PreviousRank { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}