using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pulse.Configuration;
using Pulse.Core.Entities;
using Pulse.Core.Models;
using Pulse.Exceptions;
using Pulse.Rank.Entities;

namespace Pulse.Core.Services {
    public interface IAuthService {
        string Register(string email, string ipAddress);
        AuthModel Refresh(string token, string refreshToken, string ipAddress);
    }

    public class AuthService : IAuthService {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        // private readonly IRatingService _ratingService;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly IEmailService _emailService;
        private readonly int _expirySeconds;
        private readonly byte[] _key;
        public AuthService(DataContext context, IConfiguration configuration, IEmailService emailService) {
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _context = context;
            _configuration = configuration;
            _emailService = emailService;

            _expirySeconds = int.Parse(_configuration["Server:TokenExpirySeconds"]);
            _key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        }

        public string Register(string email, string ipAddress) {
            if (!IsValidEmail(email))
                throw new MyUnauthorizedException("Invalid email: " + email);

            var player = _context.Player.FirstOrDefault(x => x.Email == email);
            if (player == null) {
                player = new Player() { Email = email };
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
                IpAddress = ipAddress
            };
            player.Sessions.Add(session);
            _emailService.SendAuthorizationLink(email, authModel.AccessToken, authModel.RefreshToken);
            _context.SaveChangesAsync();

            return "Please check your email for access link";
        }

        public AuthModel Refresh(string token, string refreshToken, string ipAddress) {
            var parameters = new TokenValidationParameters() {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
            };

            var principal = _jwtSecurityTokenHandler.ValidateToken(token, parameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid access token");

            var email = principal.FindFirst(ClaimTypes.Email).Value;
            var refreshTokenExpirationDays = _configuration.GetValue<int>("Server:RefreshTokenExpirationDays");
            var session = _context.PlayerSession
                .Include(x => x.Player)
                .FirstOrDefault(x =>
                    x.RefreshToken == refreshToken &&
                    x.Player.Email == email &&
                    x.DeletedAt == null &&
                    x.UpdatedAt > DateTime.UtcNow.AddDays(-1 * refreshTokenExpirationDays)
                );

            if (session == null)
                throw new SecurityTokenException("Invalid refresh token");

            var authModel = new AuthModel {
                AccessToken = GenerateToken(principal.Claims),
                ExpiresIn = _expirySeconds,
                RefreshToken = refreshToken
            };

            return authModel;
        }

        private string GenerateToken(IEnumerable<Claim> claims) {
            var expirySeconds = int.Parse(_configuration["Server:TokenExpirySeconds"]);
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(expirySeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = _jwtSecurityTokenHandler.CreateToken(tokenDescriptor);

            return _jwtSecurityTokenHandler.WriteToken(token);
        }

        private bool IsValidEmail(string email) {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
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
                Console.WriteLine("Invalid email: " + e.Message);
                return false;
            } catch (ArgumentException e) {
                Console.WriteLine("Invalid email: " + e.Message);
                return false;
            }

            try {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            } catch (RegexMatchTimeoutException) {
                return false;
            }
        }
    }
}