using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Wall;

namespace Pulse.Games.SchottenTotten2.Game {
  public class GameConfig {
    public readonly int SuitCount = 5;
    public readonly int RankCount = 12;
    public readonly int HandSize = 6;
    public readonly int OilCount = 3;
    public readonly int OilIndex = 0;

    public Section GetSection(string name, bool isTopSide = true) {
      var section = new Section() { Name = name, IsDamaged = !isTopSide };
      switch (name) {
        case "Pit":
          {
            if (isTopSide) {
              var formationTypes = new List<FormationType>() { FormationType.RUN, FormationType.SUM };
              section.Setup(4, formationTypes, false);
            } else {
              var formationTypes = new List<FormationType>() { FormationType.RUN, FormationType.SUM };
              section.Setup(2, formationTypes, true);
            }
            break;
          }
        case "Tower":
          {
            if (isTopSide) {
              var formationTypes = new List<FormationType>() { FormationType.RUN, FormationType.SUM };
              section.Setup(4, formationTypes, false);
            } else {
              var formationTypes = new List<FormationType>() { FormationType.RUN, FormationType.SUM };
              section.Setup(2, formationTypes, true);
            }
            break;
          }
        case "Wall":
          {
            if (isTopSide) {
              var formationTypes = new List<FormationType>() { FormationType.RUN, FormationType.SUM };
              section.Setup(4, formationTypes, false);
            } else {
              var formationTypes = new List<FormationType>() { FormationType.RUN, FormationType.SUM };
              section.Setup(2, formationTypes, true);
            }
            break;
          }
        case "Door":
          {
            if (isTopSide) {
              var formationTypes = new List<FormationType>() { FormationType.RUN, FormationType.SUM };
              section.Setup(4, formationTypes, false);
            } else {
              var formationTypes = new List<FormationType>() { FormationType.RUN, FormationType.SUM };
              section.Setup(2, formationTypes, true);
            }
            break;
          }
      }
      return section;
    }
  }
}