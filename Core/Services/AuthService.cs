using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pulse.Configuration;
using Pulse.Core.Entities;
using Pulse.Core.Models;
using Pulse.Exceptions;
using Pulse.Rating.Entities;

namespace Pulse.Core.Services
{
    public interface IAuthService
    {
        AuthModel Authenticate(string cgeToken, string cgeRefreshToken, string ipAddress);
        AuthModel Refresh(string jwt, string jwtRefresh, string ipAddress);
        string RefreshCgeToken(string cgeRefreshToken);
    }

    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        // private readonly IRatingService _ratingService;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly byte[] _key;

        public AuthService(DataContext context, IConfiguration configuration)
        {
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            _context = context;
            _configuration = configuration;
            // _ratingService = ratingService;

            _key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        }

        public AuthModel Authenticate(string token, string refreshToken, string ipAddress)
        {
            var session = _context.PlayerSession.Include(x => x.Player).Where(x => x.RefreshToken == refreshToken).FirstOrDefault();
            // Generate and save app JWT token
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, session.Player.Username.ToString()),
                new Claim(ClaimTypes.NameIdentifier, session.Player.Id.ToString())
            };
            var authModel = GenerateJwt(claims);

            // player.Sessions.Add(new PlayerSession()
            // {
            //     RefreshToken = authModel.RefreshToken,
            //         CreatedAt = DateTime.UtcNow,
            //         UpdatedAt = DateTime.UtcNow,
            //         IpAddress = ipAddress
            // });

            _context.SaveChanges();

            return authModel;
        }

        public AuthModel Refresh(string jwt, string jwtRefresh, string ipAddress)
        {
            var parameters = new TokenValidationParameters()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
            };

            var principal = _jwtSecurityTokenHandler.ValidateToken(jwt, parameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid access token");

            var playerId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier).Value);
            var auth = _context.PlayerSession.FirstOrDefault(x =>
                x.RefreshToken == jwtRefresh &&
                x.PlayerId == playerId &&
                x.DeletedAt == null &&
                x.UpdatedAt > DateTime.UtcNow.AddDays(-1 * _configuration.GetValue<int>("Cge:RefreshExpiresAtDays")));

            if (auth == null)
                throw new SecurityTokenException("Invalid refresh token");

            var newJwt = GenerateJwt(principal.Claims);

            // Keep the same refresh token, so it can be reused
            newJwt.RefreshToken = jwtRefresh;

            return newJwt;
        }

        public string RefreshCgeToken(string cgeRefreshToken)
        {
            var token = "";
            if (token == null)
                throw new MyUnauthorizedException("Player not found. Invalid refresh token");

            return "";
        }

        private AuthModel GenerateJwt(IEnumerable<Claim> claims)
        {
            var jwtRefresh = GenerateToken();
            var expirySeconds = int.Parse(_configuration["Jwt:ExpirySeconds"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(expirySeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256Signature)
            };
            var jwt = _jwtSecurityTokenHandler.CreateToken(tokenDescriptor);

            return new AuthModel()
            {
                AccessToken = _jwtSecurityTokenHandler.WriteToken(jwt),
                    ExpiresIn = expirySeconds,
                    RefreshToken = jwtRefresh
            };
        }

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }
    }
}