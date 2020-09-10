using Microsoft.Extensions.Configuration;

namespace Pulse.Configuration {
  public class ApiConfiguration {
    private IConfiguration _configuration;
    public readonly ServerConfiguration Server;
    public readonly AuthConfiguration Auth;
    public readonly EmailConfiguration Email;
    public readonly RatingConfiguration Rating;
    public readonly DecayConfiguration Decay;
    public readonly LeaderboardConfiguration Leaderboard;
    public ApiConfiguration(IConfiguration configuration) {
      _configuration = configuration;
      Auth = new AuthConfiguration(_configuration);
      Server = new ServerConfiguration(_configuration);
      Email = new EmailConfiguration(_configuration);
      Rating = new RatingConfiguration();
      Decay = new DecayConfiguration();
      Leaderboard = new LeaderboardConfiguration();

    }
  }
}