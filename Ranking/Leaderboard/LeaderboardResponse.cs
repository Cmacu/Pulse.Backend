using System;
using System.Collections.Generic;

namespace Pulse.Ranking.Leaderboard {
  public class LeaderboardResponse {
    public int PlayerId { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
    public string Country { get; set; }
    public double LeaderboardRating { get; set; }
    public int TotalDecay { get; set; }
    public int Rank { get; set; }
    public int? PreviousRank { get; set; }
    public DateTime CreatedAt { get; set; }
  }

  public class PagedLeaderboardResponse {
    public int Total { get; set; }
    public List<LeaderboardResponse> Results { get; set; }
    public DateTime? CreatedAt { get; set; }
  }
}