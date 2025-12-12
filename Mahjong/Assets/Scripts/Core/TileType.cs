using System;

namespace MJ.Core
{
    public enum Suit {
        Dots,
        Bamboo,
        Characters,
        Winds,
        Dragons,
        // You can add Flowers/Seasons later
    }

    public enum Wind {
        East, South, West, North
    }

    public enum Dragon {
        Red, Green, White
    }

    public readonly struct TileType : IEquatable<TileType> {
        public Suit Suit { get; }
        public int Rank { get; }       // 1–9 for suits, 0 for honors
        public Wind? Wind { get; }
        public Dragon? Dragon { get; }

        public TileType(Suit suit, int rank, Wind? wind = null, Dragon? dragon = null) {
            Suit = suit;
            Rank = rank;
            Wind = wind;
            Dragon = dragon;
        }

        public static TileType Suited(Suit suit, int rank) =>
            new TileType(suit, rank);

        public static TileType WindTile(Wind wind) =>
            new TileType(Suit.Winds, 0, wind);

        public static TileType DragonTile(Dragon dragon) =>
            new TileType(Suit.Dragons, 0, null, dragon);

        // Equality
        public bool Equals(TileType other) =>
            Suit == other.Suit &&
            Rank == other.Rank &&
            Wind == other.Wind &&
            Dragon == other.Dragon;

        public override bool Equals(object obj) =>
            obj is TileType other && Equals(other);

        // public override int GetHashCode() =>
        //     HashCode.Combine(Suit, Rank, Wind, Dragon);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Suit;
                hash = hash * 31 + Rank;
                hash = hash * 31 + (Wind.HasValue ? ((int)Wind.Value + 1) : 0);
                hash = hash * 31 + (Dragon.HasValue ? ((int)Dragon.Value + 1) : 0);
                return hash;
            }
        }
        // Useful for logging/debugging
        public override string ToString() {
            return Suit switch {
                Suit.Winds    => $"{Wind} Wind",
                Suit.Dragons  => $"{Dragon} Dragon",
                _             => $"{Rank} {Suit}"
            };
        }
    }
}
