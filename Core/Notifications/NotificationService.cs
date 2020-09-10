using System;
using Pulse.Core.PlayerSettings;
using Pulse.Matchmaker.Matches;

namespace Pulse.Core.Notifications {
    public class NotificationService {
        private readonly PlayerSettingService _playerSettingService;
        private readonly EmailService _emailService;

        public NotificationService(
            PlayerSettingService playerSettingService,
            EmailService emailService
        ) {
            _playerSettingService = playerSettingService;
            _emailService = emailService;
        }

        public void MatchCreated(Match match) {
            foreach (var matchPlayer in match.MatchPlayers) {
                if (_playerSettingService.Get(matchPlayer.PlayerId).EmailNotifications)
                    _emailService.SendMatchCreatedNotification(match, matchPlayer.Player);
            }
        }

        public void ExceptionCaught(Exception ex) {
            _emailService.SendException(ex);
        }
    }
}