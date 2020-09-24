using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pulse.Games.SchottenTotten2.Storage;

namespace Pulse.Games.SchottenTotten2.Schotten2 {
  // Defines interface for sending messages to the client
  // Replaces magic strings, example:
  // await Clients.All.SendAsync("SendMessage", message) => await Clients.All.SendMessage(message)
  public interface ISchotten2Hub {
    Task SendDisconnect(string message);
    Task UpdateState(Schotten2Response state);
  }

  [Authorize]
  public class Schotten2Hub : Hub<ISchotten2Hub> {
    private readonly Schotten2Service _service;
    public Schotten2Hub(Schotten2Service service) {
      _service = service;
    }

    public override Task OnConnectedAsync() {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      // Console.WriteLine($"Player connected: {playerId} Match: {matchId}");
      Groups.AddToGroupAsync(Context.ConnectionId, matchId);
      var game = _service.Load(matchId);
      Clients.User(playerId).UpdateState(_service.MapResponse(game, playerId));
      return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception) {
      var playerId = GetPlayerId();
      // Console.WriteLine($"Player disconnected: {playerId}");
      return base.OnDisconnectedAsync(exception);
    }

    public void PlayCard(int sectionIndex, int handIndex) {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      Console.WriteLine($"Play Card: Section {sectionIndex}, Card {handIndex}, Player {playerId}, Match {matchId}");
      var game = _service.PlayCard(matchId, playerId, sectionIndex, handIndex);
      SendState(game, sectionIndex);
    }

    public void Retreat(int sectionIndex) {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      Console.WriteLine($"Retreat: Section {sectionIndex}, Player {playerId}, Match {matchId}");
      var game = _service.Retreat(matchId, playerId, sectionIndex);
      SendState(game, sectionIndex);
    }

    public void UseOil(int sectionIndex) {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      Console.WriteLine($"Use Oil: Section {sectionIndex}, Player {playerId}, Match {matchId}");
      var game = _service.UseOil(matchId, playerId, sectionIndex);
      SendState(game, sectionIndex);
    }

    private Task SendState(Schotten2Game game, int sectionIndex) {
      Clients.User(game.AttackerId).UpdateState(_service.MapResponse(game, game.AttackerId, sectionIndex));
      Clients.User(game.DefenderId).UpdateState(_service.MapResponse(game, game.DefenderId, sectionIndex));
      return Task.CompletedTask;
    }

    private string GetPlayerId() {
      return Context.UserIdentifier ?? "";
    }

    private string GetMatchId() {
      return Context.GetHttpContext().Request.Query["matchId"];
    }
  }
}