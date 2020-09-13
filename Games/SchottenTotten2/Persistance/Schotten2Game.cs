using System;
using Pulse.Games.SchottenTotten2.Gameplay;

namespace Pulse.Games.SchottenTotten2.Persistance {
  public class Schotten2Game {
    public int Id { get; set; }
    public string Winner { get; set; }
    public string Attacker { get; set; }
    public string Defender { get; set; }
    public GameState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime DeletedAt { get; set; }
  }
}