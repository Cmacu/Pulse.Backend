using System;
using Pulse.Matchmaker.Entities;

namespace Pulse.Core.Services
{
    public interface INotificationService
    {
        void MatchCreated(Match match);
        void ExceptionCaught(Exception ex);
    }
    public class NotificationService : INotificationService
    {
        private readonly IPlayerSettingService _playerSettingService;
        private readonly IEmailService _emailService;

        public NotificationService(
            IPlayerSettingService playerSettingService,
            IEmailService emailService
        )
        {
            _playerSettingService = playerSettingService;
            _emailService = emailService;
        }

        public void MatchCreated(Match match)
        {
            foreach (var matchPlayer in match.MatchPlayers)
            {
                if (_playerSettingService.Get(matchPlayer.PlayerId).EmailNotifications)
                    _emailService.SendMatchCreatedNotification(match, matchPlayer.Player);
            }
        }

        public void ExceptionCaught(Exception ex)
        {
            _emailService.SendException(ex);
        }
    }
}