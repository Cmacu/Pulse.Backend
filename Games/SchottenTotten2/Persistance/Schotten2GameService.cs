using System;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Backend;
using Pulse.Games.SchottenTotten2.Gameplay;

namespace Pulse.Games.SchottenTotten2.Persistance {
  public class Schotten2GameService {

    private DataContext _context;
    public Schotten2GameService(DataContext context) {
      _context = context;
    }

    public Schotten2Game GetSchotten2Game(int gameId) {
      var game = _context.Schotten2Games.FirstOrDefault(x => x.Id == gameId);
      // TODO: Handle not found exception
      return game;
    }

    public int CreateSchotten2Game(
      string attacker,
      string defender,
      GameState state
    ) {
      var game = new Schotten2Game() {
        Attacker = attacker,
        Defender = defender,
        State = state,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
      };
      _context.Schotten2Games.Add(game);
      _context.SaveChanges();
      return game.Id;
    }

    public void UpdateSchotten2Game(int gameId, GameState state) {
      var game = GetSchotten2Game(gameId);
      game.State = state;
      game.UpdatedAt = DateTime.UtcNow;
      _context.SaveChangesAsync();
    }

    public Task SaveLog(int gameId, string player, string action, GameState state, int? sectionIndex = null, int? handIndex = null) {
      var log = new Schotten2Log() {
      GameId = gameId,
      Player = player,
      Action = "Create",
      HandIndex = handIndex,
      SectionIndex = sectionIndex,
      State = state,
      Timestamp = DateTime.UtcNow,
      };
      _context.Schotten2Logs.Add(log);
      _context.SaveChangesAsync();
      return Task.CompletedTask;
    }
  }
}