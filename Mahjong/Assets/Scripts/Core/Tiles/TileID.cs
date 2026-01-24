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

    public class TileID : IEquatable<TileID>, IComparable<TileID>
    {
        public Suit Suit;
        public int Rank;
        public Winds Wind;
        public Dragons Dragon;

        public TileID(Suit suit, int rank) {
            Suit = suit;
            Rank = rank;
        }

        public TileID(Winds wind) {
            Suit = Suit.Winds;
            Wind = wind;
        }

        public TileID(Dragons dragon) {
            Suit = Suit.Dragons;
            Dragon = dragon;
        }

        public bool Equals(TileID other) {
            if (Suit != other.Suit) return false;

            return Suit switch {
                Suit.Characters or Suit.Bamboo or Suit.Dots =>
                    Suit == other.Suit && Rank == other.Rank,
                Suit.Winds => Wind == other.Wind,
                Suit.Dragons => Dragon == other.Dragon,
                _ => false
            };
        }

        public override bool Equals(object obj) {
            return obj is TileID other && Equals(other);
        }

        public override int GetHashCode() {
            return Suit switch {
                Suit.Characters or Suit.Bamboo or Suit.Dots =>
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
                Suit.Characters or Suit.Bamboo or Suit.Dots =>
                    Suit != other.Suit ? Suit.CompareTo(other.Suit) : Rank.CompareTo(other.Rank),
                Suit.Winds => Wind.CompareTo(other.Wind),
                Suit.Dragons => Dragon.CompareTo(other.Dragon),
                _ => 99
            };
        }

        public override string ToString() {
            return Suit switch {
                Suit.Characters or Suit.Bamboo or Suit.Dots =>
                    $"{Rank} {Suit}",
                Suit.Winds => $"{Wind} Wind",
                Suit.Dragons => $"{Dragon} Dragon",
                _ => "Unknown Tile"
            };
        }
    }
}
