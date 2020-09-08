using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Core.Entities;
using Pulse.Core.Services;
using Pulse.Matchmaker.Models;

namespace Pulse.Matchmaker.Services
{
    public interface IMatchmakerService
    {
        List<BatchModel> Run(
            Func<BatchModel, Task> onMatched,
            Func<BatchModel, Task> onPlaying,
            Func<BatchModel, Task> onError,
            int playersPerMatch = 2,
            int scoreLimit = 1000
        );
        Task AddPlayer(string player, DateTime? joinedAt = null);
        Task RemovePlayer(string player);
        List<SeekModel> ListPlayers();
    }

    public class MatchmakerService : IMatchmakerService
    {
        private readonly int _recentGameCount = 3;
        private readonly IMatchmakerPoolSingleton _matchmakerPoolSingleton;
        private readonly IMatchmakerLogService _matchmakerLogService;
        private readonly IPlayerService _playerService;
        private readonly IMatchService _matchService;
        private readonly INotificationService _notificationService;

        public MatchmakerService(
            IMatchmakerPoolSingleton matchmakerPoolSingleton,
            IMatchmakerLogService matchmakerLogService,
            IMatchService matchService,
            IPlayerService playerService,
            INotificationService notificationService
        )
        {
            _matchmakerPoolSingleton = matchmakerPoolSingleton;
            _matchmakerLogService = matchmakerLogService;
            _matchService = matchService;
            _playerService = playerService;
            _notificationService = notificationService;
        }

        public List<BatchModel> Run(
            Func<BatchModel, Task> onMatched,
            Func<BatchModel, Task> onPlaying,
            Func<BatchModel, Task> onError,
            int playersPerMatch = 2,
            int scoreLimit = 1000
        )
        {
            if (!this._matchmakerPoolSingleton.CanStart())
            {
                return null;
            }
            var matches = new List<BatchModel>();
            try
            {
                var matcher = new GroupAndSortMatcher(this._matchmakerPoolSingleton.ListPlayers());
                matcher.createPotentialMatches(playersPerMatch);
                matches = matcher.getMatches(scoreLimit);

                foreach (var BatchModel in matches)
                {
                    this._matchmakerPoolSingleton.MatchPlayers(BatchModel);
                    onMatched(BatchModel);
                    try
                    {
                        // TODO: implement _gameService.CreateGame
                        var gameId = 1;
                        var match = _matchService.CreateMatch(BatchModel.Randomize(), gameId);
                        onPlaying(BatchModel);
                        _matchmakerLogService.SetMatchId(BatchModel.ToList(), match.Id);

                        _notificationService.MatchCreated(match);
                    }
                    catch (Exception ex)
                    {
                        this._matchmakerPoolSingleton.UnmatchPlayers(BatchModel);
                        onError(BatchModel);
                        _notificationService.ExceptionCaught(ex);
                    }
                };
            }
            catch (Exception ex)
            {
                _notificationService.ExceptionCaught(ex);
            }
            this._matchmakerPoolSingleton.Stop();
            return matches;
        }

        public Task AddPlayer(string playerId, DateTime? joinedAt = null)
        {
            _matchmakerPoolSingleton.CanAddPlayer(playerId, () =>
            {
                var playerDetails = this.GetMatchmakerPlayerDetails(playerId);
                if (joinedAt != null)
                {
                    playerDetails.JoinedAt = joinedAt ?? DateTime.UtcNow;
                }
                _matchmakerLogService.Add(playerDetails);

                return playerDetails;
            });

            return Task.CompletedTask;
        }

        public Task RemovePlayer(string playerId)
        {
            _matchmakerPoolSingleton.RemovePlayer(playerId);
            var logEntry = _matchmakerLogService.Expire(int.Parse(playerId));

            return Task.CompletedTask;
        }

        // Use to get all players for running the matchmaker from external instance
        // For example: running th matchmaker via HTTP request instead of socket
        public List<SeekModel> ListPlayers()
        {
            return _matchmakerPoolSingleton.ListPlayers().ToList();
        }

        private SeekModel GetMatchmakerPlayerDetails(string player)
        {
            var playerId = int.Parse(player);
            var playerStatus = this._playerService.GetStatus(playerId);
            if (playerStatus == PlayerStatus.Playing.ToString("F"))
            {
                throw new System.InvalidOperationException("Another match currently in progress.");
            }
            if (playerStatus == PlayerStatus.Blocked.ToString("F"))
            {
                throw new System.InvalidOperationException("Blocked.");
            }
            var playerData = this._playerService.Get(playerId);
            var recentOpponents = this.GetRecentOpponents(playerId);
            return new SeekModel(player, playerData.ConservativeRating, recentOpponents);
        }

        private List<string> GetRecentOpponents(int playerId)
        {
            var recentMatches = this._matchService.GetRecent(playerId, null, 0, this._recentGameCount).Results;
            var opponentsList = new List<string>();
            foreach (var match in recentMatches)
            {
                foreach (var opponent in match.Opponents)
                {
                    if (opponent.Id != playerId)
                    {
                        opponentsList.Add(opponent.Id.ToString());
                    }
                }
            }
            return opponentsList;
        }
    }
}