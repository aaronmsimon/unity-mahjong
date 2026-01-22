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

    public class TileID : IEquatable<TileID>
    {
        public Suit Suit;
        public int Rank;
        public Winds Wind;
        public Dragons Dragon;

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

        public override bool Equals(object obj)
        {
            return obj is TileID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Suit switch {
                Suit.Characters or Suit.Bamboo or Suit.Dots =>
                    HashCode.Combine(Suit, Rank),
                Suit.Winds => HashCode.Combine(Suit, Wind),
                Suit.Dragons => HashCode.Combine(Suit, Dragon),
                _ => Suit.GetHashCode()
            };
        }
    }
}
