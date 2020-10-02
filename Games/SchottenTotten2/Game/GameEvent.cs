using System.ComponentModel;

namespace Pulse.Games.SchottenTotten2.Game {
  public enum GameEvent {
    [Description("started")]
    Start,

    [Description("retreated")]
    Retreat,

    [Description("used oil")]
    UseOil,

    [Description("eleminated")]
    Eliminate,

    [Description("played card")]
    PlayCard,

    [Description("completed their turn")]
    DrawCard,

    [Description("damaged")]
    Damaged,

    [Description("destroyed")]
    Destroyed,

    [Description("defended")]
    Defended,

    [Description("expired")]
    Expired,

    [Description("resigned")]
    Resigned,

  }
}