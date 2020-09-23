using System;
using System.Collections.Generic;

namespace Pulse.Matchmaker.Matches {
  public class ResultModel {
    public MatchStatus Status { get; set; }
    public List<PlayerResultModel> Players { get; set; }
  }

  public class PlayerResultModel {
    public string PlayerId { get; set; }
    public int Score { get; set; }
    public MatchStatus Status { get; set; }
  }
}