using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Game;
using Pulse.Games.SchottenTotten2.Storage;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Schotten2 {

  public enum ExitType {
    Resign,
    Timeout,
  }

  public class Schotten2Service {
    private GameEngine _engine;
    private Schotten2Storage _storage;
    public Schotten2Service(GameEngine engine, Schotten2Storage storage) {
      _engine = engine;
      _storage = storage;
    }

    public void Start(string attackerId, string defenderId, string matchId) {
      if (string.IsNullOrEmpty(attackerId)) throw new ForbiddenException("AttackerId is required!");
      if (string.IsNullOrEmpty(defenderId)) throw new ForbiddenException("DefenderId is required!");
      var state = _engine.CreateGame();
      _storage.CreateGame(matchId, attackerId, defenderId, state);
      _storage.SaveLog(matchId, state, attackerId, "Create");
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
      if (!game.State.IsAttackersTurn) throw new ForbiddenException("It's defender's turn to play.");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException("InvalidInvalid Wall Section.");
      // Perform action
      var state = _engine.Retreat(game.State, sectionIndex);
      // Update state
      _storage.UpdateGame(game.MatchId, state);
      _storage.SaveLog(game.MatchId, state, playerId, "Retreat", sectionIndex);
      return MapState(state, playerId == game.AttackerId);
    }

    public Schotten2Response PlayCard(string playerId, int sectionIndex, int handIndex) {
      var game = _storage.LoadGame(playerId);

      if (game.State.IsAttackersTurn && game.AttackerId != playerId) throw new ForbiddenException("It's attacker's turn to play.");
      if (!game.State.IsAttackersTurn && game.DefenderId != playerId) throw new ForbiddenException("It's defenders's turn to play.");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException("Invalid Wall Section.");

      var state = _engine.PlayCard(game.State, sectionIndex, handIndex);

      if (state.AttackerCards.Count == 0) game.WinnerId = game.AttackerId;
      if (state.DefenderCards.Count == 0) game.WinnerId = game.DefenderId;

      _storage.UpdateGame(game.MatchId, state);
      _storage.SaveLog(game.MatchId, state, playerId, "PlayCard", sectionIndex, handIndex);
      return MapState(state, playerId == game.AttackerId);
    }

    public Schotten2Response UseOil(string playerId, int sectionIndex) {
      var game = _storage.LoadGame(playerId);
      if (playerId != game.AttackerId) throw new ForbiddenException("Only defender can use oil");
      if (game.State.OilCount == 0) throw new ForbiddenException("There are no more oil left to use");
      if (game.State.IsAttackersTurn) throw new ForbiddenException("It's attacker's turn to play.");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException("Invalid Wall Section.");
      if (game.State.Sections[sectionIndex].Attack.Count == 0) throw new ForbiddenException("No attacker cards found on this section.");

      var state = _engine.UseOil(game.State, sectionIndex);

      _storage.UpdateGame(game.MatchId, state);
      _storage.SaveLog(game.MatchId, state, playerId, "UseOil", sectionIndex);
      return MapState(state, playerId == game.AttackerId);
    }

    public Schotten2Response Exit(string playerId, ExitType exitType) {
      var game = _storage.LoadGame(playerId);

      if (playerId == game.AttackerId) game.WinnerId = game.DefenderId;
      else if (playerId == game.DefenderId) game.WinnerId = game.AttackerId;
      else throw new ForbiddenException("Only players in the game can resign.");
      var state = game.State;

      _storage.UpdateGame(game.MatchId, state);
      _storage.SaveLog(game.MatchId, state, playerId, exitType.ToString("F"));
      return MapState(game.State, playerId == game.AttackerId);
    }

    public Schotten2Response MapState(GameState state, bool isAttacker) {
      var model = new Schotten2Response() {
        isAttacker = isAttacker,
        isAttackerTurn = state.IsAttackersTurn,
        OilCount = state.OilCount,
        Sections = state.Sections,
        HandCards = isAttacker ? state.AttackerCards : state.DefenderCards,
        SiegeCardsCount = state.SiegeCards.Count,
        DiscardCards = state.DiscardCards,
      };
      return model;
    }

  }
}