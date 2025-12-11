using System.Collections.Generic;
using System.Linq;
using MJ.Core;

namespace MJ.Player
{
    public sealed class PlayerState {
        /// <summary>
        /// 0–3 for a 4-player table. You can map 0 = East, 1 = South, etc.
        /// </summary>
        public int SeatIndex { get; }

        /// <summary>
        /// Concealed tiles currently in hand.
        /// </summary>
        public List<TileInstance> Hand { get; } = new();

        /// <summary>
        /// Exposed melds (chi/pung/kong) will go here later.
        /// </summary>
        public List<Meld> Melds { get; } = new();

        public bool HasWon { get; set; }

        public PlayerState(int seatIndex) {
            SeatIndex = seatIndex;
        }

        // -----------------------
        // Hand manipulation
        // -----------------------

        /// <summary>
        /// Add a tile to the hand (e.g., drawing from the wall or dealing).
        /// </summary>
        public void AddToHand(TileInstance tile) {
            Hand.Add(tile);
        }

        /// <summary>
        /// Remove a specific tile instance from the hand.
        /// Returns true if removed; false if it wasn't found.
        /// </summary>
        public bool RemoveFromHand(TileInstance tile) {
            return Hand.Remove(tile);
        }

        /// <summary>
        /// Convenience: remove the first tile in hand that matches a TileType.
        /// Useful if you're discarding by type instead of specific instance.
        /// </summary>
        public TileInstance RemoveFirstOfType(TileType type) {
            var tile = Hand.FirstOrDefault(t => t.Type.Equals(type));
            if (tile != null) {
                Hand.Remove(tile);
            }
            return tile;
        }

        /// <summary>
        /// Sort the hand in a consistent order: suit, then rank, with honors at the end.
        /// This is purely visual/quality-of-life.
        /// </summary>
        public void SortHand() {
            Hand.Sort(TileInstanceComparer.Instance);
        }

        public override string ToString() {
            return $"Seat {SeatIndex}: {string.Join(", ", Hand.Select(h => h.Type.ToString()))}";
        }
    }
}
