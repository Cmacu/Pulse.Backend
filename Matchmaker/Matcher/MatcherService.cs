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
      _sortedPlayers = sortedPlayers;
    }

    public void createPotentialMatches(int playersPerMatch) {
      var potentialCount = _sortedPlayers.Count - playersPerMatch;
      for (int i = 0; i <= potentialCount; i++) {
        var potentialPlayers = new List<SeekModel>();
        potentialPlayers.Add(_sortedPlayers[i]);
        for (int j = 1; j < playersPerMatch; j++) {
          potentialPlayers.Add(_sortedPlayers[i + j]);
        }
        _potentialMatches.Add(new PotentialMatchResponse(potentialPlayers));
      }
    }

    public List<MatchedPlayers> getMatches(int scoreLimit) {
      var matches = new List<MatchedPlayers>();
      var matchedPlayers = new List<string>();
      foreach (var potentialMatchResponse in _potentialMatches.OrderBy(x => x.Score)) {
        if (potentialMatchResponse.Score > scoreLimit) break;
        var players = potentialMatchResponse.PlayerList;
        if (players.Where(player => matchedPlayers.Contains(player)).Count() > 0) {
          continue;
        }
        matchedPlayers.AddRange(players);
        var match = new MatchedPlayers();
        match.AddRange(players);
        matches.Add(match);
      }

      return matches;
    }
  }
}