using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Cards;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Schotten2 {
  public class Schotten2Response {
    public bool IsAttacker { get; set; }
    public bool IsCurrentPlayer { get; set; }
    public bool EnablePreparation { get; set; }
    public int NewCards { get; set; }
    public int OilCount { get; set; }
    public List<Section> Sections { get; set; }
    public List<Card> HandCards { get; set; }
    public int SiegeCardsCount { get; set; }
    public int OpponentCardsCount { get; set; }
    public List<Card> DiscardCards { get; set; }
    public string LastEvent { get; set; }
    public int LastSection { get; set; }
  }
}