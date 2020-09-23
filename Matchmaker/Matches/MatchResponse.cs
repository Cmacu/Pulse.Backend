using System;
using System.Collections.Generic;

namespace Pulse.Matchmaker.Matches {
  public class MatchResponse {
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<OpponentResponse> Opponents { get; set; }
    public MatchStatus Status { get; set; }
  }

  public class PagedMatchResponse {
    public int Total { get; set; }
    public List<MatchResponse> Results { get; set; }
  }
}