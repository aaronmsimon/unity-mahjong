using System;

namespace MJ2.Core.Tiles
{
    /// <summary>
    /// Suit types for Mahjong tiles
    /// </summary>
    public enum TileSuit
    {
        Characters,  // 萬子 (1-9)
        Bamboo,      // 索子 (1-9)
        Dots,        // 筒子 (1-9)
        Wind,        // 風牌 (East, South, West, North)
        Dragon,      // 三元牌 (Red, Green, White)
        Flower,      // 花牌 (bonus tiles)
        Season,       // 季牌 (bonus tiles)
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
        Red,    // 紅中
        Green,  // 發財
        White   // 白板
    }

    /// <summary>
    /// Immutable struct representing a Mahjong tile's identity
    /// Each physical tile in the game has unique data
    /// </summary>
    public readonly struct TileType : IEquatable<TileType>
    {
        public TileSuit Suit { get; }
        public int Number { get; }  // 1-9 for suited tiles, 1-4 for Flowers/Seasons
        public WindType? Wind { get; }
        public DragonType? Dragon { get; }

        #region Constructors

        /// <summary>
        /// Constructor for suited tiles (Bamboo, Characters, Dots)
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
            Number = number;
            Wind = null;
            Dragon = null;
        }

        /// <summary>
        /// Constructor for Wind tiles
        /// </summary>
        public TileType(WindType wind)
        {
            Suit = TileSuit.Wind;
            Number = 0;
            Wind = wind;
            Dragon = null;
        }

        /// <summary>
        /// Constructor for Dragon tiles
        /// </summary>
        public TileType(DragonType dragon)
        {
            Suit = TileSuit.Dragon;
            Number = 0;
            Wind = null;
            Dragon = dragon;
        }

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
            Number == other.Number &&
            Wind == other.Wind &&
            Dragon == other.Dragon;

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
            if (Suit == TileSuit.Wind)
            {
                return $"{Wind} Wind";
            }
            
            if (Suit == TileSuit.Dragon)
            {
                return $"{Dragon} Dragon";
            }
            
            if (Suit == TileSuit.Flower)
            {
                return $"{Number} Flower";
            }
            
            if (Suit == TileSuit.Season)
            {
                return $"{Number} Season";
            }
            
            return $"{Number} {Suit}";
        }

        /// <summary>
        /// Returns a compact string representation (useful for debugging)
        /// Examples: "1B", "5C", "7D", "EW", "RD"
        /// </summary>
        public string ToCompactString()
        {
            if (Suit == TileSuit.Wind)
            {
                return Wind switch
                {
                    WindType.East => "EW",
                    WindType.South => "SW",
                    WindType.West => "WW",
                    WindType.North => "NW",
                    _ => "?W"
                };
            }
            
            if (Suit == TileSuit.Dragon)
            {
                return Dragon switch
                {
                    DragonType.Red => "RD",
                    DragonType.Green => "GD",
                    DragonType.White => "WD",
                    _ => "?D"
                };
            }
            
            if (Suit == TileSuit.Flower)
            {
                return $"{Number}F";
            }
            
            if (Suit == TileSuit.Season)
            {
                return $"{Number}S";
            }
            
            char suitChar = Suit switch
            {
                TileSuit.Bamboo => 'B',
                TileSuit.Characters => 'C',
                TileSuit.Dots => 'D',
                _ => '?'
            };
            
            return $"{Number}{suitChar}";
        }

        #endregion
    }
}
