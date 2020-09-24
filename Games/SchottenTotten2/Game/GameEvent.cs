using System.ComponentModel;

namespace Pulse.Games.SchottenTotten2.Game {
  public enum GameEvent {
    [Description("started")]
    Start,

    [Description("retreated")]
    Retreat,

    [Description("used oil")]
    UseOil,

    [Description("send troops")]
    PlayCard,

    [Description("expired")]
    Expired,

    [Description("resigned")]
    Resigned
  }
}