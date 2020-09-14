using System;
namespace Pulse.Games.SchottenTotten2.Storage {
  public class Schotten2Player {
    public int Id { get; set; }
    public string MatchId { get; set; }
    public string PlayerId { get; set; }
    public Schotten2Role Role { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}