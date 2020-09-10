using System;
using Microsoft.Extensions.Configuration;

namespace Pulse.Configuration {
  public class ServerConfiguration {
    public readonly string Domain;
    public readonly string CorsPolicyName = "AllowOrigins";
    public readonly string MatchmakerHubPath = "/matchmaker";
    public readonly string[] AllowedHosts = new string[] {
      "https://localhost:8080",
      "http://localhost:8000",
      "https://app.pulsegames.io",
    };
    public ServerConfiguration(IConfiguration configuration) {
      Domain = configuration.GetValue<string>("Server:Domain") ?? "https://localhost:5001";
    }
  }
}