using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.Games.SchottenTotten2.Schotten2 {

  [ApiController]
  // [Authorize]
  [Route("[controller]")]
  public class Schotten2Controller : ControllerBase {
    private Schotten2Service _service;
    private string _player;
    public Schotten2Controller(Schotten2Service service) {
      _service = service;
      _player = "1"; //TODO: User.FindFirst(ClaimTypes.NameIdentifier).ToString();
    }

    [HttpGet]
    [Route("{gameId}")]
    public ActionResult<Schotten2Response> GetGame(int gameId) {
      return _service.GetGame(_player, gameId);
    }

    [HttpGet]
    [Route("create")]
    public ActionResult<Schotten2Response> CreateGame(string opponent) {
      return _service.CreateGame(_player, opponent);
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