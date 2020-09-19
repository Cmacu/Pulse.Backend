using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.Matchmaker.Matcher {
  public class BatchModel : List<string> {
    public List<int> ToList() {
      return this.Select(int.Parse).ToList();
    }

    public List<int> Randomize() {
      var r = new Random();
      return this.OrderBy(x => r.Next()).Select(int.Parse).ToList();
    }

  }

  public class PotentialMatchResponse : IComparable<PotentialMatchResponse> {
    public double Score { get; } = 0;
    public List<string> PlayerList { get; } = new List<string>();
    private double _avoidRecentMatchesMultiplier = 10;

    private double _reduceWaitMultiplier = 1;

    public PotentialMatchResponse(List<SeekModel> players) {
      addPlayers(players);
      Score = 0;
      Score += getRatingDifference(players);
      Score += getRecentMatchesBetweenCount(players) * _avoidRecentMatchesMultiplier;
      Score -= getTotalWaitTime(players) * _reduceWaitMultiplier;
    }

    public int CompareTo(PotentialMatchResponse other) {
      return Score.CompareTo(other.Score);
    }

    private void addPlayers(List<SeekModel> players) {
      foreach (var player in players) {
        PlayerList.Add(player.Player);
      }
    }

    private double getRatingDifference(List<SeekModel> players) {
      return Math.Abs(players[0].Rating - players[players.Count - 1].Rating);
    }

    private double getRecentMatchesBetweenCount(List<SeekModel> players) {
      var recentMatchesBetweenThePlayers = 0;
      foreach (var player in players) {
        foreach (var opponent in player.RecentOpponents) {
          if (PlayerList.Contains(opponent)) {
            recentMatchesBetweenThePlayers++;
          }
        }
      }
      return recentMatchesBetweenThePlayers;
    }

    private double getTotalWaitTime(List<SeekModel> players) {
      var totalWaitTime = 0;
      var currentTime = new DateTime();
      foreach (var player in players) {
        var waitTime = currentTime - player.JoinedAt;
        totalWaitTime += waitTime.Seconds;
      }
      return totalWaitTime;
    }
  }
}