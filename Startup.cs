using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Pulse.Backend;
using Pulse.Core.Authorization;
using Pulse.Matchmaker.Matcher;

namespace Pulse {
  public class Startup {
    public IConfiguration _configuration { get; }
    private IWebHostEnvironment _env { get; set; }
    private string[] _corsOrigins;
    private string _corsPolicyName;
    private string _matchmakerHubPath;
    private byte[] _jwtKey;
    public Startup(IConfiguration configuration, IWebHostEnvironment env) {
      _configuration = configuration;
      _env = env;
      var appConfig = new AppConfig(configuration);
      _corsOrigins = appConfig.AllowedHosts;
      _corsPolicyName = appConfig.CorsPolicyName;
      _matchmakerHubPath = appConfig.MatchmakerHubPath;
      var authConfig = new AuthConfig(configuration);
      _jwtKey = Encoding.ASCII.GetBytes(authConfig.JwtKey);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      ConfigureSecurity(services);
      IocContainerConfiguration.ConfigureService(services, _configuration, _env);
      services.AddHsts(options => {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(60);
      });
      services.AddAutoMapper(typeof(Startup));
      services.AddMemoryCache();
      services.AddControllers();
      services.AddSignalR();
      // services.AddRouting();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseCors(_corsPolicyName);

      app.UseAuthentication();
      app.UseAuthorization();
      app.UseExceptionHandler("/error");

      app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
        endpoints.MapHub<MatchmakerHub>(_matchmakerHubPath);
      });
    }

    private void ConfigureSecurity(IServiceCollection services) {
      services.AddCors(options => {
        options.AddPolicy(name: _corsPolicyName, builder => {
          builder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithOrigins(_corsOrigins);
        });
      });

      services.AddAuthentication(x => {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      }).AddJwtBearer(options => {
        options.Events = new JwtBearerEvents {
          OnMessageReceived = context => {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(_matchmakerHubPath)) {
              context.Token = accessToken;
            }
            return Task.CompletedTask;
          }
        };
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(_jwtKey),
          ValidateIssuer = false,
          ValidateAudience = false,
        };
      });
    }
  }
}