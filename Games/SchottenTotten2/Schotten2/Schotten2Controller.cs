using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.Games.SchottenTotten2.Schotten2 {

  [ApiController]
  // [Authorize]
  [Route("[controller]")]
  public class Schotten2Controller : ControllerBase {
    private Schotten2Service _service;
    public Schotten2Controller(Schotten2Service service) {
      _service = service;
    }

    [HttpGet]
    [Route("")]
    public ActionResult<Schotten2Response> Load(string playerId) {
      // var playerId = User.FindFirst(ClaimTypes.NameIdentifier).ToString();
      return _service.Load(playerId);
    }

    [HttpGet]
    [Route("start")]
    public ActionResult<Schotten2Response> Start(string playerId, string opponentId) {
      // var playerId = User.FindFirst(ClaimTypes.NameIdentifier).ToString();
      var r = new Random();
      var players = new List<string>() { playerId, opponentId };
      players.Sort((x, y) => r.Next(0, 100) - r.Next(0, 100));
      _service.Start(players[0], players[1], Guid.NewGuid().ToString().Split('-') [2]);
      return _service.Load(playerId);
    }

    [HttpGet]
    [Route("retreat")]
    public ActionResult<Schotten2Response> Retreat(string playerId, int sectionIndex) {
      // var playerId = User.FindFirst(ClaimTypes.NameIdentifier).ToString();
      return _service.Retreat(playerId, sectionIndex);
    }

    [HttpGet]
    [Route("oil")]
    public ActionResult<Schotten2Response> UseOil(string playerId, int sectionIndex) {
      // var playerId = User.FindFirst(ClaimTypes.NameIdentifier).ToString();
      return _service.UseOil(playerId, sectionIndex);
    }

    [HttpGet]
    [Route("card")]
    public ActionResult<Schotten2Response> PlayCard(string playerId, int sectionIndex, int handIndex) {
      // var playerId = User.FindFirst(ClaimTypes.NameIdentifier).ToString();
      return _service.PlayCard(playerId, sectionIndex, handIndex);
    }

    [HttpGet]
    [Route("resign")]
    public ActionResult<Schotten2Response> Resign(string playerId) {
      // var playerId = User.FindFirst(ClaimTypes.NameIdentifier).ToString();
      return _service.Exit(playerId, ExitType.Resign);
    }
  }
}