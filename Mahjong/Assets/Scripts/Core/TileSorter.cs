using System.Collections.Generic;

namespace MJ.Core
{
    public sealed class TileInstanceComparer : IComparer<TileInstance> {
        public static readonly TileInstanceComparer Instance = new TileInstanceComparer();

        private TileInstanceComparer() { }

        public int Compare(TileInstance x, TileInstance y) {
            var a = x.Type;
            var b = y.Type;

            // 1. Order suits in a custom way
            int suitOrderA = GetSuitOrder(a.Suit);
            int suitOrderB = GetSuitOrder(b.Suit);
            int cmp = suitOrderA.CompareTo(suitOrderB);
            if (cmp != 0) return cmp;

            // 2. Within suits, order by rank (1–9). Honors typically have rank 0 so they naturally come first or last; adjust if needed.
            cmp = a.Rank.CompareTo(b.Rank);
            if (cmp != 0) return cmp;

            // 3. For winds/dragons, further differentiate by Wind/Dragon enum
            if (a.Wind.HasValue && b.Wind.HasValue) {
                cmp = a.Wind.Value.CompareTo(b.Wind.Value);
                if (cmp != 0) return cmp;
            }

            if (a.Dragon.HasValue && b.Dragon.HasValue) {
                cmp = a.Dragon.Value.CompareTo(b.Dragon.Value);
                if (cmp != 0) return cmp;
            }

            // 4. As a final fallback, compare InstanceId so there's a consistent order
            return x.InstanceId.CompareTo(y.InstanceId);
        }

        private int GetSuitOrder(Suit suit) => suit switch {
            Suit.Dots       => 2,
            Suit.Bamboo     => 1,
            Suit.Characters => 0,
            Suit.Winds      => 3,
            Suit.Dragons    => 4,
            _               => 5
        };
    }
}
