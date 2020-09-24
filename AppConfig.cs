using Microsoft.Extensions.Configuration;

namespace Pulse {
  public class AppConfig {
    private IConfiguration _configuration;
    public readonly string Domain;
    public readonly string CorsPolicyName = "AllowOrigins";
    public readonly string MatchmakerHubPath = "/matchmaker";
    public readonly string Schotten2HubPath = "/st2hub";
    public readonly string[] AllowedHosts = new string[] {
      "https://localhost:8888",
      "http://localhost:8000",
      "https://app.pulsegames.io",
    };

    public AppConfig(IConfiguration configuration) {
      _configuration = configuration;
      Domain = configuration.GetValue<string>("Server:Domain") ?? "https://localhost:5001";
    }
  }
}