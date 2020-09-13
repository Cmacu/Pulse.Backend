using System;
using Pulse.Games.SchottenTotten2.Gameplay;

namespace Pulse.Games.SchottenTotten2.Persistance {
  public class Schotten2Log {
    public int Id { get; set; }
    public int GameId { get; set; }
    public Schotten2Game Game { get; set; }
    public string Player { get; set; }
    public string Action { get; set; }
    public int? HandIndex { get; set; }
    public int? SectionIndex { get; set; }
    public GameState State { get; set; }
    public DateTime Timestamp { get; set; }
  }
}