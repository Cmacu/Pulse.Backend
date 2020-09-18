using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.Ranking.Leaderboard {
  [Route("[controller]")]
  [ApiController]
  public class LeaderboardController : ControllerBase {
    private readonly LeaderboardService _leaderboardService;

    private readonly IMapper _mapper;

    public LeaderboardController(LeaderboardService leaderboardService, IMapper mapper) {
      _leaderboardService = leaderboardService;
      _mapper = mapper;
    }

    /// <summary>
    /// Retrieve a sorted list of top-ranked players.
    /// </summary>
    /// <param name="skip">The number of players to skip.</param>
    /// <param name="take">The number of players to retrieve.</param>
    [HttpGet]
    [Route("")]
    public ActionResult<PagedLeaderboardResponse> Leaderboard(int skip, int take) {
      return _leaderboardService.Get(skip, take);
    }

    /// <summary>
    /// Retrieve a history of top-ranked players.
    /// </summary>
    /// <param name="username">The optional username of the player. If provided, only logs for the player will be returned.</param>
    [HttpGet]
    [Route("Log")]
    public ActionResult<List<LeaderboardLogResponse>> Log(string username) {
      var data = _leaderboardService.GetLog(username);
      var model = _mapper.Map<List<LeaderboardLogResponse>>(data);
      return model;
    }

    [HttpGet]
    // [Authorize(Roles = "Administrator")]
    [Route("clearcache")]
    public ActionResult ClearCache() {
      _leaderboardService.RemoveCache();
      return Ok();
    }

    [HttpGet]
    [Route("restart")]
    public ActionResult<PagedLeaderboardResponse> restart() {
      _leaderboardService.RemoveCache();
      _leaderboardService.Truncate();
      return _leaderboardService.Get(0, 10);
    }

  }
}