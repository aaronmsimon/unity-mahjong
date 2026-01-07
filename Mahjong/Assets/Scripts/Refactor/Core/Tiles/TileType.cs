using System;

namespace MJ2.Core.Tiles
{
    /// <summary>
    /// Suit types for Mahjong tiles
    /// </summary>
    public enum TileSuit
    {
        Characters,
        Bamboo,
        Dots,
        Wind,
        Dragon,
        Flower,
        Season,
        Jokers
    }

    /// <summary>
    /// Wind tile types
    /// </summary>
    public enum WindType
    {
        East,
        South,
        West,
        North
    }

    /// <summary>
    /// Dragon tile types
    /// </summary>
    public enum DragonType
    {
        Red,
        Green,
        White
    }

    /// <summary>
    /// Immutable struct representing a Mahjong tile's identity
    /// Each physical tile in the game has unique data
    /// </summary>
    public readonly struct TileType : IEquatable<TileType>
    {
        public TileSuit Suit { get; }
        public TileValue Value { get; }

        #region Constructors

        /// <summary>
        /// Constructor for suited tiles (Bamboo, Characters, Dots, Flower, Season)
        /// </summary>
        public TileType(TileSuit suit, int number)
        {
            if (suit != TileSuit.Bamboo && suit != TileSuit.Characters && suit != TileSuit.Dots &&
                suit != TileSuit.Flower && suit != TileSuit.Season)
            {
                throw new ArgumentException($"Invalid suit for numbered tile: {suit}");
            }

            if (number < 1 || number > 9)
            {
                throw new ArgumentException($"Number must be between 1-9, got {number}");
            }

            Suit = suit;
            Value = new NumericTileValue(number);
        }

        /// <summary>
        /// Constructor for Wind tiles
        /// </summary>
        public TileType(WindType wind)
        {
            Suit = TileSuit.Wind;
            Value = new WindTileValue(wind);
        }

        /// <summary>
        /// Constructor for Dragon tiles
        /// </summary>
        public TileType(DragonType dragon)
        {
            Suit = TileSuit.Dragon;
            Value = new DragonTileValue(dragon);
        }

        #endregion

        #region Convenience Properties

        /// <summary>
        /// Gets numeric value if this is a numbered tile, otherwise -1
        /// </summary>
        public int Number => Value?.NumericValue ?? -1;

        /// <summary>
        /// Gets wind type if this is a wind tile, otherwise null
        /// </summary>
        public WindType? Wind => Value is WindTileValue wind ? wind.Wind : null;

        /// <summary>
        /// Gets dragon type if this is a dragon tile, otherwise null
        /// </summary>
        public DragonType? Dragon => Value is DragonTileValue dragon ? dragon.Dragon : null;

        #endregion

        #region Classification Methods

        /// <summary>
        /// Checks if this is an honor tile (Wind or Dragon)
        /// </summary>
        public bool IsHonor() => Suit == TileSuit.Wind || Suit == TileSuit.Dragon;

        /// <summary>
        /// Checks if this is a terminal tile (1 or 9 of a suit)
        /// </summary>
        public bool IsTerminal() => !IsHonor() && !IsBonus() && (Number == 1 || Number == 9);

        /// <summary>
        /// Checks if this is a simple tile (2-8 of a suit)
        /// </summary>
        public bool IsSimple() => !IsHonor() && !IsBonus() && !IsTerminal();

        /// <summary>
        /// Checks if this is a bonus tile (Flower or Season)
        /// </summary>
        public bool IsBonus() => Suit == TileSuit.Flower || Suit == TileSuit.Season;

        #endregion

        #region Comparison Methods

        public override bool Equals(object obj) => obj is TileType && Equals((TileType)obj);

        public bool Equals(TileType other) =>
            Suit == other.Suit &&
            Value == other.Value;

        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(TileType left, TileType right) => left.Equals(right);

        public static bool operator !=(TileType left, TileType right) => !left.Equals(right);

        #endregion

        #region String Representation

        /// <summary>
        /// Returns a human-readable string representation of the tile
        /// </summary>
        public override string ToString()
        {            
            return $"{Value} {Suit}";
        }

        /// <summary>
        /// Returns a compact string representation (useful for debugging)
        /// Examples: "1B", "5C", "7D", "EW", "RD"
        /// </summary>
        public string ToCompactString()
        {
            if (IsHonor()) return Value.ToCompactString();

            return $"{Value.ToCompactString()}{Suit.ToString()[0]}";
        }

        #endregion
    }
}
