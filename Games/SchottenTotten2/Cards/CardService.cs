using System;
using System.Collections.Generic;

namespace Pulse.Games.SchottenTotten2.Cards {

  public class CardService {
    public List<Card> CreateDeck(int suitCount, int rankCount) {
      var deck = new List<Card>();

      for (int i = 0; i < suitCount; i++) {
        for (int j = 0; j < rankCount; j++) {
          var card = new Card() { Suit = i, Rank = j };
          deck.Add(card);
        }
      }

      return deck;
    }

    public Card DrawCard(List<Card> deck) {
      var lastIndex = deck.Count - 1;
      var topCard = deck[lastIndex];
      deck.RemoveAt(lastIndex);
      return topCard;
    }

    public List<Card> Shuffle(List<Card> deck) {
      return FisherYatesShuffle(deck);
    }

    private List<Card> FisherYatesShuffle(List<Card> deck) {
      var random = new Random();
      for (var i = deck.Count - 1; i > 0; i--) {
        var temp = deck[i];
        var index = random.Next(0, i + 1);
        deck[i] = deck[index];
        deck[index] = temp;
      }

      return deck;
    }
  }
}