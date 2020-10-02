using System;
using System.Collections.Generic;
using Pulse.Games.SchottenTotten2.Game;

namespace Pulse.Games.SchottenTotten2.Storage {
  public class Schotten2Game {
    public int Id { get; set; }
    public string MatchId { get; set; }
    public string AttackerId { get; set; }
    public string DefenderId { get; set; }
    public string ResignedId { get; set; }
    public string ExpiredId { get; set; }
    public string WinnerId { get; set; }
    public GameState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
  }
}