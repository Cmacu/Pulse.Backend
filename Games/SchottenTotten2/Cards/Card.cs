using Pulse.Core.AppErrors;

namespace Pulse.Games.SchottenTotten2.Cards {
  public class Card {
    private int? _suit;
    public int Suit {
      get {
        if (_suit == null) throw new InternalException("Card.Suit not set yet!");
        return (int) _suit;
      }
      set {
        if (_suit != null) throw new InternalException("Card.Suit can not be changed!");
        _suit = value;
      }
    }
    private int? _rank;
    public int Rank {
      get {
        if (_rank == null) throw new InternalException("Card.Rank not set yet!");
        return (int) _rank;
      }
      set {
        if (_rank != null) throw new InternalException("Card.Rank can not be changed!");
        _rank = value;
      }
    }
  }
}