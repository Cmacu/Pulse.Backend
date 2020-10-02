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
  }
}