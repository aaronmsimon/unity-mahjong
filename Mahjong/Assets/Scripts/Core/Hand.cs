using System.Collections.Generic;
using System.Linq;

namespace MJ.Core
{
    public sealed class Hand {
        public List<TileInstance> Concealed { get; } = new();
        public List<Meld> Melds { get; } = new();

        public void AddTile(TileInstance tile) {
            Concealed.Add(tile);
        }

        public bool RemoveTile(TileInstance tile) {
            return Concealed.Remove(tile);
        }

        public void SortHand() {
            Concealed.Sort(TileInstanceComparer.Instance);
        }

        public override string ToString() {
            return $"Hand: Concealed [{Concealed.Select(c => c.Type.ToString())}], Melds [{Melds.Select(m => m.Type.ToString())}]";
        }
    }
}
