using System;
using System.Collections.Generic;

namespace Pulse.Games.SchottenTotten2.Storage {
  public class Schotten2Singleton {
    private readonly object _gamesLock = new object();
    private readonly Dictionary<string, Schotten2Game> _games = new Dictionary<string, Schotten2Game>();

    public void SetGame(Schotten2Game game) {
      var matchId = game.MatchId;
      lock(_gamesLock) {
        _games[matchId] = game;
      }
    }
    public Schotten2Game GetGame(string matchId) {
      return _games.GetValueOrDefault(matchId);
    }
    public void RemoveGame(string matchId) {
      lock(_gamesLock) {
        _games.Remove(matchId);
      }
    }
    public void Purge(DateTime before) {
      foreach (var match in _games) {
        if (match.Value.CreatedAt < before) {
          RemoveGame(match.Value.MatchId);
        }
      }
    }
  }
}