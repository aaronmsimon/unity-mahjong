using System;

namespace MJ.Core.Tiles
{
    public enum Suit {
        Characters,
        Bamboo,
        Dots,
        Winds,
        Dragons,
        Flowers,
        Seasons,
        Jokers,
        Blanks
    }

    public enum Winds {
        East,
        South,
        West,
        North
    }

    public enum Dragons {
        Red,
        Green,
        White
    }

    [Serializable]
    public struct TileID : IEquatable<TileID>, IComparable<TileID>
    {
        public Suit Suit;
        public int Rank;
        public Winds Wind;
        public Dragons Dragon;

        public bool IsSuited => Suit is Suit.Characters or Suit.Bamboo or Suit.Dots;

        public bool IsHonor => Suit is Suit.Winds or Suit.Dragons;

        public bool IsBonus => Suit is Suit.Flowers or Suit.Seasons;

        public bool Equals(TileID other) {
            if (Suit != other.Suit) return false;

            return Suit switch {
                Suit.Characters or Suit.Bamboo or Suit.Dots or Suit.Flowers or Suit.Seasons =>
                    Suit == other.Suit && Rank == other.Rank,
                Suit.Winds => Wind == other.Wind,
                Suit.Dragons => Dragon == other.Dragon,
                _ => false
            };
        }

        public override bool Equals(object obj) {
            return obj is TileID other && Equals(other);
        }

        public static bool operator ==(TileID left, TileID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TileID left, TileID right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode() {
            return Suit switch {
                Suit.Characters or Suit.Bamboo or Suit.Dots or Suit.Flowers or Suit.Seasons =>
                    HashCode.Combine(Suit, Rank),
                Suit.Winds => HashCode.Combine(Suit, Wind),
                Suit.Dragons => HashCode.Combine(Suit, Dragon),
                _ => Suit.GetHashCode()
            };
        }

        public int CompareTo(TileID other)
        {
            int suitCompare = Suit.CompareTo(other.Suit);
            if (suitCompare != 0) return suitCompare;

            return Suit switch {
                Suit.Characters or Suit.Bamboo or Suit.Dots or Suit.Flowers or Suit.Seasons =>
                    Rank.CompareTo(other.Rank),
                Suit.Winds => Wind.CompareTo(other.Wind),
                Suit.Dragons => Dragon.CompareTo(other.Dragon),
                _ => 99
            };
        }

        public override string ToString() {
            return Suit switch {
                Suit.Characters or Suit.Bamboo or Suit.Dots or Suit.Flowers or Suit.Seasons =>
                    $"{Rank} {Suit}",
                Suit.Winds => $"{Wind} Wind",
                Suit.Dragons => $"{Dragon} Dragon",
                _ => "Unknown Tile"
            };
        }
    }
}
