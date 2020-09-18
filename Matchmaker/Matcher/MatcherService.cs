using System.Collections.Generic;
using System.Linq;

namespace Pulse.Matchmaker.Matcher {
    public abstract class MatchmakerMatcher {
        public MatchmakerMatcher(IReadOnlyList<SeekModel> sortedPlayers) {

        }

    }

    public class GroupAndSortMatcher : MatchmakerMatcher {
        private IReadOnlyList<SeekModel> _sortedPlayers;
        private List<PotentialMatchResponse> _potentialMatches = new List<PotentialMatchResponse>();
        public GroupAndSortMatcher(IReadOnlyList<SeekModel> sortedPlayers) : base(sortedPlayers) {
            this._sortedPlayers = sortedPlayers;
        }

        public void createPotentialMatches(int playersPerMatch) {
            var potentialCount = this._sortedPlayers.Count - playersPerMatch;
            for (int i = 0; i <= potentialCount; i++) {
                var potentialPlayers = new List<SeekModel>();
                potentialPlayers.Add(this._sortedPlayers[i]);
                for (int j = 1; j < playersPerMatch; j++) {
                    potentialPlayers.Add(this._sortedPlayers[i + j]);
                }
                this._potentialMatches.Add(new PotentialMatchResponse(potentialPlayers));
            }
        }

        public List<BatchModel> getMatches(int scoreLimit) {
            var matches = new List<BatchModel>();
            var matchedPlayers = new List<string>();
            foreach (var potentialMatchResponse in _potentialMatches.OrderBy(x => x.Score)) {
                if (potentialMatchResponse.Score > scoreLimit) break;
                var players = potentialMatchResponse.PlayerList;
                if (players.Where(player => matchedPlayers.Contains(player)).Count() > 0) {
                    continue;
                }
                matchedPlayers.AddRange(players);
                var match = new BatchModel();
                match.AddRange(players);
                matches.Add(match);
            }

            return matches;
        }
    }
}