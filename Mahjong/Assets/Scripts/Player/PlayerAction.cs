using MJ.Core;

namespace MJ.Player
{
    public enum ActionType
    {
        Draw,
        Discard,
        DeclareWin,   // win declaration hook (we'll wire real logic later)
    }

    /// <summary>
    /// Represents a single action taken by a player:
    /// - Draw:      no tile specified, Seat must equal CurrentSeat
    /// - Discard:   Tile must be one currently in that player's hand
    /// - DeclareWin: no tile; seat declares win with current hand
    /// </summary>
    public sealed class PlayerAction
    {
        public ActionType Type { get; }
        public int Seat { get; }
        public TileInstance Tile { get; }

        private PlayerAction(ActionType type, int seat, TileInstance tile = null)
        {
            Type = type;
            Seat = seat;
            Tile = tile;
        }

        public static PlayerAction Draw(int seat) =>
            new PlayerAction(ActionType.Draw, seat);

        public static PlayerAction Discard(int seat, TileInstance tile) =>
            new PlayerAction(ActionType.Discard, seat, tile);

        public static PlayerAction DeclareWin(int seat) =>
            new PlayerAction(ActionType.DeclareWin, seat);

        public override string ToString()
        {
            return Type switch
            {
                ActionType.Draw        => $"Seat {Seat} -> Draw",
                ActionType.Discard     => $"Seat {Seat} -> Discard {Tile?.Type} (Instance #{Tile?.InstanceId})",
                ActionType.DeclareWin  => $"Seat {Seat} -> Declare Win",
                _                      => $"Seat {Seat} -> {Type}"
            };
        }
    }
}
