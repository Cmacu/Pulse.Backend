using System;
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

    public Schotten2Game LoadGame(string playerId) {
      var matchId = _cache.GetFromPlayer(playerId);
      Schotten2Game game;
      if (string.IsNullOrEmpty(matchId)) {
        game = FindGame(playerId);
        _cache.SetPlayer(game.AttackerId, game.MatchId);
        _cache.SetPlayer(game.DefenderId, game.MatchId);
      } else {
        game = GetGame(matchId);
      }
      return game;
    }

    public void CreateGame(string matchId, string attackerId, string defenderId, GameState state) {
      _cache.Purge(DateTime.Now.AddHours(-2));
      _cache.SetPlayer(defenderId, matchId);
      _cache.SetPlayer(attackerId, matchId);
      AddGame(matchId, attackerId, defenderId, state);
    }

    public Task UpdateGame(string matchId, GameState state) {
      var game = GetGame(matchId);
      game.State = state;
      game.UpdatedAt = DateTime.UtcNow;
      if (string.IsNullOrEmpty(game.WinnerId)) {
        _cache.SetGame(game);
      } else {
        CompleteGame(game.MatchId, game.AttackerId, game.DefenderId);
      }
      using(var scope = _scopeFactory.CreateScope()) {
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.Update(game);
        dataContext.SaveChanges();
      }
      return Task.CompletedTask;
    }

    public Task CompleteGame(string matchId, string attackerId, string defenderId) {
      _cache.RemovePlayer(attackerId);
      _cache.RemovePlayer(defenderId);
      _cache.RemoveGame(matchId);
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
      string playerId,
      string action,
      int? sectionIndex = null,
      int? handIndex = null
    ) {
      var log = new Schotten2Log() {
      MatchId = matchId,
      PlayerId = playerId,
      Action = "Create",
      HandIndex = handIndex,
      SectionIndex = sectionIndex,
      State = state,
      Timestamp = DateTime.UtcNow,
      };
      using(var scope = _scopeFactory.CreateScope()) {
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.Schotten2Logs.Add(log);
        dataContext.SaveChanges();
      }
      return Task.CompletedTask;
    }

    private Schotten2Game GetGame(string matchId) {
      var game = _cache.GetGame(matchId);
      if (game != null) return game;

      game = _context.Schotten2Games.FirstOrDefault(x => x.MatchId == matchId);

      if (game == null) throw new InternalException($"Game {matchId} not found!");
      if (!string.IsNullOrEmpty(game.WinnerId)) throw new NotFoundException("Game is over");

      _cache.SetGame(game);

      return game;
    }
  }
}