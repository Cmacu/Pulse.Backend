using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulse.Core.Authorization;
using Pulse.Core.Notifications;
using Pulse.Core.Players;
using Pulse.Core.PlayerSettings;
using Pulse.Games.SchottenTotten2.Game;
using Pulse.Games.SchottenTotten2.Schotten2;
using Pulse.Games.SchottenTotten2.Storage;
using Pulse.Matchmaker.Logs;
using Pulse.Matchmaker.Matcher;
using Pulse.Matchmaker.Matches;
using Pulse.Ranking.Decay;
using Pulse.Ranking.Leaderboard;
using Pulse.Ranking.Rating;

namespace Pulse.Backend {
  /// <summary>
  /// Inversion of Control Container Configuration
  /// </summary>
  public static class IocContainerConfiguration {
    /// <summary>
    /// Configures dependency injection for all services.
    /// </summary>
    /// <param name="services">The collection of services that will receive injections.</param>
    /// <param name="configuration">Access to the Configuration object, if needed.</param>
    /// <param name="env">Access to the Environment object, if needed.</param>
    public static void ConfigureService(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env) {
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddSingleton<MatchmakerPoolSingleton>();

      services.AddScoped<AuthService>();
      services.AddScoped<NotificationService>();
      services.AddScoped<EmailService>();
      services.AddScoped<PlayerService>();
      services.AddScoped<PlayerSettingService>();

      services.AddScoped<MatchService>();
      services.AddScoped<MatchmakerService>();
      services.AddScoped<MatchmakerLogService>();

      services.AddScoped<LeaderboardService>();
      services.AddScoped<RatingService>();
      services.AddScoped<DecayService>();

      services.AddSingleton<Schotten2Singleton>();
      services.AddScoped<Schotten2Service>();
      services.AddScoped<Schotten2Storage>();
      services.AddScoped<GameEngine>();

      if (env.IsDevelopment()) {
        services.AddDbContext<DataContext>(options =>
          options.UseMySql(
            configuration.GetConnectionString("DefaultConnection")
          )
        );
      } else {
        services.AddDbContext<DataContext>(options =>
          options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection")
          )
        );
      }
    }
  }
}