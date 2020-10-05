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
      var deck = _cardService.CreateDeck(_config.SuitCount, _config.RankCount);
      var siegeCards = _cardService.Shuffle(deck);
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
        EnablePreparation = true,
        OilCount = _config.OilCount,
        NewCards = 0,
        Sections = CreateSections(),
        SiegeCards = siegeCards,
        AttackerCards = attackerCards,
        DefenderCards = defenderCards,
        DiscardCards = new List<Card>(),
        LastEvent = GameEvent.Start,
        LastSection = -1,
      };
      return state;
    }

    public GameState Retreat(GameState state, int sectionIndex) {
      var cards = state.Sections[sectionIndex].Attack;
      state.DiscardCards.AddRange(cards);
      state.Sections[sectionIndex].Attack = new List<Card>();
      state.NewCards = 0;
      state.EnablePreparation = true;
      state.LastEvent = GameEvent.Retreat;
      state.LastSection = sectionIndex;
      return state;
    }

    public GameState UseOil(GameState state, int sectionIndex) {
      var oilIndex = _config.OilIndex;
      var cards = state.Sections[sectionIndex].Attack;
      state.DiscardCards.Add(cards[oilIndex]);
      cards.RemoveAt(oilIndex);
      state.OilCount--;
      state.EnablePreparation = false;
      state.LastEvent = GameEvent.UseOil;
      state.LastSection = sectionIndex;

      return state;
    }

    public GameState PlayCard(GameState state, int sectionIndex, int handIndex) {
      var hand = state.IsAttackersTurn ? state.AttackerCards : state.DefenderCards;
      if (handIndex < 0 || handIndex >= hand.Count) throw new ForbiddenException("Invalid Hand Card.");

      var section = state.Sections[sectionIndex];
      var formation = state.IsAttackersTurn ? section.Attack : section.Defense;
      if (formation.Count >= section.Spaces) throw new ForbiddenException("Formation capacity reached.");

      var card = hand[handIndex];
      formation.Add(card);
      hand[handIndex] = null;
      state.LastSection = sectionIndex;
      state.LastEvent = GameEvent.PlayCard;

      return state;
    }

    public GameState CompleteTurn(GameState state, int handIndex) {
      if (
        state.LastEvent == GameEvent.Destroy ||
        state.LastEvent == GameEvent.Demolish ||
        state.LastEvent == GameEvent.Defend
      ) {
        return state;
      }

      if (GetDamagedCount(state.Sections) >= 4) {
        state.LastEvent = GameEvent.Demolish;
        return state;
      }

      if (state.IsAttackersTurn && state.SiegeCards.Count == 0) {
        state.LastEvent = GameEvent.Defend;
        return state;
      }

      if (state.SiegeCards.Count != 0) {
        var hand = state.IsAttackersTurn ? state.AttackerCards : state.DefenderCards;
        hand[handIndex] = _cardService.DrawCard(state.SiegeCards);
        state.NewCards = 1;
      }
      state.IsAttackersTurn = !state.IsAttackersTurn;
      state.EnablePreparation = state.IsAttackersTurn || state.OilCount > 0;
      // state.LastEvent = GameEvent.DrawCard;
      return state;
    }

    private List<Section> CreateSections() {
      var leftPit = GetSection(SectionStyle.LeftPit);
      var leftTower = GetSection(SectionStyle.LeftTower);
      var leftWall = GetSection(SectionStyle.LeftWall);
      var door = GetSection(SectionStyle.Gate);
      var rightWall = GetSection(SectionStyle.RightWall);
      var rightTower = GetSection(SectionStyle.RightTower);
      var rightPit = GetSection(SectionStyle.RightPit);
      return new List<Section>() { leftPit, leftTower, leftWall, door, rightWall, rightTower, rightPit };
    }

    public bool CheckControl(GameState state) {
      var extraCards = new List<Card>(state.DefenderCards);
      extraCards.AddRange(state.SiegeCards);
      extraCards = state.Sections[0].SortFormation(extraCards);
      for (var i = 0; i < state.Sections.Count; i++) {
        var section = state.Sections[i];
        if (section.CanDefend(extraCards)) continue;

        state.LastSection = i;
        state.LastEvent = GameEvent.Damage;

        if (section.IsDamaged) return true;

        state.DiscardCards.AddRange(section.Attack);
        state.DiscardCards.AddRange(section.Defense);
        state.Sections[i] = GetSection(section.Style, true);

        if (GetDamagedCount(state.Sections) >= 4) return true;
      }
      return false;
    }

    private int GetDamagedCount(List<Section> sections) {
      var count = 0;
      foreach (var section in sections) {
        if (section.IsDamaged) count++;
      }
      return count;
    }

    public bool HandleArchenemies(GameState state, int sectionIndex, bool isAttacker) {
      var section = state.Sections[sectionIndex];
      var formation = isAttacker ? section.Attack : section.Defense;
      if (formation.Count == 0) throw new ForbiddenException("Invalid section. Formation not found.");
      var card = formation[formation.Count - 1];
      var rank = card.Rank.ToString();
      if (!_config.Archenemies.ContainsKey(rank)) return false;

      var opposite = _config.Archenemies.GetValueOrDefault(rank);
      var archenemy = new Card() { Suit = card.Suit, Rank = opposite };
      var opponent = isAttacker ? section.Defense : section.Attack;

      if (!opponent.Remove(archenemy)) return false;

      state.DiscardCards.Add(archenemy);
      formation.Remove(card);
      state.DiscardCards.Add(card);

      state.LastSection = sectionIndex;
      state.LastEvent = GameEvent.Eliminate;
      return true;
    }

    public Section GetSection(SectionStyle style, bool isDamaged = false, int maxTries = 0) {
      var cardSpaces = 3;
      var formationTypes = new List<FormationType>() {
        FormationType.SUIT_RUN,
        FormationType.SAME_RANK,
        FormationType.SAME_SUIT,
        FormationType.RUN,
        FormationType.SUM
      };

      switch (style) {
        case SectionStyle.RightPit:
          cardSpaces = 3;
          formationTypes = isDamaged ?
            new List<FormationType>() { FormationType.RUN, FormationType.SUM } :
            new List<FormationType>() { FormationType.LOW_SUM };
          break;
        case SectionStyle.LeftPit:
          formationTypes = isDamaged ?
            new List<FormationType>() { FormationType.RUN, FormationType.SUM } :
            new List<FormationType>() { FormationType.SUM };
          break;
        case SectionStyle.LeftTower:
        case SectionStyle.RightTower:
          if (isDamaged) {
            cardSpaces = 2;
            formationTypes = new List<FormationType>() {
              FormationType.SAME_RANK,
              FormationType.SUM,
            };
          } else {
            cardSpaces = 4;
          }
          break;
        case SectionStyle.LeftWall:
        case SectionStyle.RightWall:
          if (isDamaged) {
            formationTypes = new List<FormationType>() {
              FormationType.SAME_SUIT,
              FormationType.SUM,
            };
          }
          break;
        case SectionStyle.Gate:
          if (isDamaged) {
            cardSpaces = 4;
            formationTypes = new List<FormationType>() { FormationType.LOW_SUM };
          } else {
            cardSpaces = 2;
          }
          break;
      }

      var section = new Section() {
        Style = style,
        Name = style.ToString("F"),
        IsDamaged = isDamaged,
        Spaces = cardSpaces,
        Types = formationTypes,
        MaxTries = maxTries,
      };

      return section;
    }
  }
}