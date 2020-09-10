using Microsoft.Extensions.Configuration;

namespace Pulse.Core.Authorization {

  public class AuthConfig {
    public readonly int TokenExpirySeconds = 3600;
    public readonly int RefreshTokenExpirationDays = 20;
    public readonly char[] JwtKey;
    public readonly string DomainRegEx = @"(@)(.+)$";
    public readonly string EmailRegEx =
      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
    public readonly string EmailError = "Invalid email: {0}";
    public readonly string RefreshTokenError = "Invalid refresh token";
    public readonly string AccessCodeError = "Invalid access code!";
    public readonly string AccessTokenError = "Invalid access token";
    public readonly string BlockedError = "Account is blocked until {0}. Contact admin for more details.";
    public readonly int MaxRequestCount = 10;
    public readonly string RequestCountError = "Too many access attempts. Accout is blocked. Contact admin to restore access.";
    public AuthConfig(IConfiguration configuration) {
      JwtKey = (configuration["Server:JwtKey"] ?? "pulsegames").ToCharArray();
    }
  }
}