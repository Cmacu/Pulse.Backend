using System;
using System.Collections.Generic;
using Pulse.Matchmaker.Entities;

namespace Pulse.Matchmaker.Models
{
  public class ResultModel
  {
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<PlayerResultModel> Players { get; set; }
    public MatchStatus Status { get; set; }
  }

  public class PlayerResultModel
  {
    public string Username { get; set; }
    public int Score { get; set; }
    public MatchStatus Status { get; set; }
  }
}