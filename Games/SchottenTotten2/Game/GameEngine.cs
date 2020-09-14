using System.Collections.Generic;
using Pulse.Backend;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Cards;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Game {
  public class GameEngine {
    private readonly DataContext _context;
    private readonly CardService _cardService;
    private readonly GameConfig _config;
    public GameEngine(DataContext context) {
      _context = context;
      _cardService = new CardService();
      _config = new GameConfig();;
    }

    public GameState CreateGame() {

      // Create Deck
      var siegeCards = _cardService.CreateDeck(_config.SuitCount, _config.RankCount);
      // Draw Cards/Hands
      var attackerCards = new List<Card>();
      var defenderCards = new List<Card>();
      for (var i = 0; i < _config.HandSize; i++) {
        attackerCards.Add(_cardService.DrawCard(siegeCards));
        defenderCards.Add(_cardService.DrawCard(siegeCards));
      }
      // Setup State
      var state = new GameState() {
        IsAttackersTurn = true,
        OilCount = _config.OilCount,
        Sections = CreateSections(),
        SiegeCards = siegeCards,
        AttackerCards = attackerCards,
        DefenderCards = defenderCards,
        DiscardCards = new List<Card>(),
      };
      return state;
    }

    public GameState Retreat(GameState state, int sectionIndex) {
      var cards = state.Sections[sectionIndex].attackFormation;
      state.DiscardCards.AddRange(cards);
      state.Sections[sectionIndex].attackFormation = new List<Card>();
      return state;
    }

    private List<Section> CreateSections() {
      var leftPit = _config.GetSection("Pit");
      var leftTower = _config.GetSection("Tower");
      var leftWall = _config.GetSection("Wall");
      var door = _config.GetSection("Door");
      var rightWall = _config.GetSection("Wall");
      var rightTower = _config.GetSection("Tower");
      var rightPit = _config.GetSection("Pit");
      return new List<Section>() { leftPit, leftTower, leftWall, door, rightWall, rightTower, rightPit };
    }

    public GameState UseOil(GameState state, int sectionIndex) {
      var oilIndex = _config.OilIndex;
      var cards = state.Sections[sectionIndex].attackFormation;
      state.DiscardCards.Add(cards[oilIndex]);
      cards.RemoveAt(oilIndex);
      state.OilCount--;

      return state;
    }

    public GameState PlayCard(GameState state, int sectionIndex, int handIndex) {
      var section = state.Sections[sectionIndex];
      var formation = state.IsAttackersTurn ? section.attackFormation : section.defendFormation;
      if (formation.Count == section.CardSpaces - 1) throw new ForbiddenException("Formation capacity reached.");
      var hand = state.IsAttackersTurn ? state.AttackerCards : state.DefenderCards;
      if (handIndex < 0 || handIndex >= hand.Count) throw new ForbiddenException("Invalid Hand Card.");

      var card = hand[handIndex];
      formation.Add(card);
      hand.RemoveAt(handIndex);
      // TODO: Check for Control conditions
      if (state.SiegeCards.Count != 0) {
        hand.Add(_cardService.DrawCard(state.SiegeCards));
      }
      state.IsAttackersTurn = !state.IsAttackersTurn;

      return state;
    }
  }
}