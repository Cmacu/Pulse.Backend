using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.Games.SchottenTotten2.Schotten2 {

  [ApiController]
  [Authorize]
  [Route("[controller]")]
  public class Schotten2Controller : ControllerBase {
    private Schotten2Service _service;
    public Schotten2Controller(Schotten2Service service) {
      _service = service;
    }

    [HttpGet]
    [Route("")]
    public ActionResult<Schotten2Response> Load(string matchId) {
      var playerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
      var game = _service.Load(matchId);
      return _service.MapResponse(game, playerId);
    }

    [HttpGet]
    [Route("start")]
    public ActionResult<Schotten2Response> Start(string opponentId) {
      var playerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
      var players = new List<string>() { playerId, opponentId };
      var r = new Random();
      players.Sort((x, y) => r.Next(0, 100) - r.Next(0, 100));
      var matchId = Guid.NewGuid().ToString().Split('-') [2];
      _service.Start(players, matchId);
      return _service.MapResponse(_service.Load(matchId), playerId.ToString());
    }

    [HttpGet]
    [Route("retreat")]
    public ActionResult<Schotten2Response> Retreat(string matchId, int sectionIndex) {
      var playerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
      var game = _service.Retreat(matchId, playerId, sectionIndex);
      return _service.MapResponse(game, playerId.ToString());
    }

    [HttpGet]
    [Route("oil")]
    public ActionResult<Schotten2Response> UseOil(string matchId, int sectionIndex) {
      var playerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
      var game = _service.UseOil(matchId, playerId, sectionIndex);
      return _service.MapResponse(game, playerId.ToString());
    }

    [HttpGet]
    [Route("card")]
    public ActionResult<Schotten2Response> PlayCard(string matchId, int sectionIndex, int handIndex) {
      var playerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
      var game = _service.PlayCard(matchId, playerId, sectionIndex, handIndex);
      return _service.MapResponse(game, playerId.ToString());
    }

    [HttpGet]
    [Route("resign")]
    public ActionResult<Schotten2Response> Resign(string matchId) {
      var playerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
      var game = _service.Exit(matchId, playerId, Game.GameEvent.Resign);
      return _service.MapResponse(game, playerId.ToString());
    }

    [HttpGet]
    [Route("log")]
    public ActionResult<List<Schotten2Response>> Log(string matchId, int skip) {
      var playerId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
      return _service.GetLogs(matchId, playerId, skip);
    }
  }
}