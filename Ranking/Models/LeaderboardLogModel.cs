using System;
using System.Collections.Generic;
using System.Text;
using Pulse.Core.Entities;

namespace Pulse.Rank.Models
{
    public class LeaderboardLogModel
    {
        public string Username { get; set; }
        public double LeaderboardRating { get; set; }
        public int TotalDecay { get; set; }
        public int Rank { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}