using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.Matchmaker.Matcher {
  public class MatchmakerPoolSingleton {
    // Store private state indicating, if the process currently running
    private readonly object _runLock = new object();
    private Boolean _isRunning = false;
    private readonly object _poolLock = new object();
    // TODO: Migrate the pool list to permanent storage (database, Redis or similar)
    private Dictionary<string, SeekModel> _playerPool = new Dictionary<string, SeekModel>();

    public Boolean CanStart() {
      lock(_runLock) {
        if (_isRunning) {
          return false;
        }
        _isRunning = true;
        return true;
      }
    }

    public void Stop() {
      lock(_runLock) {
        _isRunning = false;
      }
    }

    public Boolean CanAddPlayer(string player, Func<SeekModel> getSeekDetails) {
      lock(_poolLock) {
        if (_playerPool.ContainsKey(player)) {
          return false;
        }
        _playerPool.Add(player, getSeekDetails());
      }
      return true;
    }

    public void RemovePlayer(string player) {
      lock(_poolLock) {
        _playerPool.Remove(player);
      }
    }

    // Use to get all players for running the matchmaker from external instance
    // For example: running th matchmaker via HTTP request instead of socket
    public IReadOnlyList<SeekModel> ListPlayers() {
      lock(_poolLock) {
        var playerPool = new List<SeekModel>(_playerPool.Values);
        return playerPool.Where(x => !x.IsMatched).OrderBy(x => x.Rating).ToList();
      }
    }

    public void MatchPlayers(IReadOnlyList<string> players) {
      foreach (var player in players) {
        UpdateIsMatched(player, true);
      }
    }

    public void UnmatchPlayers(IReadOnlyList<string> players) {
      foreach (var player in players) {
        UpdateIsMatched(player, false);
      }
    }

    public void UpdateIsMatched(string player, Boolean isMatched) {
      var playerDetails = _playerPool.GetValueOrDefault(player);
      if (playerDetails != null) playerDetails.IsMatched = isMatched;
    }

    public override string ToString() {
      if (_playerPool.Count() > 1)
        return $"The pool has {_playerPool.Count()} players.";
      else if (_playerPool.Count() == 1)
        return "The pool has exactly 1 player.";
      else
        return $"The pool is empty.";
    }
  }
}