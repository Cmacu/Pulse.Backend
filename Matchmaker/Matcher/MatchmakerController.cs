using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Matchmaker.Logs;

namespace Pulse.Matchmaker.Matcher {
  [Route("[controller]")]
  [ApiController]
  public class MatchmakerController : ControllerBase {
    private readonly MatchmakerLogService _matchmakerLogService;
    private readonly MatchmakerService _matchmakerService;

    public MatchmakerController(
      MatchmakerLogService matchmakerLogService,
      MatchmakerService matchmakerService
    ) {
      _matchmakerLogService = matchmakerLogService;
      _matchmakerService = matchmakerService;
    }

    /// <summary>
    /// Lists the currently active matchmaker log.
    /// </summary>
    /// <returns>Current matchmaker log</returns>
    [HttpGet]
    [Authorize]
    [Route("log")]
    public ActionResult<List<SeekModel>> Log() {
      return _matchmakerService.ListPlayers();
    }

    /// <summary>
    /// Lists the active user counts per hour in given timeframe
    /// </summary>
    /// <param name="utcFrom">Optional UTC DateTime as start of the timeframe</param>
    /// <param name="utcTo">Optional UTC DateTime as end of the timeframe</param>
    /// <returns>List of player activity percentages in given timeframe</returns>
    [HttpGet]
    // [Authorize]
    [Route("activity")]
    public ActionResult<List<int>> Activity(
      DateTime? utcFrom = null,
      DateTime? utcTo = null
    ) {
      DateTime toDate = utcTo ?? DateTime.UtcNow;
      DateTime fromDate = utcFrom ?? toDate.AddHours(-24);
      return _matchmakerLogService.ListPlayerCounts(fromDate, toDate) ?? new List<int>();
    }

    /// <summary>
    /// Lists the aggregated user counts per hour in given timeframe
    /// </summary>
    /// <param name="utcFrom">Optional UTC DateTime as start of the timeframe</param>
    /// <param name="utcTo">Optional UTC DateTime as end of the timeframe</param>
    /// <returns>List of player activity percentages in given timeframe</returns>
    [HttpGet]
    [Route("aggregate")]
    public ActionResult<List<int>> Aggregate(
      DateTime? utcFrom = null,
      DateTime? utcTo = null
    ) {
      DateTime toDate = utcTo ?? DateTime.UtcNow;
      DateTime fromDate = utcFrom ?? toDate.AddDays(-7);
      return _matchmakerLogService.ListPlayerAggregates(fromDate, toDate) ?? new List<int>();
    }

    /// <summary>
    /// Adds the specified player to the matchmaker player pool.
    /// </summary>
    /// <param name="playerId">The ID of the specified player.</param>
    /// <returns>Current matchmaker player pool</returns>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [Route("Add")]
    public ActionResult<List<SeekModel>> Add(int playerId) {
      _matchmakerService.AddPlayer(playerId.ToString());
      return _matchmakerService.ListPlayers();
    }
    /// <summary>
    /// Runs the matchmaker using the current player pool.
    /// </summary>
    /// <returns>List of generated matches</returns>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [Route("Run")]
    public ActionResult<List<MatchedPlayers>> Run() {
      var result = new List<MatchedPlayers>();
      var matches = _matchmakerService.Run((match) => Task.CompletedTask, (match, matchId) => Task.CompletedTask, (match) => Task.CompletedTask);
      if (matches == null) {
        return result;
      }
      result.AddRange(matches);
      return result;
    }

  }
}