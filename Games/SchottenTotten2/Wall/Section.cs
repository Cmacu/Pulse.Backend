using System.Collections.Generic;
using Pulse.Core.AppErrors;
using Pulse.Games.SchottenTotten2.Cards;

namespace Pulse.Games.SchottenTotten2.Wall {
  public class Section {
    private string _name;
    public SectionStyle Style { get; set; }
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
    public bool IsDamaged { get; set; } = false;
    public int Spaces { get; set; }
    public List<FormationType> Types { get; set; }
    public List<Card> Attack { get; set; } = new List<Card>();
    public List<Card> Defense { get; set; } = new List<Card>();
    public int MaxTries { get; set; } = 0;

    private double _attackStrength = 0;
    private List<Card> _extraCards = new List<Card>();
    private int _tryCounter = 0;

    public bool CanDefend(List<Card> extraCards) {
      if (Attack.Count != Spaces) return true;

      _attackStrength = GetFormationStrength(Attack);

      if (Defense.Count == Spaces)
        return GetFormationStrength(Defense) >= _attackStrength;

      if (_attackStrength == GetMaxStrength()) return false;

      _extraCards = extraCards;
      if (Types[0] == FormationType.LOW_SUM) _extraCards.Reverse();

      _tryCounter = 0;
      var canDefend = GenerateFormations(Defense, 0);
      if (_tryCounter > MaxTries) MaxTries = _tryCounter;

      return canDefend;
    }

    private bool GenerateFormations(List<Card> testFormation, int cardIndex) {
      _tryCounter++;
      if (testFormation.Count >= Spaces)
        return GetFormationStrength(testFormation) > _attackStrength;

      for (var i = cardIndex; i < _extraCards.Count; i++) {
        var newFormation = new List<Card>(testFormation);
        newFormation.Add(_extraCards[i]);
        if (GenerateFormations(newFormation, i + 1)) return true;
      }

      return false;
    }

    public List<Card> SortFormation(List<Card> cards) {
      var sorted = new List<Card>();
      sorted.AddRange(cards);
      sorted.Sort((x, y) => y.Rank - x.Rank);
      return sorted;
    }

    private double GetFormationStrength(List<Card> formation) {
      var sum = SumFormation(formation) / 100d;
      formation = SortFormation(formation);
      foreach (var formationType in Types) {
        if (formationType == FormationType.SUIT_RUN && CheckSuit(formation) && CheckRun(formation)) {
          return (int) FormationType.SUIT_RUN + sum;
        }
        if (formationType == FormationType.SAME_RANK && CheckRank(formation)) {
          return (int) FormationType.SAME_RANK + sum;
        }
        if (formationType == FormationType.SAME_SUIT && CheckSuit(formation)) {
          return (int) FormationType.SAME_SUIT + sum;
        }
        if (formationType == FormationType.RUN && CheckRun(formation)) {
          return (int) FormationType.RUN + sum;
        }
        if (formationType == FormationType.LOW_SUM) {
          return (int) FormationType.LOW_SUM - sum;
        }
      }
      return sum;
    }

    private bool CheckSuit(List<Card> formation) {
      var card = formation[0];
      for (var i = 1; i < formation.Count; i++) {
        var next = formation[i];
        if (card.Suit != next.Suit) return false;
        card = next;
      }
      return true;
    }

    private bool CheckRank(List<Card> formation) {
      var card = formation[0];
      for (var i = 1; i < formation.Count; i++) {
        var next = formation[i];
        if (card.Rank != next.Rank) return false;
        card = next;
      }
      return true;
    }

    private bool CheckRun(List<Card> formation) {
      var card = formation[0];
      for (var i = 1; i < formation.Count; i++) {
        var next = formation[i];
        if (card.Rank != next.Rank - 1) return false;
        card = next;
      }
      return true;
    }

    private int SumFormation(List<Card> formation) {
      var sum = 0;
      foreach (var card in formation)
        sum += card.Rank;
      return sum;
    }

    private double GetMaxStrength() {
      var topFormation = Types[0];
      if (topFormation == FormationType.LOW_SUM) return 0.99;
      if (topFormation == FormationType.SAME_SUIT)
        return (int) FormationType.SAME_SUIT + Spaces * 0.11;

      var topRank = 11;
      var maxStrength = 0d;
      for (var x = topRank; x > topRank - Spaces; x--)
        maxStrength += x;

      maxStrength = maxStrength / 100d;
      maxStrength += (double) topFormation;
      return maxStrength;
    }
  }
}