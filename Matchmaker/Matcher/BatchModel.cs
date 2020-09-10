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

    public class PotentialMatchModel : IComparable<PotentialMatchModel> {
        public double Score { get; } = 0;
        public List<string> PlayerList { get; } = new List<string>();
        private double _avoidRecentMatchesMultiplier = 10;

        private double _reduceWaitMultiplier = 1;

        public PotentialMatchModel(List<SeekModel> players) {
            this.addPlayers(players);
            this.Score = 0;
            this.Score += this.getRatingDifference(players);
            this.Score += this.getRecentMatchesBetweenCount(players) * this._avoidRecentMatchesMultiplier;
            this.Score -= this.getTotalWaitTime(players) * this._reduceWaitMultiplier;
        }

        public int CompareTo(PotentialMatchModel other) {
            return this.Score.CompareTo(other.Score);
        }

        private void addPlayers(List<SeekModel> players) {
            foreach (var player in players) {
                this.PlayerList.Add(player.Player);
            }
        }

        private double getRatingDifference(List<SeekModel> players) {
            return Math.Abs(players[0].Rating - players[players.Count - 1].Rating);
        }

        private double getRecentMatchesBetweenCount(List<SeekModel> players) {
            var recentMatchesBetweenThePlayers = 0;
            foreach (var player in players) {
                foreach (var opponent in player.RecentOpponents) {
                    if (this.PlayerList.Contains(opponent)) {
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