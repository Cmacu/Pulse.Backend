using System;
using System.Collections.Generic;
using System.Text;
using Pulse.Core.Entities;

namespace Pulse.Rating.Models
{
    public class LeaderboardModel
    {
        public int PlayerId { get; set; }
        public string CgeUsername { get; set; }
        public string Avatar { get; set; }
        public string Country { get; set; }
        public double LeaderboardRating { get; set; }
        public int TotalDecay { get; set; }
        public int Rank { get; set; }
        public int? PreviousRank { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PagedLeaderboardModel
    {
        public int Total { get; set; }
        public List<LeaderboardModel> Results { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}