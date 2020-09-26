using Microsoft.Extensions.Configuration;

namespace Pulse.Core.Notifications {
  public class EmailConfig {
    public readonly string InternalAddress = "admin@pulsegames.io";
    public readonly string FromAddress = "admin@pulsegames.io";
    public readonly string FromName = "Pulse Admin";
    public readonly string ApiKey;

    public EmailConfig(IConfiguration configuration) {
      ApiKey = configuration.GetValue<string>("EmailApiKey") ?? "";
    }
  }
}