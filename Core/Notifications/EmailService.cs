using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pulse.Backend;
using Pulse.Core.Players;
using Pulse.Matchmaker.Matcher;
using Pulse.Matchmaker.Matches;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Pulse.Core.Notifications {
  public class EmailService {
    private readonly EmailConfig _emailConfig;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _domain;
    private readonly string _fromAddress;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, IServiceScopeFactory scopeFactory) {
      var appConfig = new AppConfig(configuration);
      _domain = appConfig.Domain;
      _emailConfig = new EmailConfig(configuration);
      _fromAddress = _emailConfig.FromAddress;
      _fromName = _emailConfig.FromName;
      _scopeFactory = scopeFactory;
    }

    private List<EmailAddress> _internalAddress {
      get {
        return (_emailConfig.InternalAddress ?? "")
          .Split(';')
          .Select(x => new EmailAddress(x))
          .ToList();
      }
    }

    public async void SendException(Exception ex) {
      var toAddress = _internalAddress;
      var subject = $"Pulse Unhandled: {ex.Message}";
      var body = ex.ToString();
      await this.SendMany(toAddress, subject, body);
    }

    public async void SendMatchmakerAddNotification(SeekModel seek) {
      var toAddress = _internalAddress;
      var subject = $"Pulse: PlayerId {seek.Player} joined the pool";
      var body = JsonConvert.SerializeObject(seek);
      await this.SendMany(toAddress, subject, body);
    }

    public async void SendAuthorizationLink(Player player) {
      var link = $"{_domain}/auth/login?email={player.Email}&code={player.AccessCode}";
      var subject = "Pulse Authorization Request";
      var message = "";
      if (!String.IsNullOrEmpty(player.Username)) message += $"Cheers {player.Username},<br><br>";
      message += $"Pulse Access Code: {player.AccessCode.ToUpper()}<br><br>";
      // message += $"Or click here: <a href='{link}'>{link}</a><br><br>";
      message += $"You are receiving this message because someone requested access via <a href='{_domain}'>{_domain}</a>.<br>";
      await Send(new EmailAddress(player.Email, player.Username), subject, message);
    }

    public async void SendMatchCreatedNotification(Match match, Player player) {
      var versus = String.Join(" vs ", match.MatchPlayers.OrderBy(x => x.Position).Select(x => x.Player.Username));

      var subject = $"{match.Name} in progress! {versus}";
      var body = @$"
                Hi {player.Username},
                <br><br>
                We found a match for you! Open the official Through The Ages app to play your game. Good luck, and have fun!
                <br><br>
                You can disable email notifications from the settings: {_domain}/settings#emailNotifications
            ";
      await this.Send(new EmailAddress(player.Email, player.Username), subject, body);
    }

    private async Task SendMany(List<EmailAddress> toAddress, string subject, string body) {
      var sends = new List<Task>();
      foreach (var to in toAddress) {
        sends.Add(Send(to, subject, body));
      }

      await Task.WhenAll(sends);
    }

    private Task<Response> Send(EmailAddress to, string subject, string body) {
      var log = new EmailLog() {
        Timestamp = DateTime.UtcNow,
        To = to.Email,
        Subject = subject,
        Body = body
      };

      using(var scope = _scopeFactory.CreateScope()) {
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
        dataContext.EmailLogs.Add(log);
        dataContext.SaveChanges();
      }

      var apiKey = _emailConfig.ApiKey;
      var client = new SendGridClient(apiKey);
      var from = new EmailAddress(_fromAddress, _fromName);
      var plainTextBody = body.Replace("<br>", Environment.NewLine);
      var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextBody, body);
      return client.SendEmailAsync(msg);
    }
  }
}