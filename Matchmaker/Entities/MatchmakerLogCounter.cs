using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pulse.Matchmaker.Entities
{
    public class MatchmakerLogCounter
    {
        public int Id { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int PlayerCount { get; set; }
        public int MatchCount { get; set; }
        public int LogCount { get; set; }
        public double WaitSeconds { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}