using System;

namespace MJ.Core.Tiles
{
    /// <summary>
    /// Suit types for Mahjong tiles
    /// </summary>
    public enum TileSuit
    {
        Bamboo,      // 索子 (1-9)
        Characters,  // 萬子 (1-9)
        Dots,        // 筒子 (1-9)
        Wind,        // 風牌 (East, South, West, North)
        Dragon,      // 三元牌 (Red, Green, White)
        Flower,      // 花牌 (bonus tiles)
        Season       // 季牌 (bonus tiles)
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
    public readonly struct TileData : IEquatable<TileData>
    {
        public TileSuit Suit { get; }
        public int Number { get; }  // 1-9 for suited tiles, 1-4 for Flowers/Seasons
        public WindType? Wind { get; }
        public DragonType? Dragon { get; }
        public int TileId { get; }  // Unique ID for each physical tile

        #region Constructors

        /// <summary>
        /// Constructor for suited tiles (Bamboo, Characters, Dots)
        /// </summary>
        public TileData(TileSuit suit, int number, int tileId)
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
            TileId = tileId;
        }

        /// <summary>
        /// Constructor for Wind tiles
        /// </summary>
        public TileData(WindType wind, int tileId)
        {
            Suit = TileSuit.Wind;
            Number = 0;
            Wind = wind;
            Dragon = null;
            TileId = tileId;
        }

        /// <summary>
        /// Constructor for Dragon tiles
        /// </summary>
        public TileData(DragonType dragon, int tileId)
        {
            Suit = TileSuit.Dragon;
            Number = 0;
            Wind = null;
            Dragon = dragon;
            TileId = tileId;
        }

        #endregion

        #region Classification Methods

        /// <summary>
        /// Checks if this is an honor tile (Wind or Dragon)
        /// </summary>
        public bool IsHonor()
        {
            return Suit == TileSuit.Wind || Suit == TileSuit.Dragon;
        }

        /// <summary>
        /// Checks if this is a terminal tile (1 or 9 of a suit)
        /// </summary>
        public bool IsTerminal()
        {
            return !IsHonor() && !IsBonus() && (Number == 1 || Number == 9);
        }

        /// <summary>
        /// Checks if this is a simple tile (2-8 of a suit)
        /// </summary>
        public bool IsSimple()
        {
            return !IsHonor() && !IsBonus() && Number >= 2 && Number <= 8;
        }

        /// <summary>
        /// Checks if this is a bonus tile (Flower or Season)
        /// </summary>
        public bool IsBonus()
        {
            return Suit == TileSuit.Flower || Suit == TileSuit.Season;
        }

        #endregion

        #region Comparison Methods

        /// <summary>
        /// Checks if two tiles are the same type (ignoring TileId)
        /// Used for matching tiles in melds
        /// </summary>
        public bool IsSameType(TileData other)
        {
            if (Suit != other.Suit) return false;

            if (Suit == TileSuit.Wind) return Wind == other.Wind;
            if (Suit == TileSuit.Dragon) return Dragon == other.Dragon;
            
            return Number == other.Number;
        }

        /// <summary>
        /// IEquatable implementation - checks if tiles are identical (including TileId)
        /// </summary>
        public bool Equals(TileData other)
        {
            return TileId == other.TileId;
        }

        public override bool Equals(object obj)
        {
            return obj is TileData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return TileId.GetHashCode();
        }

        public static bool operator ==(TileData left, TileData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TileData left, TileData right)
        {
            return !left.Equals(right);
        }

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
                return $"Flower {Number}";
            }
            
            if (Suit == TileSuit.Season)
            {
                return $"Season {Number}";
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
                return $"F{Number}";
            }
            
            if (Suit == TileSuit.Season)
            {
                return $"S{Number}";
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
