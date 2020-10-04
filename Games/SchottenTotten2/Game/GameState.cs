using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Cards;
using Pulse.Games.SchottenTotten2.Storage;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Game {
  public class GameState {
    public bool IsAttackersTurn { get; set; }
    public string CurrentPlayerId { get; set; }
    public bool EnablePreparation { get; set; }
    public int NewCards { get; set; }
    public int OilCount { get; set; }
    public List<Section> Sections { get; set; }
    public List<Card> AttackerCards { get; set; }
    public List<Card> DefenderCards { get; set; }
    public List<Card> SiegeCards { get; set; }
    public List<Card> DiscardCards { get; set; }
    public Schotten2Role LastPlayer { get; set; }
    public GameEvent LastEvent { get; set; }
    public int LastSection { get; set; }

    public List<Card> CloneCards(List<Card> original) {
      var cards = new List<Card>();
      foreach (var card in original) {
        var cloned = card == null ? null : new Card() {
          Suit = card.Suit,
          Rank = card.Rank,
        };
        cards.Add(cloned);
      }
      return cards;
    }

    public List<Section> CloneSections(List<Section> original) {
      var sections = new List<Section>();
      foreach (var section in original) {
        var types = new List<FormationType>();
        types.AddRange(section.Types);
        var clone = new Section() {
          Name = section.Name,
          Spaces = section.Spaces,
          Style = section.Style,
          IsDamaged = section.IsDamaged,
          Attack = CloneCards(section.Attack),
          Defense = CloneCards(section.Defense),
          Types = types,
          MaxTries = section.MaxTries,
        };
        sections.Add(clone);
      }
      return sections;
    }

    public GameState DeepClone() {
      var attackerCards = new List<Card>();
      var state = new GameState() {
        IsAttackersTurn = this.IsAttackersTurn,
        CurrentPlayerId = this.CurrentPlayerId,
        EnablePreparation = this.EnablePreparation,
        NewCards = this.NewCards,
        OilCount = this.OilCount,
        Sections = CloneSections(this.Sections),
        AttackerCards = CloneCards(this.AttackerCards),
        DefenderCards = CloneCards(this.DefenderCards),
        SiegeCards = CloneCards(this.SiegeCards),
        DiscardCards = CloneCards(this.DiscardCards),
        LastPlayer = this.LastPlayer,
        LastEvent = this.LastEvent,
        LastSection = this.LastSection,
      };
      return state;
    }
  }

}