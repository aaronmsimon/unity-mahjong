using UnityEngine;

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
        
        /// <summary>
        /// Constructor
        /// </summary>
        public TileInstance(TileData data)
        {
            Data = data;
            IsConcealed = true;  // Tiles start concealed by default
            TileObject = null;
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
