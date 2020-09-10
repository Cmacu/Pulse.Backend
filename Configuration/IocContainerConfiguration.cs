using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulse.Core.Services;
using Pulse.Matchmaker.Services;
using Pulse.Rank.Services;

namespace Pulse.Configuration {
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
      services.AddSingleton<ApiConfiguration>();

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

      if (env.IsDevelopment()) {
        services.AddDbContext<DataContext>(options =>
          options.UseMySql(
            configuration.GetConnectionString("DefaultConnection")
          ),
          ServiceLifetime.Transient
        );
      } else {
        services.AddDbContext<DataContext>(options =>
          options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection")
          ),
          ServiceLifetime.Transient
        );
      }
    }
  }
}