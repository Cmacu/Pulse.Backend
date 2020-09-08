using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Matchmaker.Models;

namespace Pulse.Matchmaker.Services
{
    public interface IMatchmakerPoolSingleton
    {
        Boolean CanStart();
        void Stop();
        Boolean CanAddPlayer(string player, Func<SeekModel> getSeekDetails);
        void RemovePlayer(string player);
        IReadOnlyList<SeekModel> ListPlayers();
        void MatchPlayers(IReadOnlyList<string> players);
        void UnmatchPlayers(IReadOnlyList<string> players);
    }

    public class MatchmakerPoolSingleton : IMatchmakerPoolSingleton
    {
        // Store private state indicating, if the process currently running
        private readonly object _runLock = new object();
        private Boolean _isRunning = false;
        private readonly object _poolLock = new object();
        // TODO: Migrate the pool list to permanent storage (database, Redis or similar)
        private Dictionary<string, SeekModel> _playerPool = new Dictionary<string, SeekModel>();

        public Boolean CanStart()
        {
            lock(this._runLock)
            {
                if (this._isRunning)
                {
                    return false;
                }
                this._isRunning = true;
                return true;
            }
        }

        public void Stop()
        {
            lock(this._runLock)
            {
                this._isRunning = false;
            }
        }

        public Boolean CanAddPlayer(string player, Func<SeekModel> getSeekDetails)
        {
            lock(this._poolLock)
            {
                if (this._playerPool.ContainsKey(player))
                {
                    return false;
                }
                this._playerPool.Add(player, getSeekDetails());
            }
            return true;
        }

        public void RemovePlayer(string player)
        {
            lock(this._poolLock)
            {
                this._playerPool.Remove(player);
            }
        }

        // Use to get all players for running the matchmaker from external instance
        // For example: running th matchmaker via HTTP request instead of socket
        public IReadOnlyList<SeekModel> ListPlayers()
        {
            lock(this._poolLock)
            {
                var playerPool = new List<SeekModel>(this._playerPool.Values);
                return playerPool.Where(x => !x.IsMatched).OrderBy(x => x.Rating).ToList();
            }
        }

        public void MatchPlayers(IReadOnlyList<string> players)
        {
            foreach (var player in players)
            {
                UpdateIsMatched(player, true);
            }
        }

        public void UnmatchPlayers(IReadOnlyList<string> players)
        {
            foreach (var player in players)
            {
                UpdateIsMatched(player, false);
            }
        }

        public void UpdateIsMatched(string player, Boolean isMatched)
        {
            var playerDetails = this._playerPool.GetValueOrDefault(player);
            if (playerDetails != null) playerDetails.IsMatched = isMatched;
        }

        public override string ToString()
        {
            if (_playerPool.Count() > 1)
                return $"The pool has {_playerPool.Count()} players.";
            else if (_playerPool.Count() == 1)
                return "The pool has exactly 1 player.";
            else
                return $"The pool is empty.";
        }
    }
}