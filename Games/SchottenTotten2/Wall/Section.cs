using System.Collections.Generic;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Cards;

namespace Pulse.Games.SchottenTotten2.Wall {
  public class Section {
    private string _name;
    public string Name {
      get {
        if (string.IsNullOrEmpty(_name)) throw new InternalException("Section.Name not set yet!");
        return _name;
      }
      set {
        if (!string.IsNullOrEmpty(_name)) throw new InternalException("Section.Name can not be changed!");
        _name = value;
      }
    }
    public int CardSpaces { get; set; }
    public bool IsDamaged { get; set; } = false;
    public bool IsLower { get; set; } = false;
    public List<FormationType> Formations { get; set; }
    public List<Card> attackFormation { get; set; }
    public List<Card> defendFormation { get; set; }
    public void Setup(int cardSpaces, List<FormationType> formations, bool isLower) {
      CardSpaces = cardSpaces;
      Formations = formations;
      IsLower = isLower;
      attackFormation = new List<Card>();
      defendFormation = new List<Card>();
    }
  }
}