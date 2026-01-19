using System;

namespace MJ.Core.Tiles
{
    /// <summary>
    /// Represents the value of a tile - can be numeric, wind, or dragon
    /// Polymorphic value that knows how to compare itself
    /// </summary>
    public abstract class TileValue : IEquatable<TileValue>
    {
        /// <summary>
        /// Gets the numeric value if this is a numeric tile (1-9), otherwise -1
        /// </summary>
        public abstract int NumericValue { get; }

        /// <summary>
        /// Checks if this value can be used in sequential melds (Chows)
        /// Only numeric tiles can form sequences
        /// </summary>
        public abstract bool CanFormSequence { get; }

        /// <summary>
        /// Checks if two values are the same (for Pongs/Kongs)
        /// </summary>
        public abstract bool Equals(TileValue other);

        /// <summary>
        /// Returns true if this value is exactly one greater than the other (for Chow validation)
        /// </summary>
        public abstract bool IsOneGreaterThan(TileValue other);

        public abstract override string ToString();
        public abstract string ToCompactString();
        public abstract override int GetHashCode();
    }
}
