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

    public Section GetSection(SectionStyle style, bool isDamaged = false) {
      var cardSpaces = 3;
      var formationTypes = new List<FormationType>() {
        FormationType.SUIT_RUN,
        FormationType.SAME_RANK,
        FormationType.SAME_SUIT,
        FormationType.RUN,
        FormationType.SUM
      };

      switch (style) {
        case SectionStyle.RightPit:
          cardSpaces = 3;
          formationTypes = isDamaged ?
            new List<FormationType>() { FormationType.RUN, FormationType.SUM } :
            new List<FormationType>() { FormationType.LOW_SUM };
          break;
        case SectionStyle.LeftPit:
          formationTypes = isDamaged ?
            new List<FormationType>() { FormationType.RUN, FormationType.SUM } :
            new List<FormationType>() { FormationType.SUM };
          break;
        case SectionStyle.Tower:
          if (isDamaged) {
            cardSpaces = 2;
            formationTypes = new List<FormationType>() {
              FormationType.SAME_RANK,
              FormationType.SUM,
            };
          } else {
            cardSpaces = 4;
          }
          break;
        case SectionStyle.Door:
          if (isDamaged) {
            cardSpaces = 4;
            formationTypes = new List<FormationType>() { FormationType.LOW_SUM };
          } else {
            cardSpaces = 2;
          }
          break;
      }

      var section = new Section() {
        Style = style,
        Name = style.ToString("F"),
        IsDamaged = isDamaged,
        Spaces = cardSpaces,
        Types = formationTypes,
      };

      return section;
    }
  }
}