using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Cards;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Gameplay {
  public class GameState {
    public bool IsAttackersTurn { get; set; }
    public int OilCount { get; set; }
    public List<Section> Sections { get; set; }
    public List<Card> AttackerCards { get; set; }
    public List<Card> DefenderCards { get; set; }
    public List<Card> SiegeCards { get; set; }
    public List<Card> DiscardCards { get; set; }
  }
}