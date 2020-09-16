using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Cards;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Schotten2 {
  public class Schotten2Response {
    public bool isAttacker { get; set; }
    public bool isAttackerTurn { get; set; }
    public int OilCount { get; set; }
    public List<Section> Sections { get; set; }
    public List<Card> HandCards { get; set; }
    public int SiegeCardsCount { get; set; }
    public List<Card> DiscardCards { get; set; }
  }
}