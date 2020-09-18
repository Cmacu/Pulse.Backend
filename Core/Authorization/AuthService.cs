using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pulse.Backend;
using Pulse.Core.AppErrors;
using Pulse.Core.Notifications;
using Pulse.Core.Players;

namespace Pulse.Core.Authorization {
  public class AuthService {
    private readonly DataContext _context;
    private readonly AuthConfig _authConfig;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly EmailService _emailService;
    private readonly int _expirySeconds;
    private readonly byte[] _key;
    public AuthService(DataContext context, IConfiguration configuration, EmailService emailService) {
      _context = context;
      _emailService = emailService;

      _authConfig = new AuthConfig(configuration);
      _expirySeconds = _authConfig.TokenExpirySeconds;
      _key = Encoding.ASCII.GetBytes(_authConfig.JwtKey);

      _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    public bool SendAccessCode(string email) {
      var needsRegistration = false;
      if (!IsValidEmail(email))
        throw new AuthException(string.Format(_authConfig.EmailError, email));

      var player = GetPlayer(email);
      if (player == null) {
        needsRegistration = true;
        player = new Player() { Email = email, CreatedAt = DateTime.UtcNow };
        _context.Players.Add(player);
      }
      player.AccessCode = Guid.NewGuid().ToString().Split('-') [1];
      player.UpdatedAt = DateTime.UtcNow;
      player.RequestCount++;
      _context.SaveChanges();
      _emailService.SendAuthorizationLink(player);
      return needsRegistration;
    }

    public int Login(string email, string accessCode) {
      var player = Authorize(email, accessCode);
      _context.SaveChanges();

      return player.Id;
    }

    public bool FindPlayer(string username) {
      return _context.Players.FirstOrDefault(x => x.Username == username) != null;
    }

    public int Register(string email, string accessCode, string username) {
      var player = Authorize(email, accessCode);

      if (string.IsNullOrEmpty(username) || username.Length < 3) throw new AuthException(_authConfig.UsernameError);

      player.Username = username;
      _context.SaveChanges();

      return player.Id;
    }

    public AuthResponse Refresh(string accessToken, string refreshToken, string ipAddress) {
      var parameters = new TokenValidationParameters() {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateLifetime = false,
        IssuerSigningKey = new SymmetricSecurityKey(_key),
      };

      var principal = _jwtSecurityTokenHandler.ValidateToken(accessToken, parameters, out var securityToken);

      if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
        !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
      ) throw new SecurityTokenException(_authConfig.AccessTokenError);

      var email = principal.FindFirst(ClaimTypes.Email).Value;
      var refreshTokenExpirationDays = _authConfig.RefreshTokenExpirationDays;
      var session = _context.Sessions
        .FirstOrDefault(x =>
          x.RefreshToken == refreshToken &&
          x.Player.Email == email &&
          x.DeletedAt == null &&
          x.UpdatedAt > DateTime.UtcNow.AddDays(-1 * refreshTokenExpirationDays)
        );

      if (session == null)
        throw new SecurityTokenException(_authConfig.RefreshTokenError);

      var authResponse = new AuthResponse {
        AccessToken = GenerateToken(principal.Claims),
        ExpiresIn = _expirySeconds,
        RefreshToken = refreshToken
      };

      session.UpdatedAt = DateTime.UtcNow;
      _context.SaveChanges();

      return authResponse;
    }

    private Player Authorize(string email, string accessCode) {
      if (string.IsNullOrEmpty(email)) throw new AuthException(string.Format(_authConfig.EmailError, email));
      if (string.IsNullOrEmpty(accessCode)) throw new AuthException(_authConfig.AccessCodeError);

      var player = GetPlayer(email);
      if (player == null) throw new AuthException(string.Format(_authConfig.EmailError, email));
      if (player.AccessCode.ToLower() != accessCode.ToLower()) {
        player.RequestCount++;
        _context.SaveChanges();
        throw new AuthException(_authConfig.AccessCodeError);
      }
      player.AccessCode = null;
      player.RequestCount = 0;
      player.UpdatedAt = DateTime.UtcNow;

      return player;
    }

    private string GenerateToken(IEnumerable<Claim> claims) {
      var expirySeconds = _authConfig.TokenExpirySeconds;
      var tokenDescriptor = new SecurityTokenDescriptor {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddSeconds(expirySeconds),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
      };
      var accessToken = _jwtSecurityTokenHandler.CreateToken(tokenDescriptor);

      return _jwtSecurityTokenHandler.WriteToken(accessToken);
    }

    private bool IsValidEmail(string email) {
      if (string.IsNullOrWhiteSpace(email))
        return false;

      try {
        // Normalize the domain
        email = Regex.Replace(email, _authConfig.DomainRegEx, DomainMapper,
          RegexOptions.None, TimeSpan.FromMilliseconds(200));

        // Examines the domain part of the email and normalizes it.
        string DomainMapper(Match match) {
          // Use IdnMapping class to convert Unicode domain names.
          var idn = new IdnMapping();

          // Pull out and process domain name (throws ArgumentException on invalid)
          var domainName = idn.GetAscii(match.Groups[2].Value);

          return match.Groups[1].Value + domainName;
        }
      } catch (RegexMatchTimeoutException e) {
        Console.WriteLine(string.Format(_authConfig.EmailError, e.Message));
        return false;
      } catch (ArgumentException e) {
        Console.WriteLine(string.Format(_authConfig.EmailError, e.Message));
        return false;
      }

      try {
        return Regex.IsMatch(email, _authConfig.EmailRegEx,
          RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
      } catch (RegexMatchTimeoutException) {
        return false;
      }
    }

    private Player GetPlayer(string email) {
      var player = _context.Players.FirstOrDefault(x => x.Email == email);
      if (player == null) return player;
      if (player.IsBlockedUntil != null && player.IsBlockedUntil < DateTime.UtcNow)
        throw new AuthException(string.Format(_authConfig.BlockedError, player.IsBlockedUntil));
      if (player.RequestCount > _authConfig.MaxRequestCount)
        throw new AuthException(_authConfig.RequestCountError);
      return player;
    }

    public AuthResponse CreateSession(int playerId, string email, string ipAddress, string browser) {
      var claims = new List<Claim>() {
        new Claim(ClaimTypes.NameIdentifier, playerId.ToString()),
        new Claim(ClaimTypes.Email, email)
      };

      var authResponse = new AuthResponse {
        AccessToken = GenerateToken(claims),
        ExpiresIn = _expirySeconds,
        RefreshToken = Guid.NewGuid().ToString()
      };

      var session = new Session {
        PlayerId = playerId,
        RefreshToken = authResponse.RefreshToken,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IpAddress = ipAddress,
        Browser = browser,
      };

      _context.Sessions.Add(session);
      _context.SaveChanges();

      return authResponse;
    }
  }
}