using System;
using System.Collections.Generic;

namespace Pulse.Games.SchottenTotten2.Storage {
  public class Schotten2Singleton {
    private readonly object _playersLock = new object();
    private readonly Dictionary<string, string> _players = new Dictionary<string, string>();
    private readonly object _matchesLock = new object();
    private readonly Dictionary<string, Schotten2Game> _matches = new Dictionary<string, Schotten2Game>();

    public void SetPlayer(string playerId, string matchId) {
      lock(_playersLock) {
        _players[playerId] = matchId;
        // if (_players.ContainsKey(playerId)) {
        //   return false
        // }
        // _players.Add(playerId, matchId);
      }
    }

    public string GetFromPlayer(string playerId) {
      return _players.GetValueOrDefault(playerId);
    }

    public void RemovePlayer(string playerId) {
      lock(_playersLock) {
        _players.Remove(playerId);
      }
    }

    public void SetGame(Schotten2Game game) {
      var matchId = game.MatchId;
      lock(_matchesLock) {
        _matches[matchId] = game;
        // if (_matches.ContainsKey(matchId))
        // else
        //   _matches.Add(matchId, game);
      }
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
      foreach (var match in _matches) {
        if (match.Value.CreatedAt < before) {
          RemovePlayer(match.Value.AttackerId);
          RemovePlayer(match.Value.DefenderId);
          RemoveGame(match.Value.MatchId);
        }
      }
    }
  }
}