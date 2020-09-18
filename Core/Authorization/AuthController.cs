using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Pulse.Core.Authorization {
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase {
    private readonly AuthService _authService;

    public AuthController(AuthService authService) {
      _authService = authService;
    }

    [HttpGet]
    [Route("request")]
    public ActionResult<bool> RequestAccess(string email) {
      try {
        return _authService.SendAccessCode(email);
      } catch (Exception ex) {
        return Unauthorized(ex.Message);
      }
    }

    [HttpGet]
    [Route("find")]
    public ActionResult<bool> Find(string username) {
      return _authService.FindPlayer(username);
    }

    [HttpGet]
    [Route("register")]
    public ActionResult<AuthResponse> Register(string email, string accessCode, string username) {
      try {
        var playerId = _authService.Register(email, accessCode, username);
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
        var browser = Request.Headers["User-Agent"].ToString();
        return _authService.CreateSession(playerId, email, ipAddress, accessCode);
      } catch (Exception ex) {
        return Unauthorized(ex.Message);
      }
    }

    /// <summary>
    /// Authorizes access to the player account and generates tokens.
    /// </summary>
    /// <param name="email">The email provided by the player</param>
    /// <param name="accessCode">The code provided by email</param>
    [HttpGet]
    [Route("login")]
    public ActionResult<AuthResponse> Login(string email, string accessCode) {
      try {
        var playerId = _authService.Login(email, accessCode);
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
        var browser = Request.Headers["User-Agent"].ToString();
        return _authService.CreateSession(playerId, email, ipAddress, accessCode);
      } catch (Exception ex) {
        return Unauthorized(ex.Message);
      }
    }

    [HttpGet]
    [Route("refresh")]
    public ActionResult<AuthResponse> Refresh(string accessToken, string refreshToken) {
      try {
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
        return _authService.Refresh(accessToken, refreshToken, ipAddress);
      } catch (SecurityTokenException ex) {
        return Unauthorized(ex.Message);
      } catch {
        return Unauthorized("Error refreshing token");
      }
    }
  }
}