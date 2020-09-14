using System.Collections.Generic;
using Pulse.Backend;
using Pulse.Games.SchottenTotten2.Cards;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Game {
  public class GameEngine {
    private readonly DataContext _context;
    private readonly CardService _cardService;
    public GameEngine(DataContext context) {
      _context = context;
      _cardService = new CardService();
    }

    public GameState CreateGame() {
      var gameConfig = new GameConfig();
      // Create Deck
      var siegeCards = _cardService.CreateDeck(gameConfig.SuitCount, gameConfig.RankCount);
      // Draw Cards/Hands
      var attackerCards = new List<Card>();
      var defenderCards = new List<Card>();
      for (var i = 0; i < gameConfig.HandSize; i++) {
        attackerCards.Add(_cardService.DrawCard(siegeCards));
        defenderCards.Add(_cardService.DrawCard(siegeCards));
      }
      // Setup State
      var state = new GameState() {
        IsAttackersTurn = true,
        OilCount = gameConfig.OilCount,
        Sections = CreateSections(gameConfig),
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

    private List<Section> CreateSections(GameConfig gameConfig) {
      var leftPit = gameConfig.GetSection("Pit");
      var leftTower = gameConfig.GetSection("Tower");
      var leftWall = gameConfig.GetSection("Wall");
      var door = gameConfig.GetSection("Door");
      var rightWall = gameConfig.GetSection("Wall");
      var rightTower = gameConfig.GetSection("Tower");
      var rightPit = gameConfig.GetSection("Pit");
      return new List<Section>() { leftPit, leftTower, leftWall, door, rightWall, rightTower, rightPit };
    }
  }
}