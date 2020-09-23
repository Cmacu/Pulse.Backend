using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Game {
  public class GameConfig {
    public readonly int SuitCount = 5;
    public readonly int RankCount = 12;
    public readonly int HandSize = 6;
    public readonly int OilCount = 3;
    public readonly int OilIndex = 0;
    public readonly Dictionary<string, int> Archenemies = new Dictionary<string, int> { { "0", 11 }, { "11", 0 } };

    public Section GetSection(string name, bool isDamaged = false) {
      var cardSpaces = 3;
      var formationTypes = new List<FormationType>() {
        FormationType.SUIT_RUN,
        FormationType.SAME_RANK,
        FormationType.SAME_SUIT,
        FormationType.RUN,
        FormationType.SUM
      };

      if (name == "RightPit") {
        formationTypes = new List<FormationType>() { isDamaged ? FormationType.RUN : FormationType.LOW_SUM };
      } else if (name == "LeftPit") {
        formationTypes = new List<FormationType>() { isDamaged ? FormationType.RUN : FormationType.SUM };
      } else if (name == "Tower" && isDamaged) {
        cardSpaces = 2;
        formationTypes = new List<FormationType>() {
          FormationType.SAME_RANK,
          FormationType.SUM,
        };
      } else if (name == "Tower") {
        cardSpaces = 4;
      } else if (name == "Door" && isDamaged) {
        cardSpaces = 4;
        formationTypes.Add(FormationType.LOW_SUM);
      } else if (name == "Door") {
        cardSpaces = 2;
      }
      var section = new Section() {
        Name = name,
        IsDamaged = isDamaged,
        Spaces = cardSpaces,
        Types = formationTypes,
      };

      return section;
    }
  }
}