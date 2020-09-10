using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Pulse.Configuration;
using Pulse.Core.Entities;
using Pulse.Core.Models;
using Pulse.Exceptions;

namespace Pulse.Core.Services {
    public class AuthService {
        private readonly DataContext _context;
        private readonly ApiConfiguration _configuration;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly EmailService _emailService;
        private readonly int _expirySeconds;
        private readonly byte[] _key;
        public AuthService(DataContext context, ApiConfiguration configuration, EmailService emailService) {
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _context = context;
            _configuration = configuration;
            _emailService = emailService;

            _expirySeconds = _configuration.Auth.TokenExpirySeconds;
            _key = Encoding.ASCII.GetBytes(_configuration.Auth.JwtKey);
        }

        public void SendAccessCode(string email) {
            if (!IsValidEmail(email))
                throw new PulseUnauthorizedException(string.Format(_configuration.Auth.EmailError, email));

            var player = GetPlayer(email);
            if (player == null) {
                player = new Player() { Email = email, CreatedAt = DateTime.UtcNow };
                _context.Player.Add(player);
            }
            player.AccessCode = Guid.NewGuid().ToString().Split('-') [1];
            player.UpdatedAt = DateTime.UtcNow;
            player.RequestCount++;
            _context.SaveChangesAsync();
            _emailService.SendAuthorizationLink(player);
        }

        public AuthModel Login(string email, string accessCode, string ipAddress, string browser) {
            var player = GetPlayer(email);
            if (player == null) throw new PulseUnauthorizedException(string.Format(_configuration.Auth.EmailError, email));
            if (player.AccessCode != accessCode) {
                player.RequestCount++;
                _context.SaveChanges();
                throw new PulseUnauthorizedException(_configuration.Auth.AccessCodeError);
            }

            var claims = new List<Claim>() {
                new Claim(ClaimTypes.Email, email)
            };
            var authModel = new AuthModel {
                AccessToken = GenerateToken(claims),
                ExpiresIn = _expirySeconds,
                RefreshToken = Guid.NewGuid().ToString()
            };

            var session = new PlayerSession {
                RefreshToken = authModel.RefreshToken,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                Browser = browser,
            };

            player.AccessCode = "";
            player.RequestCount = 0;
            player.UpdatedAt = DateTime.UtcNow;
            player.Sessions.Add(session);
            _context.SaveChanges();

            return authModel;
        }

        public AuthModel Refresh(string accessToken, string refreshToken, string ipAddress) {
            var parameters = new TokenValidationParameters() {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
            };

            var principal = _jwtSecurityTokenHandler.ValidateToken(accessToken, parameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
            ) throw new SecurityTokenException(_configuration.Auth.AccessTokenError);

            var email = principal.FindFirst(ClaimTypes.Email).Value;
            var refreshTokenExpirationDays = _configuration.Auth.RefreshTokenExpirationDays;
            var session = _context.PlayerSession
                .FirstOrDefault(x =>
                    x.RefreshToken == refreshToken &&
                    x.Player.Email == email &&
                    x.DeletedAt == null &&
                    x.UpdatedAt > DateTime.UtcNow.AddDays(-1 * refreshTokenExpirationDays)
                );

            if (session == null)
                throw new SecurityTokenException(_configuration.Auth.RefreshTokenError);

            var authModel = new AuthModel {
                AccessToken = GenerateToken(principal.Claims),
                ExpiresIn = _expirySeconds,
                RefreshToken = refreshToken
            };

            session.UpdatedAt = DateTime.UtcNow;
            _context.SaveChangesAsync();

            return authModel;
        }

        private string GenerateToken(IEnumerable<Claim> claims) {
            var expirySeconds = _configuration.Auth.TokenExpirySeconds;
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
                email = Regex.Replace(email, _configuration.Auth.DomainRegEx, DomainMapper,
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
                Console.WriteLine(string.Format(_configuration.Auth.EmailError, e.Message));
                return false;
            } catch (ArgumentException e) {
                Console.WriteLine(string.Format(_configuration.Auth.EmailError, e.Message));
                return false;
            }

            try {
                return Regex.IsMatch(email, _configuration.Auth.EmailRegEx,
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            } catch (RegexMatchTimeoutException) {
                return false;
            }
        }

        private Player GetPlayer(string email) {
            var player = _context.Player.FirstOrDefault(x => x.Email == email);
            if (player == null) return player;
            if (player.IsBlockedUntil != null && player.IsBlockedUntil < DateTime.UtcNow)
                throw new PulseUnauthorizedException(string.Format(_configuration.Auth.BlockedError, player.IsBlockedUntil));
            if (player.RequestCount > _configuration.Auth.MaxRequestCount)
                throw new PulseUnauthorizedException(_configuration.Auth.RequestCountError);
            return player;
        }
    }
}