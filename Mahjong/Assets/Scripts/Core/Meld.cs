using System.Collections.Generic;

namespace MJ.Core
{
    public enum MeldType {
        Chow,
        Pung,
        Kong,
        Pair
    }

    public sealed class Meld {
        public MeldType Type { get; }
        public TileType BaseTile { get; }   // e.g., 3 Bamboo for 3-4-5 chow or 3-3-3 pung
        public bool IsConcealed { get; }

        public IReadOnlyList<TileInstance> Tiles { get; }

        public Meld(MeldType type, TileType baseTile, bool isConcealed, IList<TileInstance> tiles) {
            Type = type;
            BaseTile = baseTile;
            IsConcealed = isConcealed;
            Tiles = new List<TileInstance>(tiles);
        }

        public override string ToString() => $"{Type} of {BaseTile} ({(IsConcealed ? "concealed" : "open")})";
    }
}
