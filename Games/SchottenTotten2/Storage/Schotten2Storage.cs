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
      var player = GetPlayer(playerId);
      var game = GetGame(player.MatchId);
      if (game == null) throw new NotFoundException($"Game not found");
      return game;
    }

    public void CreateGame(string matchId, string attackerId, string defenderId, GameState state) {
      _cache.Purge(DateTime.Now.AddHours(-2)); // TODO: Move purgeHours to config
      AddGame(matchId, attackerId, defenderId, state);
      AddPlayer(matchId, attackerId, Schotten2Role.Attacker);
      AddPlayer(matchId, defenderId, Schotten2Role.Deffender);
    }

    public Task UpdateGame(string matchId, GameState state) {
      var game = LoadGame(matchId);
      game.State = state;
      game.UpdatedAt = DateTime.UtcNow;
      _cache.SetGame(game);
      using(var scope = _scopeFactory.CreateScope()) {
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.Update(game);
        dataContext.SaveChanges();
      }
      return Task.CompletedTask;
    }

    public Task CompleteGame(string matchId, string winner) {
      var game = LoadGame(matchId);
      _cache.RemovePlayer(game.AttackerId);
      _cache.RemovePlayer(game.DefenderId);
      _cache.RemoveGame(matchId);
      return Task.CompletedTask;
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
      _cache.SetGame(game);

      return game;
    }

    private Task AddPlayer(
      string matchId,
      string playerId,
      Schotten2Role role
    ) {
      var player = new Schotten2Player() {
        MatchId = matchId,
        PlayerId = playerId,
        Role = role,
        CreatedAt = DateTime.UtcNow
      };
      _cache.AddPlayer(player);
      using(var scope = _scopeFactory.CreateScope()) {
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.Schotten2Players.Add(player);
        dataContext.SaveChanges();
      }
      return Task.CompletedTask;
    }

    private Schotten2Player GetPlayer(string playerId) {
      var player = _cache.GetPlayer(playerId);
      if (player != null) return player;

      player = _context.Schotten2Players
        .Where(x => x.PlayerId == playerId)
        .OrderByDescending(x => x.CreatedAt)
        .FirstOrDefault();

      if (player == null) throw new InternalException("Player has no matches!");
      _cache.AddPlayer(player);

      return player;
    }
  }
}