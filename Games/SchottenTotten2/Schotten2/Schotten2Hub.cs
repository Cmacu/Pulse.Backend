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
    Task PlayCard(SectionCard sectionCard);
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
      var game = _service.Load(matchId);
      // Groups.AddToGroupAsync(Context.ConnectionId, matchId); // Handle spectators
      Clients.User(playerId).UpdateState(_service.MapResponse(game, playerId));
      return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception) {
      var playerId = GetPlayerId();
      // Console.WriteLine($"Player disconnected: {playerId}");
      return base.OnDisconnectedAsync(exception);
    }

    public void CheckState(int siegeCardsCount) {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      var game = _service.Load(matchId);
      if (game.State.SiegeCards.Count != siegeCardsCount) {
        Clients.User(playerId).UpdateState(_service.MapResponse(game, playerId));
      }
    }

    public void PlayCard(int sectionIndex, int handIndex) {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      var game = _service.PlayCard(matchId, playerId, sectionIndex, handIndex);
      SendState(game);
      game = _service.CompleteTurn(matchId, playerId, sectionIndex, handIndex);
      SendState(game);
    }

    public void Resign() {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      Console.WriteLine($"Resign: {playerId}");
      var game = _service.Exit(matchId, playerId, Game.GameEvent.Resigned);
      SendState(game);
    }

    public void Retreat(int sectionIndex) {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      var game = _service.Retreat(matchId, playerId, sectionIndex);
      SendState(game);
    }

    public void UseOil(int sectionIndex) {
      var playerId = GetPlayerId();
      var matchId = GetMatchId();
      var game = _service.UseOil(matchId, playerId, sectionIndex);
      SendState(game);
    }

    private void SendState(Schotten2Game game) {
      Clients.User(game.AttackerId).UpdateState(_service.MapResponse(game, game.AttackerId));
      Clients.User(game.DefenderId).UpdateState(_service.MapResponse(game, game.DefenderId));
    }

    private void SendPlayCard(SectionCard sectionCard, string opponentId) {
      Clients.User(opponentId).PlayCard(sectionCard);
    }

    private string GetPlayerId() {
      return Context.UserIdentifier ?? "";
    }

    private string GetMatchId() {
      return Context.GetHttpContext().Request.Query["matchId"];
    }
  }
}