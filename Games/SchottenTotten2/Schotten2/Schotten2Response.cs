using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Cards;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Schotten2 {
  public class Schotten2Response {
    public bool isMyTurn { get; set; }
    public int OilCount { get; set; }
    public List<SectionModel> Sections { get; set; }
    public List<Card> MyCards { get; set; }
    public int SiegeCardsCount { get; set; }
    public List<Card> DiscardCards { get; set; }
  }
}