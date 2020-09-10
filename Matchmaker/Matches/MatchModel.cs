using System;
using System.Collections.Generic;

namespace Pulse.Matchmaker.Matches {
    public class MatchModel {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<OpponentModel> Opponents { get; set; }
        public MatchStatus Status { get; set; }
    }

    public class PagedMatchModel {
        public int Total { get; set; }
        public List<MatchModel> Results { get; set; }
    }
}