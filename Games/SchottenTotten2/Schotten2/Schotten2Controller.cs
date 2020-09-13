using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Games.SchottenTotten2.Gameplay;

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
    [Route("{gameId}")]
    public ActionResult<Schotten2Response> GetGame(int gameId) {
      var player = "1";
      return _service.GetGame(gameId, player);
    }

    [HttpGet]
    [Route("create")]
    public ActionResult<Schotten2Response> CreateGame(string player, string opponent) {
      return _service.CreateGame(player, opponent);
    }

    [HttpPost]
    [Route("retreat")]
    public ActionResult<Schotten2Response> Retreat(int wallIndex) {
      return new Schotten2Response();
    }

    [HttpPost]
    [Route("oil")]
    public ActionResult<Schotten2Response> UseOil(int wallIndex) {
      return new Schotten2Response();
    }

    [HttpPost]
    [Route("card")]
    public ActionResult<Schotten2Response> PlayCard(int handIndex, int wallIndex) {
      return new Schotten2Response();
    }

    [HttpPost]
    [Route("controll")]
    public ActionResult<Schotten2Response> Controll(int wallIndex) {
      return new Schotten2Response();
    }

    [HttpPost]
    [Route("resign")]
    public ActionResult<Schotten2Response> Resign() {
      return new Schotten2Response();
    }
  }
}