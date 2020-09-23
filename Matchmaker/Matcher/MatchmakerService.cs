using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Core.Notifications;
using Pulse.Core.Players;
using Pulse.Games.SchottenTotten2.Schotten2;
using Pulse.Matchmaker.Logs;
using Pulse.Matchmaker.Matches;

namespace Pulse.Matchmaker.Matcher {
  public class MatchmakerService {
    private readonly int _recentGameCount = 3;
    private readonly MatchmakerPoolSingleton _matchmakerPoolSingleton;
    private readonly MatchmakerLogService _matchmakerLogService;
    private readonly PlayerService _playerService;
    private readonly MatchService _matchService;
    private readonly Schotten2Service _schotten2Service;
    private readonly NotificationService _notificationService;

    public MatchmakerService(
      MatchmakerPoolSingleton matchmakerPoolSingleton,
      MatchmakerLogService matchmakerLogService,
      MatchService matchService,
      PlayerService playerService,
      Schotten2Service schotten2Service,
      NotificationService notificationService
    ) {
      _matchmakerPoolSingleton = matchmakerPoolSingleton;
      _matchmakerLogService = matchmakerLogService;
      _matchService = matchService;
      _playerService = playerService;
      _schotten2Service = schotten2Service;
      _notificationService = notificationService;
    }

    public List<MatchedPlayers> Run(
      Func<MatchedPlayers, Task> onMatched,
      Func<MatchedPlayers, string, Task> onPlaying,
      Func<MatchedPlayers, Task> onError,
      int playersPerMatch = 2,
      int scoreLimit = 300
    ) {
      if (!_matchmakerPoolSingleton.CanStart()) {
        return null;
      }
      var matches = new List<MatchedPlayers>();
      try {
        var matcher = new GroupAndSortMatcher(_matchmakerPoolSingleton.ListPlayers());
        matcher.createPotentialMatches(playersPerMatch);
        matches = matcher.getMatches(scoreLimit);

        foreach (var matchedPlayers in matches) {
          _matchmakerPoolSingleton.MatchPlayers(matchedPlayers);
          onMatched(matchedPlayers);
          try {
            var players = matchedPlayers.Randomize();
            var playerIds = players.Select(int.Parse).ToList();
            var matchId = _matchService.CreateMatch(playerIds);
            _schotten2Service.Start(players, matchId.ToString());
            onPlaying(matchedPlayers, matchId.ToString());
            _matchmakerLogService.SetMatchId(playerIds, matchId);

            // TODO: Enable _notificationService.MatchCreated(matchId);
          } catch (Exception ex) {
            _matchmakerPoolSingleton.UnmatchPlayers(matchedPlayers);
            onError(matchedPlayers);
            _notificationService.ExceptionCaught(ex);
          }
        };
      } catch (Exception ex) {
        _notificationService.ExceptionCaught(ex);
      }
      _matchmakerPoolSingleton.Stop();
      return matches;
    }

    public Task AddPlayer(string playerId, DateTime? joinedAt = null) {
      _matchmakerPoolSingleton.CanAddPlayer(playerId, () => {
        var playerDetails = GetSeekDetails(playerId);
        if (joinedAt != null) {
          playerDetails.JoinedAt = joinedAt ?? DateTime.UtcNow;
        }
        _matchmakerLogService.Add(playerDetails);

        return playerDetails;
      });

      return Task.CompletedTask;
    }

    public Task RemovePlayer(string playerId) {
      _matchmakerPoolSingleton.RemovePlayer(playerId);
      var logEntry = _matchmakerLogService.Expire(int.Parse(playerId));

      return Task.CompletedTask;
    }

    // Use to get all players for running the matchmaker from external instance
    // For example: running th matchmaker via HTTP request instead of socket
    public List<SeekModel> ListPlayers() {
      return _matchmakerPoolSingleton.ListPlayers().ToList();
    }

    private SeekModel GetSeekDetails(string player) {
      var playerId = int.Parse(player);
      var playerStatus = _playerService.GetStatus(playerId);
      if (playerStatus == PlayerStatus.Playing.ToString("F")) {
        throw new System.InvalidOperationException("Another match currently in progress.");
      }
      if (playerStatus == PlayerStatus.Blocked.ToString("F")) {
        throw new System.InvalidOperationException("Blocked.");
      }
      var playerData = _playerService.Get(playerId);
      var recentOpponents = GetRecentOpponents(playerId);
      return new SeekModel(player, playerData.ConservativeRating, recentOpponents);
    }

    private List<string> GetRecentOpponents(int playerId) {
      var recentMatches = _matchService.GetRecent(playerId, null, 0, _recentGameCount).Results;
      var opponentsList = new List<string>();
      foreach (var match in recentMatches) {
        foreach (var opponent in match.Opponents) {
          if (opponent.Id != playerId) {
            opponentsList.Add(opponent.Id.ToString());
          }
        }
      }
      return opponentsList;
    }
  }
}