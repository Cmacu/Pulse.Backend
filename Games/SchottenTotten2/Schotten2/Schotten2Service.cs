using System.Collections.Generic;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Game;
using Pulse.Games.SchottenTotten2.Storage;

namespace Pulse.Games.SchottenTotten2.Schotten2 {

  public class Schotten2Service {
    private GameEngine _engine;
    private Schotten2Storage _storage;
    public Schotten2Service(GameEngine engine, Schotten2Storage storage) {
      _engine = engine;
      _storage = storage;
    }

    public void Start(List<string> players, string matchId) {
      if (players.Count != 2) throw new ForbiddenException("Two players required to start a game!");
      var state = _engine.CreateGame();
      _storage.CreateGame(matchId, players[0].ToString(), players[1].ToString(), state);
      _storage.SaveLog(matchId, state, GameEvent.Start);
    }

    public Schotten2Game Load(string matchId) {
      var game = _storage.LoadGame(matchId);
      return game;
    }

    public Schotten2Game Retreat(string matchId, string playerId, int sectionIndex) {
      // Retrieve Game
      var game = _storage.LoadGame(matchId);
      // Validate input
      if (playerId != game.AttackerId) throw new ForbiddenException("Only attacker is allowed to retreat");
      if (!game.State.IsAttackersTurn) throw new ForbiddenException("It's defender's turn to play.");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException("Invalid Wall Section.");
      if (!game.State.EnablePreparation) throw new ForbiddenException("Retreat is only allowed during preparation.");
      // Perform action
      var state = _engine.Retreat(game.State, sectionIndex);
      // Update state
      _storage.UpdateGame(game.MatchId, state);
      _storage.SaveLog(game.MatchId, state, GameEvent.Retreat, playerId, sectionIndex);
      return game;
    }

    public Schotten2Game PlayCard(string matchId, string playerId, int sectionIndex, int handIndex) {
      var game = _storage.LoadGame(matchId);

      if (game.State.IsAttackersTurn && game.AttackerId != playerId) throw new ForbiddenException("It's attacker's turn to play.");
      if (!game.State.IsAttackersTurn && game.DefenderId != playerId) throw new ForbiddenException("It's defenders's turn to play.");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException("Invalid Wall Section.");

      var state = _engine.PlayCard(game.State, sectionIndex, handIndex);

      if (state.AttackerCards.Count == 0) game.WinnerId = game.DefenderId;
      if (state.DefenderCards.Count == 0) game.WinnerId = game.AttackerId;

      _storage.UpdateGame(game.MatchId, state);
      _storage.SaveLog(game.MatchId, state, GameEvent.PlayCard, playerId, sectionIndex, handIndex);
      return game;
    }

    public Schotten2Game UseOil(string matchId, string playerId, int sectionIndex) {
      var game = _storage.LoadGame(matchId);
      if (playerId != game.DefenderId) throw new ForbiddenException("Only defender can use oil");
      if (game.State.OilCount == 0) throw new ForbiddenException("There are no more oil left to use");
      if (game.State.IsAttackersTurn) throw new ForbiddenException("It's attacker's turn to play.");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException("Invalid Wall Section.");
      if (game.State.Sections[sectionIndex].Attack.Count == 0) throw new ForbiddenException("No attacker cards found on this section.");
      if (!game.State.EnablePreparation) throw new ForbiddenException("Using oil is only allowed during preparation.");

      var state = _engine.UseOil(game.State, sectionIndex);

      _storage.UpdateGame(game.MatchId, state);
      _storage.SaveLog(game.MatchId, state, GameEvent.UseOil, playerId, sectionIndex);
      return game;
    }

    public Schotten2Game Exit(string matchId, string playerId, GameEvent exitType) {
      var game = _storage.LoadGame(matchId);

      if (playerId == game.AttackerId) game.WinnerId = game.DefenderId;
      else if (playerId == game.DefenderId) game.WinnerId = game.AttackerId;
      else throw new ForbiddenException("Only players in the game can resign.");
      var state = game.State;

      _storage.UpdateGame(game.MatchId, state);
      _storage.SaveLog(game.MatchId, state, exitType, playerId);
      return game;
    }

    public Schotten2Response MapResponse(Schotten2Game game, string playerId, int sectionIndex = -1) {
      var isAttacker = playerId == game.AttackerId;
      var state = game.State;
      var isCurrentPlayer = (isAttacker && state.IsAttackersTurn) || (!isAttacker && !state.IsAttackersTurn);
      var model = new Schotten2Response() {
        IsAttacker = isAttacker,
        IsCurrentPlayer = isCurrentPlayer,
        EnablePreparation = isCurrentPlayer && state.EnablePreparation,
        ActiveSectionIndex = sectionIndex,
        NewCards = state.NewCards,
        OilCount = state.OilCount,
        Sections = state.Sections,
        HandCards = isAttacker ? state.AttackerCards : state.DefenderCards,
        OpponentCardsCount = (isAttacker ? state.DefenderCards : state.AttackerCards).Count,
        SiegeCardsCount = state.SiegeCards.Count,
        DiscardCards = state.DiscardCards,
        LastEvent = state.LastEvent.ToString("F"),
      };
      return model;
    }

  }
}