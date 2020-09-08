using System;

namespace Pulse.Matchmaker.Entities
{
    public class MatchmakerLog
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public DateTime AddedAt { get; set; }
        public Double Rating { get; set; }
        public String RecentOpponents { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public int? MatchId { get; set; }
    }
}