using System;
using System.Collections.Generic;

namespace Pulse.Matchmaker.Matcher {
  public class SeekModel : IComparable<SeekModel> {
    public string Player { get; }
    public double Rating { get; }
    public IReadOnlyList<string> RecentOpponents;
    public DateTime JoinedAt { get; set; }

    public Boolean IsMatched { get; set; }

    public SeekModel(
      string player,
      double rating,
      IReadOnlyList<string> recentOpponents,
      DateTime? joinedAt = null
    ) {
      Player = player;
      Rating = rating;
      RecentOpponents = recentOpponents;
      JoinedAt = joinedAt ?? DateTime.UtcNow;
      IsMatched = false;
    }

    public int CompareTo(SeekModel other) {
      if (Player == other.Player) {
        return 0;
      }
      if (Rating >= other.Rating) {
        return 1;
      }
      return -1;
    }
  }
}