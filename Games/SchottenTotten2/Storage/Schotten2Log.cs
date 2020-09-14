using System;
using Pulse.Games.SchottenTotten2.Game;

namespace Pulse.Games.SchottenTotten2.Storage {
  public class Schotten2Log {
    public int Id { get; set; }
    public string MatchId { get; set; }
    public string PlayerId { get; set; }
    public string Action { get; set; }
    public int? HandIndex { get; set; }
    public int? SectionIndex { get; set; }
    public GameState State { get; set; }
    public DateTime Timestamp { get; set; }
  }
}