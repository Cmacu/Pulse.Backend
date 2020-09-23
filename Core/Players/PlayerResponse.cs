using System.Collections.Generic;
using Pulse.Ranking.Rating;

namespace Pulse.Core.Players {
  public class PlayerResponse {
    public int Id { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
    public string Country { get; set; }
    public string Status { get; set; }
    public string MatchId { get; set; }
    public Division Division { get; set; }
    public int Level { get; set; }
    public int TotalWins { get; set; }
    public int TotalGames { get; set; }
    public int TotalTimeouts { get; set; }
    public int TotalResigns { get; set; }
    public int TotalCulture { get; set; }
    public int TotalDecay { get; set; }
    public int RegainDecay { get; set; }
    public double ConservativeRating { get; set; }
    public List<BadgeResponse> Badges { get; set; }
  }
}