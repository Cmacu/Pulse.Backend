using System.Linq;
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
      };
      _context.Schotten2Games.Add(game);
      _context.SaveChanges();
      return game.Id;
    }

    public void UpdateSchotten2Game(int gameId, GameState state) {
      var game = GetSchotten2Game(gameId);
      game.State = state;
      _context.SaveChangesAsync();
    }
  }
}