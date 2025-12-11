using System.Collections.Generic;
using System;
using System.Linq;
using MJ.Core;
using MJ.Player;

namespace MJ.Rules
{
    public static class HandEvaluator
    {
        /// <summary>
        /// Returns true if the player's concealed hand is a standard winning hand,
        /// using only the "4 melds + 1 pair" pattern.
        /// Ignores special hands (Seven Pairs, Thirteen Orphans, etc.) for now.
        /// </summary>
        public static bool IsWinningHand(PlayerState player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            return IsWinningHand(player.Hand.Select(t => t.Type));
        }

        /// <summary>
        /// Core winning hand check operating on TileType sequence.
        /// </summary>
        public static bool IsWinningHand(IEnumerable<TileType> tiles)
        {
            var tileList = tiles.ToList();
            int count = tileList.Count;

            // Standard mahjong hand sizes are 14 (or 2 mod 3 if you support weird things),
            // but we'll enforce 2 mod 3 and >= 2 tiles.
            if (count < 2 || count % 3 != 2)
                return false;

            // Count tiles by type.
            var counts = new Dictionary<TileType, int>();
            foreach (var t in tileList)
            {
                counts.TryGetValue(t, out int c);
                counts[t] = c + 1;
            }

            // Try every possible pair.
            foreach (var kvp in counts.ToList())
            {
                var tileType = kvp.Key;
                int c = kvp.Value;
                if (c < 2)
                    continue;

                // Use this tile as the pair.
                counts[tileType] -= 2;
                if (CanFormMelds(counts))
                    return true;

                // Backtrack.
                counts[tileType] += 2;
            }

            return false;
        }

        /// <summary>
        /// Returns true if all tiles represented in the counts dictionary
        /// can be partitioned into melds (pungs/chows), with no leftover tiles.
        /// </summary>
        private static bool CanFormMelds(Dictionary<TileType, int> counts)
        {
            // Find the first tile that still has a non-zero count.
            TileType? firstTypeNullable = null;
            foreach (var kvp in counts)
            {
                if (kvp.Value > 0)
                {
                    firstTypeNullable = kvp.Key;
                    break;
                }
            }

            // No tiles left: success.
            if (firstTypeNullable == null)
                return true;

            var firstType = firstTypeNullable.Value;

            // Try a pung (three of the same).
            if (counts[firstType] >= 3)
            {
                counts[firstType] -= 3;
                if (CanFormMelds(counts))
                    return true;
                counts[firstType] += 3;
            }

            // Try a chow (sequence of 3 in the same suit),
            // only for suited tiles (Dots, Bamboo, Characters).
            if (IsSuited(firstType.Suit))
            {
                int rank = firstType.Rank;
                var suit = firstType.Suit;

                // Need rank, rank+1, rank+2 in same suit.
                if (rank <= 7) // 7,8,9 is the last possible chow
                {
                    var t1 = TileType.Suited(suit, rank);
                    var t2 = TileType.Suited(suit, rank + 1);
                    var t3 = TileType.Suited(suit, rank + 2);

                    if (counts.TryGetValue(t1, out int c1) && c1 > 0 &&
                        counts.TryGetValue(t2, out int c2) && c2 > 0 &&
                        counts.TryGetValue(t3, out int c3) && c3 > 0)
                    {
                        counts[t1]--;
                        counts[t2]--;
                        counts[t3]--;

                        if (CanFormMelds(counts))
                            return true;

                        counts[t1]++;
                        counts[t2]++;
                        counts[t3]++;
                    }
                }
            }

            // Neither pung nor chow worked from this tile type.
            return false;
        }

        private static bool IsSuited(Suit suit)
        {
            return suit == Suit.Dots || suit == Suit.Bamboo || suit == Suit.Characters;
        }
    }
}
