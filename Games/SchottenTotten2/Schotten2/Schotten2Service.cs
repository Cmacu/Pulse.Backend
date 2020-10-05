using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Cards;
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
      state.CurrentPlayerId = players[0].ToString();
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

      _storage.SaveLog(game.MatchId, game.State, GameEvent.Retreat, playerId, sectionIndex);
      // Perform action
      game.State = _engine.Retreat(game.State, sectionIndex);
      game.State.LastPlayer = Schotten2Role.Attacker;
      game.State.CurrentPlayerId = game.AttackerId;
      // Update state
      _storage.UpdateGame(game);
      return game;
    }

    public Schotten2Game PlayCard(string matchId, string playerId, int sectionIndex, int handIndex) {
      var game = _storage.LoadGame(matchId);

      if (game.State.IsAttackersTurn && game.AttackerId != playerId) throw new ForbiddenException("It's attacker's turn to play.");
      if (!game.State.IsAttackersTurn && game.DefenderId != playerId) throw new ForbiddenException("It's defenders's turn to play.");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException("Invalid Wall Section.");

      _storage.SaveLog(game.MatchId, game.State, GameEvent.PlayCard, playerId, sectionIndex, handIndex);
      game.State = _engine.PlayCard(game.State, sectionIndex, handIndex);
      game.State.LastPlayer = game.State.IsAttackersTurn ? Schotten2Role.Attacker : Schotten2Role.Deffender;
      game.State.CurrentPlayerId = null;

      _storage.UpdateGame(game);
      return game;
    }

    public List<Schotten2Response> GetLogs(string matchId, string playerId, int skip) {
      var game = _storage.LoadGame(matchId);
      var logs = _storage.GetLogs(matchId, skip);
      var response = new List<Schotten2Response>();
      foreach (var log in logs) {
        game.State = log.State;
        response.Add(MapResponse(game, playerId));
      }
      return response;
      throw new System.NotImplementedException();
    }

    public Schotten2Game CompleteTurn(string matchId, string playerId, int sectionIndex, int handIndex) {
      var game = _storage.LoadGame(matchId);
      var logState = game.State.DeepClone();

      if (game.State.IsAttackersTurn && game.AttackerId != playerId) throw new ForbiddenException("It's attacker's turn to play.");
      if (!game.State.IsAttackersTurn && game.DefenderId != playerId) throw new ForbiddenException("It's defenders's turn to play.");
      if (sectionIndex >= game.State.Sections.Count) throw new ForbiddenException("Invalid Wall Section.");
      if (handIndex < 0 || handIndex > 6) throw new ForbiddenException("Invalid Hand Card.");

      var isAttacker = game.AttackerId == playerId;

      if (_engine.HandleArchenemies(game.State, sectionIndex, isAttacker)) {
        return Complete(game, logState, handIndex);
      }

      var extraCards = new List<Card>(game.State.DefenderCards.Where(x => x != null));
      extraCards.AddRange(game.State.SiegeCards);
      for (var i = 0; i < game.State.Sections.Count; i++) {
        var section = game.State.Sections[i];
        if (section.CanDefend(extraCards)) continue;
        game.State.LastSection = i;
        game.State.LastEvent = GameEvent.Damage;
        game.State.DiscardCards.AddRange(section.Attack);
        game.State.DiscardCards.AddRange(section.Defense);

        if (section.IsDamaged) {
          section.Name = GameEvent.Destroy.ToString("F");
          game.State.LastEvent = GameEvent.Destroy;
          return Complete(game, logState, handIndex);
        }

        game.State.Sections[i] = _engine.GetSection(section.Style, true, section.MaxTries);
      }

      return Complete(game, logState, handIndex);;
    }

    private Schotten2Game Complete(Schotten2Game game, GameState state, int handIndex) {
      game.State = _engine.CompleteTurn(game.State, handIndex);
      game.State.CurrentPlayerId = game.State.IsAttackersTurn ? game.AttackerId : game.DefenderId;
      _storage.UpdateGame(game);
      if (game.State.LastEvent != GameEvent.PlayCard)
        _storage.SaveLog(game.MatchId, state, game.State.LastEvent, game.State.CurrentPlayerId, game.State.LastSection, handIndex);
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

      _storage.SaveLog(game.MatchId, game.State, GameEvent.UseOil, playerId, sectionIndex);
      game.State = _engine.UseOil(game.State, sectionIndex);
      game.State.LastPlayer = Schotten2Role.Deffender;
      game.State.CurrentPlayerId = game.DefenderId;

      _storage.UpdateGame(game);
      return game;
    }

    public Schotten2Game Exit(string matchId, string playerId, GameEvent exitType) {
      var game = _storage.LoadGame(matchId);

      if (playerId == game.AttackerId) game.WinnerId = game.DefenderId;
      else if (playerId == game.DefenderId) game.WinnerId = game.AttackerId;
      else throw new ForbiddenException("Only players in the game can complete.");

      _storage.SaveLog(game.MatchId, game.State, exitType, playerId);
      game.State.LastEvent = GameEvent.Resign;
      game.State.LastPlayer = playerId == game.AttackerId ? Schotten2Role.Attacker : Schotten2Role.Deffender;
      game.State.CurrentPlayerId = null;
      game.ResignedId = playerId;

      _storage.UpdateGame(game);
      return game;
    }

    public Schotten2Response MapResponse(Schotten2Game game, string playerId) {
      var isAttacker = playerId == game.AttackerId;
      var isDefender = playerId == game.DefenderId;
      var state = game.State;
      var isCurrentPlayer = playerId == game.State.CurrentPlayerId;

      var handCards = new List<Card>();
      if (isAttacker) handCards = state.AttackerCards;
      else if (isDefender) handCards = state.DefenderCards;

      var OpponentCardsCount = 0;
      if (isAttacker) OpponentCardsCount = state.DefenderCards.Count;
      else if (isDefender) OpponentCardsCount = state.AttackerCards.Count;

      var gameOver = "";
      if (!string.IsNullOrEmpty(game.WinnerId)) {
        if (isAttacker || isDefender) {
          gameOver = playerId == game.WinnerId ? "Win" : "Loss";
        } else {
          gameOver = "GameOver";
        }
      }

      var model = new Schotten2Response() {
        IsAttacker = isAttacker,
        IsCurrentPlayer = isCurrentPlayer,
        EnablePreparation = isCurrentPlayer && state.EnablePreparation,
        NewCards = state.NewCards,
        OilCount = state.OilCount,
        Sections = state.Sections,
        HandCards = handCards,
        OpponentCardsCount = OpponentCardsCount,
        SiegeCardsCount = state.SiegeCards.Count,
        DiscardCards = state.DiscardCards,
        LastPlayer = state.LastPlayer,
        LastEvent = state.LastEvent.ToString("F"),
        LastSection = state.LastSection,
        GameOver = gameOver,
      };
      return model;
    }

  }
}