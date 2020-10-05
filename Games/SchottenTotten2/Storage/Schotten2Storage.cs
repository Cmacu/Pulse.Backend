using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Backend;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Game;

namespace Pulse.Games.SchottenTotten2.Storage {

  public class Schotten2Storage {
    private DataContext _context;
    private IServiceScopeFactory _scopeFactory;
    private Schotten2Singleton _cache;
    public Schotten2Storage(IServiceScopeFactory scopeFactory, DataContext context, Schotten2Singleton cache) {
      _scopeFactory = scopeFactory;
      _context = context;
      _cache = cache;
    }

    public Schotten2Game LoadGame(string matchId) {
      return GetGame(matchId);
    }

    public void CreateGame(string matchId, string attackerId, string defenderId, GameState state) {
      _cache.Purge(DateTime.Now.AddHours(-1));
      AddGame(matchId, attackerId, defenderId, state);
    }

    public Task UpdateGame(Schotten2Game game) {
      game.UpdatedAt = DateTime.UtcNow;
      if (game.State.LastEvent == GameEvent.Destroy) game.WinnerId = game.AttackerId;
      if (game.State.LastEvent == GameEvent.Demolish) game.WinnerId = game.AttackerId;
      if (game.State.LastEvent == GameEvent.Defend) game.WinnerId = game.DefenderId;
      _cache.SetGame(game);
      using(var scope = _scopeFactory.CreateScope()) {
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.Update(game);
        dataContext.SaveChanges();
      }
      return Task.CompletedTask;
    }

    private Schotten2Game FindGame(string playerId) {
      return _context.Schotten2Games
        .Where(x => x.AttackerId == playerId || x.DefenderId == playerId)
        .OrderByDescending(x => x.CreatedAt)
        .FirstOrDefault();
    }

    private Task AddGame(string matchId, string attackerId, string defenderId, GameState state) {
      var game = new Schotten2Game() {
        MatchId = matchId,
        AttackerId = attackerId,
        DefenderId = defenderId,
        State = state,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
      };
      _cache.SetGame(game);
      using(var scope = _scopeFactory.CreateScope()) {
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.Schotten2Games.Add(game);
        dataContext.SaveChanges();
      }
      return Task.CompletedTask;
    }

    public Task SaveLog(
      string matchId,
      GameState state,
      GameEvent action,
      string playerId = null,
      int? sectionIndex = null,
      int? handIndex = null
    ) {
      var log = new Schotten2Log() {
      MatchId = matchId,
      State = state,
      Action = action.ToString("F"),
      PlayerId = playerId,
      SectionIndex = sectionIndex,
      HandIndex = handIndex,
      Timestamp = DateTime.UtcNow,
      };
      using(var scope = _scopeFactory.CreateScope()) {
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.Schotten2Logs.Add(log);
        dataContext.SaveChanges();
      }
      return Task.CompletedTask;
    }

    private Schotten2Game ReadGame(string matchId) {
      return _context.Schotten2Games.FirstOrDefault(x => x.MatchId == matchId);
    }

    public List<Schotten2Log> GetLogs(string matchId, int skip) {
      return _context.Schotten2Logs.Where(x => x.MatchId == matchId).OrderBy(x => x.Timestamp).Skip(skip).ToList();
    }

    private Schotten2Game GetGame(string matchId) {
      var game = _cache.GetGame(matchId);
      if (game != null) return game;
      game = ReadGame(matchId);
      if (game == null) throw new NotFoundException($"Game {matchId} not found!");
      if (string.IsNullOrEmpty(game.WinnerId)) {
        _cache.SetGame(game);
      }
      return game;
    }
  }
}