using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Cards;

namespace Pulse.Games.SchottenTotten2.Wall {
  public class SectionModel {
    public string Name { get; set; }
    public int CardSpaces { get; set; }
    public bool IsDamaged { get; set; } = false;
    public bool IsLower { get; set; } = false;
    public List<FormationType> Formations { get; set; }
    public List<Card> playerFormation { get; set; }
    public List<Card> opponentFormation { get; set; }
    public bool canPlayCard { get; set; }
  }
}