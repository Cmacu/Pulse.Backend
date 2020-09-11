using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Pulse.Matchmaker.Matcher {
    // Defines interface for sending messages to the client
    // Replaces magic strings, example:
    // await Clients.All.SendAsync("SendMessage", message) => await Clients.All.SendMessage(message)
    public interface IMatchmakerHub {
        Task ScheduleMatchmakerRun(int milliseconds);
        Task Matched(string message);
        Task Playing(string message);
        Task Error(string message);
        Task SendMessage(string message);
        Task Disconnect(string message);
    }

    [Authorize]
    public class MatchmakerHub : Hub<IMatchmakerHub> {
        private readonly int _matchmakerRunTimeout = 30 * 1000; // milliseconds
        private readonly MatchmakerService _matchmakerService;
        public MatchmakerHub(MatchmakerService matchmakerService) {
            this._matchmakerService = matchmakerService;
        }

        public override Task OnConnectedAsync() {
            var player = this.getContextPlayer();
            Console.WriteLine("SignalR: Add Player: " + player);
            // Run the matchmaker in x milliseconds
            this.Clients.Users(player).ScheduleMatchmakerRun(_matchmakerRunTimeout);
            if (player != null) {
                try {
                    _matchmakerService.AddPlayer(player);
                } catch (Exception exception) {
                    Console.WriteLine(exception.Message);
                    this.Clients.User(player).Disconnect(exception.Message);
                }
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception) {
            var player = this.getContextPlayer();
            Console.WriteLine("SignalR: Remove Player: " + player);
            this.Clients.User(player).Disconnect("Cancel search");

            if (player != null) {
                _matchmakerService.RemovePlayer(player);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task RunMatchmaker() {
            Console.WriteLine("SignalR: Run Matchmaker");
            await this.Clients.All.ScheduleMatchmakerRun(_matchmakerRunTimeout);
            var matches = _matchmakerService.Run(this.OnMatched, this.OnPlaying, this.OnError);
        }

        private string getContextPlayer() {
            return Context.UserIdentifier ?? "";
        }

        private async void HandleMatches(IReadOnlyList<string> match) {
            await this.Clients.Users(match).Matched("Match found");
            var matchName = "1"; // this._cgeService.CreateMatch(match[0], match[1]);
            if (matchName.Length == 0) {
                return;
            }

            await this.Clients.Users(match).Playing(matchName + " match started!");
        }

        private Task OnMatched(IReadOnlyList<string> match) {
            return this.Clients.Users(match).Matched("Match found");
        }

        private Task OnError(IReadOnlyList<string> match) {
            return this.Clients.Users(match).Error("Error creating match.");
        }

        private Task OnPlaying(IReadOnlyList<string> match) {
            return this.Clients.Users(match).Playing("Match started!");
        }
    }
}