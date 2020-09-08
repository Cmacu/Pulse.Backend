using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pulse.Backend
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        public static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json", optional : true)
                .AddEnvironmentVariables()
                .Build();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddEventSourceLogger();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
                {
                    webBuilder.UseUrls(_configuration["hosturl"]);
                }
                webBuilder.UseStartup<Startup>();
            });
    }
}