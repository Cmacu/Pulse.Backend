using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// using Pulse.Matchmaker.Services;
// using Pulse.Core.Services;

namespace Pulse.Configuration
{
  /// <summary>
  /// Inversion of Control Container Configuration
  /// </summary>
  public static class IocContainerConfiguration
  {
    /// <summary>
    /// Configures dependency injection for all services.
    /// </summary>
    /// <param name="services">The collection of services that will receive injections.</param>
    /// <param name="configuration">Access to the Configuration object, if needed.</param>
    /// <param name="env">Access to the Environment object, if needed.</param>
    public static void ConfigureService(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      // services.AddScoped<IAuthService, AuthService>();
      // services.AddScoped<IPlayerService, PlayerService>();
      // services.AddScoped<IPlayerSettingService, PlayerSettingService>();

      // services.AddSingleton<IMatchmakerPoolSingleton, MatchmakerPoolSingleton>();
      // services.AddScoped<IMatchService, MatchService>();
      // services.AddScoped<IMatchmakerService, MatchmakerService>();
      // services.AddScoped<IMatchmakerLogService, MatchmakerLogService>();

      // services.AddScoped<ILeaderboardService, LeaderboardService>();
      // services.AddScoped<IRatingService, RatingService>();
      // services.AddScoped<IDecayService, DecayService>();

      // services.AddScoped<INotificationService, NotificationService>();
      // services.AddScoped<IEmailService, EmailService>();

      if (env.IsDevelopment())
      {
        services.AddDbContext<DataContext>(options =>
          options.UseMySQL(
            configuration.GetConnectionString("DefaultConnection")
          )
        );
      }
      else
      {
        services.AddDbContext<DataContext>(options =>
          options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection")
          )
        );
      }
    }
  }
}