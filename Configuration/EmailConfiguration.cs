using Microsoft.Extensions.Configuration;

namespace Pulse.Configuration {
  public class EmailConfiguration {
    public readonly string InternalAddress = "admin@pulsegames.io";
    public readonly string FromAddress = "admin@pulsegames.io";
    public readonly string FromName = "Pulse Admin";
    public readonly string ApiKey;

    public EmailConfiguration(IConfiguration configuration) {
      ApiKey = configuration.GetValue<string>("Email:ApiKey") ?? "";
    }
  }
}