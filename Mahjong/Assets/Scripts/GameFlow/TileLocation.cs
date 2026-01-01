using MJ.Core.Tiles;

namespace MJ.GameFlow
{
    /// <summary>
    /// Information about where a tile is currently located
    /// Stored directly on TileInstance for instant lookup
    /// </summary>
    public struct TileLocationInfo
    {
        public LocationType Type { get; set; }
        public int PlayerIndex { get; set; }  // -1 for non-player locations
        public int MeldIndex { get; set; }    // -1 if not in meld

        public TileLocationInfo(LocationType type, int playerIndex = -1, int meldIndex = -1)
        {
            Type = type;
            PlayerIndex = playerIndex;
            MeldIndex = meldIndex;
        }

        public override string ToString()
        {
            return Type switch
            {
                LocationType.PlayerHand => $"Player {PlayerIndex} - Hand (Concealed)",
                LocationType.PlayerBonus => $"Player {PlayerIndex} - Bonus",
                LocationType.PlayerMeld => $"Player {PlayerIndex} - Meld #{MeldIndex}",
                LocationType.Wall => "Wall (Draw Pile)",
                LocationType.DeadWall => "Dead Wall",
                LocationType.DiscardPile => "Discard Pile",
                _ => "Unknown"
            };
        }
    }

    /// <summary>
    /// Helper class for displaying tile locations in UI
    /// Wraps TileInstance with formatted display string
    /// </summary>
    public class TileLocation
    {
        public TileInstance Tile { get; set; }
        public LocationType Type => Tile.Location.Type;
        public int PlayerIndex => Tile.Location.PlayerIndex;
        public int MeldIndex => Tile.Location.MeldIndex;
        public string DisplayString { get; private set; }

        public TileLocation(TileInstance tile)
        {
            Tile = tile;
            DisplayString = GenerateDisplayString();
        }

        private string GenerateDisplayString()
        {
            string location = Tile.Location.ToString();
            return $"{Tile.Data.ToCompactString()} [ID:{Tile.TileId}] - {location}";
        }

        public override string ToString() => DisplayString;
    }

    /// <summary>
    /// Types of locations where tiles can be
    /// </summary>
    public enum LocationType
    {
        Unknown,        // Initial state or error
        PlayerHand,     // In player's concealed hand
        PlayerMeld,     // In player's exposed meld (Pong/Kong/Chow)
        PlayerBonus,    // In player's bonus tiles
        Wall,           // In the main draw pile
        DeadWall,       // In the dead wall
        DiscardPile     // In the discard pile
    }
}
