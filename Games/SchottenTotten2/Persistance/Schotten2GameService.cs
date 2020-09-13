using System;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Backend;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Gameplay;

namespace Pulse.Games.SchottenTotten2.Persistance {
  public class Schotten2GameService {

    private DataContext _context;
    public Schotten2GameService(DataContext context) {
      _context = context;
    }

    public Schotten2Game GetSchotten2Game(string player) {
      var game = _context.Schotten2Games
        .Where(x => x.Attacker == player || x.Defender == player)
        .OrderByDescending(x => x.UpdatedAt)
        .FirstOrDefault();
      if (game == null) throw new NotFoundException($"Game not found");
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

    public void UpdateSchotten2Game(Schotten2Game game) {
      game.UpdatedAt = DateTime.UtcNow;
      _context.SaveChangesAsync();
    }

    public Task SaveLog(int gameId, GameState state, string player, string action, int? sectionIndex = null, int? handIndex = null) {
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