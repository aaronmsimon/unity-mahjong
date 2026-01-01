using UnityEngine;
using MJ.GameFlow;

namespace MJ.Core.Tiles
{
    /// <summary>
    /// Represents a physical tile instance in the game
    /// Wraps immutable TileData with mutable game state
    /// </summary>
    public class TileInstance
    {
        // Immutable tile identity
        public TileData Data { get; private set; }
        
        // Mutable game state
        public bool IsConcealed { get; set; }
        public GameObject TileObject { get; set; }

        // Location tracking for debug, undo/redo, and replay
        public TileLocationInfo Location { get; private set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public TileInstance(TileData data)
        {
            Data = data;
            IsConcealed = true;  // Tiles start concealed by default
            TileObject = null;
            Location = new TileLocationInfo(LocationType.Unknown);
        }

        /// <summary>
        /// Sets the current location of this tile
        /// Internal - only specific systems should update location
        /// </summary>
        internal void SetLocation(LocationType type, int playerIndex = -1, int meldIndex = -1)
        {
            Location = new TileLocationInfo(type, playerIndex, meldIndex);
        }

        /// <summary>
        /// Convenience property to access TileId
        /// </summary>
        public int TileId => Data.TileId;

        /// <summary>
        /// Returns a string representation of this tile instance
        /// </summary>
        public override string ToString()
        {
            string concealedStatus = IsConcealed ? "[Concealed]" : "[Exposed]";
            return $"{Data.ToString()} {concealedStatus}";
        }

        /// <summary>
        /// Returns a compact string representation
        /// </summary>
        public string ToCompactString()
        {
            return Data.ToCompactString();
        }
    }
}
