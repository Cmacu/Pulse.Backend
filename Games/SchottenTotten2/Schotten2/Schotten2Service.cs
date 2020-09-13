using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Cards;
using Pulse.Games.SchottenTotten2.Gameplay;
using Pulse.Games.SchottenTotten2.Persistance;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Schotten2 {
  public class Schotten2Service {
    private GameEngine _engine;
    private Schotten2GameService _persistance;
    public Schotten2Service(GameEngine gameplay, Schotten2GameService storage) {
      _engine = gameplay;
      _persistance = storage;
    }

    public Schotten2Response CreateGame(string player, string opponent) {
      var state = _engine.CreateGame();
      // var r = new Random();
      // return this.OrderBy(x => r.Next()).Select(int.Parse).ToList();
      var gameId = _persistance.CreateSchotten2Game(player, opponent, state);
      _persistance.SaveLog(gameId, state, player, "Create");

      return MapState(state, true);
    }

    public Schotten2Response GetGame(string player) {
      var game = _persistance.GetSchotten2Game(player);
      var state = game.State;
      return MapState(state, player == game.Attacker);
    }

    public Schotten2Response Retreat(string player, int sectionIndex) {
      // Retrieve Game
      var game = _persistance.GetSchotten2Game(player);
      // Validate input
      if (player != game.Attacker) throw new ForbiddenException("Defender can not retreat");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException($"Invalid Wall Section: {sectionIndex}");
      // Perform action
      game.State = _engine.Retreat(game.State, sectionIndex);
      // Update state
      _persistance.UpdateSchotten2Game(game);
      // Add Log
      _persistance.SaveLog(game.Id, game.State, player, "Retreat", sectionIndex);
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