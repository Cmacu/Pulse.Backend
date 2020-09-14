using System;
using System.Collections.Generic;

namespace Pulse.Games.SchottenTotten2.Storage {
  public class Schotten2Singleton {
    private readonly object _playersLock = new object();
    private readonly Dictionary<string, Schotten2Player> _players = new Dictionary<string, Schotten2Player>();
    private readonly object _matchesLock = new object();
    private readonly Dictionary<string, Schotten2Game> _matches = new Dictionary<string, Schotten2Game>();

    public bool AddPlayer(Schotten2Player player) {
      var playerId = player.PlayerId;
      lock(_playersLock) {
        if (_players.ContainsKey(playerId)) {
          return false;
        }
        _players.Add(playerId, player);
      }
      return true;
    }

    public Schotten2Player GetPlayer(string playerId) {
      return _players.GetValueOrDefault(playerId);
    }

    public void RemovePlayer(string playerId) {
      lock(_playersLock) {
        _players.Remove(playerId);
      }
    }

    public bool SetGame(Schotten2Game game) {
      var matchId = game.MatchId;
      lock(_matchesLock) {
        if (_matches.ContainsKey(matchId))
          _matches[matchId] = game;
        else
          _matches.Add(matchId, game);
      }
      return true;
    }

    public Schotten2Game GetGame(string matchId) {
      return _matches.GetValueOrDefault(matchId);
    }

    public void RemoveGame(string matchId) {
      lock(_matchesLock) {
        _matches.Remove(matchId);
      }
    }

    public void Purge(DateTime before) {
      foreach (var player in _players) {
        if (player.Value.CreatedAt < before) {
          RemovePlayer(player.Value.PlayerId);
        }
      }
      foreach (var match in _matches) {
        if (match.Value.CreatedAt < before) {
          RemoveGame(match.Value.MatchId);
        }
      }
    }
  }
}