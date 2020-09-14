using System.Linq;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Game;
using Pulse.Games.SchottenTotten2.Storage;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Schotten2 {
  public class Schotten2Service {
    private GameEngine _engine;
    private Schotten2Storage _storage;
    public Schotten2Service(GameEngine engine, Schotten2Storage storage) {
      _engine = engine;
      _storage = storage;
    }

    public Schotten2Response Start(string playerId, string opponentId, string matchId) {
      var state = _engine.CreateGame();
      // var r = new Random();
      // return this.OrderBy(x => r.Next()).Select(int.Parse).ToList();
      _storage.CreateGame(matchId, playerId, opponentId, state);
      _storage.SaveLog(matchId, state, playerId, "Create");

      return MapState(state, true);
    }

    public Schotten2Response Load(string playerId) {
      var game = _storage.LoadGame(playerId);
      return MapState(game.State, playerId == game.AttackerId);
    }

    public Schotten2Response Retreat(string playerId, int sectionIndex) {
      // Retrieve Game
      var game = _storage.LoadGame(playerId);
      // Validate input
      if (playerId != game.AttackerId) throw new ForbiddenException("Only attacker is allowed to retreat");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException($"Invalid Wall Section: {sectionIndex}");
      // Perform action
      game.State = _engine.Retreat(game.State, sectionIndex);
      // Update state
      _storage.UpdateGame(game.MatchId, game.State);
      // Add Log
      _storage.SaveLog(game.MatchId, game.State, playerId, "Retreat", sectionIndex);
      return MapState(game.State, true);
    }

    public Schotten2Response MapState(GameState state, bool isAttacker) {
      var sections = state.Sections.Select(x => {
        var playerFormation = isAttacker ? x.attackFormation : x.defendFormation;
        var section = new SectionModel() {
          Name = x.Name,
          CardSpaces = x.CardSpaces,
          IsDamaged = x.IsDamaged,
          IsLower = x.IsLower,
          Formations = x.Formations,
          playerFormation = playerFormation,
          opponentFormation = isAttacker ? x.defendFormation : x.attackFormation,
          canPlayCard = playerFormation.Count < x.CardSpaces,
        };
        return section;
      });
      var model = new Schotten2Response() {
        isAttacker = isAttacker,
        isAttackerTurn = state.IsAttackersTurn,
        OilCount = state.OilCount,
        Sections = sections.ToList(),
        MyCards = isAttacker ? state.AttackerCards : state.DefenderCards,
        SiegeCardsCount = state.SiegeCards.Count,
        DiscardCards = state.DiscardCards,
      };
      return model;
    }

  }
}