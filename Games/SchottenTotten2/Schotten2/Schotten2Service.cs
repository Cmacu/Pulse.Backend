using System;
using System.Linq;
using System.Security.Claims;
using Pulse.Core.AppErrors;
using Pulse.Core.Authorization;
using Pulse.Games.SchottenTotten2.Gameplay;
using Pulse.Games.SchottenTotten2.Persistance;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Schotten2 {
  public class Schotten2Service {
    private GameEngine _engine;
    private Schotten2GameService _storage;
    public Schotten2Service(GameEngine gameplay, Schotten2GameService storage) {
      _engine = gameplay;
      _storage = storage;
    }

    public Schotten2Response CreateGame(string player, string opponent) {
      if (string.IsNullOrEmpty(player)) throw new AuthException("Active session required to play Schotten 2");
      var state = _engine.CreateGame();
      // var r = new Random();
      // return this.OrderBy(x => r.Next()).Select(int.Parse).ToList();
      _storage.CreateSchotten2Game(player, opponent, state);

      return MapState(state, true);
    }

    public Schotten2Response GetGame(string player, int gameId) {
      var game = _storage.GetSchotten2Game(gameId);
      if (game == null) {
        throw new NotFoundException($"Game {gameId} not found");
      }
      var state = game.State;
      return MapState(state, player == game.Attacker);
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
        isMyTurn = (state.IsAttackersTurn && isAttacker) || (!state.IsAttackersTurn && !isAttacker),
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