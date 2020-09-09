using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pulse.Configuration;
using Pulse.Core.Entities;
using Pulse.Matchmaker.Entities;
using Pulse.Matchmaker.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Pulse.Core.Services {
    public interface IEmailService {
        Task SendException(Exception ex);
        Task SendAuthorizationLink(string email, string accessToken, string refreshToken);
        Task SendMatchmakerAddNotification(SeekModel seek);
        Task SendMatchCreatedNotification(Match match, Player player);
    }

    public class EmailService : IEmailService {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public EmailService(IConfiguration configuration, DataContext context) {
            _configuration = configuration;
            _context = context;
        }

        private List<EmailAddress> _internalAddress {
            get {
                return (_configuration.GetValue<string>("Email:InternalAddress") ?? "")
                    .Split(';')
                    .Select(x => new EmailAddress(x))
                    .ToList();
            }
        }

        public async Task SendException(Exception ex) {
            var toAddress = _internalAddress;
            var subject = $"Pulse Unhandled: {ex.Message}";
            var body = ex.ToString();
            await this.SendMany(toAddress, subject, body);
        }

        public async Task SendMatchmakerAddNotification(SeekModel seek) {
            var toAddress = _internalAddress;
            var subject = $"Pulse: PlayerId {seek.Player} joined the pool";
            var body = JsonConvert.SerializeObject(seek);
            await this.SendMany(toAddress, subject, body);
        }

        public Task SendAuthorizationLink(string email, string accessToken, string refreshToken) {
            var subject = "Pulse Authorization Link";
            var link = $"https://app.pulsegames.io/login?token={accessToken}&refreshToken={refreshToken}";
            var body = @$"<a href='{link}'>{link}</a>";
            return Send(new EmailAddress(email, ""), subject, body);
        }

        public async Task SendMatchCreatedNotification(Match match, Player player) {
            var versus = String.Join(" vs ", match.MatchPlayers.OrderBy(x => x.Position).Select(x => x.Player.Username));

            var subject = $"{match.Name} in progress! {versus}";
            var body = @$"
                Hi {player.Username},
                <br><br>
                We found a match for you! Open the official Through The Ages app to play your game. Good luck, and have fun!
                <br><br>
                You can disable email notifications from the settings: https://app.ttapulse.com/settings#emailNotifications
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

        private async Task<Response> Send(EmailAddress to, string subject, string body) {
            _context.EmailLog.Add(new EmailLog() {
                Timestamp = DateTime.UtcNow,
                    To = to.Email,
                    Subject = subject,
                    Body = body
            });
            _context.SaveChanges();

            var apiKey = _configuration.GetValue<string>("Email:ApiKey");
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetValue<string>("Email:FromAddr"), _configuration.GetValue<string>("Email:FromName"));
            var plainTextBody = body.Replace("<br>", Environment.NewLine);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextBody, body);
            return await client.SendEmailAsync(msg);
        }
    }
}