using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Pulse.Core.Models;
using Pulse.Core.Services;

namespace Pulse.Controllers {
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase {
    private readonly string _ipAddress;
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) {
      _authService = authService;
    }

    [HttpGet]
    [Route("request")]
    public ActionResult RequestAccess(string email) {
      try {
        _authService.SendAccessCode(email);
        return Ok();
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
    public ActionResult<AuthModel> Login(string email, string accessCode) {
      try {
        var browser = Request.Headers["User-Agent"].ToString();
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
        return _authService.Login(email, accessCode, ipAddress, browser);
      } catch (Exception ex) {
        return Unauthorized(ex.Message);
      }
    }

    [HttpGet]
    [Route("refresh")]
    public ActionResult<AuthModel> Refresh(string accessToken, string refreshToken) {
      try {
        return _authService.Refresh(accessToken, refreshToken, _ipAddress);
      } catch (SecurityTokenException ex) {
        return Unauthorized(ex.Message);
      } catch {
        return Unauthorized("Error refreshing token");
      }
    }
  }
}