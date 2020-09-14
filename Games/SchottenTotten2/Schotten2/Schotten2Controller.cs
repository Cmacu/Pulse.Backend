using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.Core.AppErrors;

namespace Pulse.Games.SchottenTotten2.Schotten2 {

  [ApiController]
  // [Authorize]
  [Route("[controller]")]
  public class Schotten2Controller : ControllerBase {
    private Schotten2Service _service;
    private string _playerId;
    public Schotten2Controller(Schotten2Service service) {
      _service = service;
      _playerId = "Cmacu"; //TODO: int.Parse(User.FindFirst(ClaimTypes.NameIdentifier));
      // if (string.IsNullOrEmpty(player)) throw new AuthException("Active session required to play Schotten 2");
    }

    [HttpGet]
    [Route("")]
    public ActionResult<Schotten2Response> Load() {
      return _service.Load(_playerId);
    }

    [HttpGet]
    [Route("start")]
    public ActionResult<Schotten2Response> Start(string opponentId) {
      if (string.IsNullOrEmpty(opponentId)) throw new ForbiddenException("OpponentId is required!");
      return _service.Start(_playerId, opponentId, "Test");
    }

    [HttpGet]
    [Route("retreat")]
    public ActionResult<Schotten2Response> Retreat(int sectionIndex) {
      return _service.Retreat(_playerId, sectionIndex);
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