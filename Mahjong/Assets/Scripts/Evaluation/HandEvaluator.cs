using System.Collections.Generic;
using System.Linq;
using MJ.Core.Tiles;
using MJ.Core.Hand;

namespace MJ.Evaluation
{
    /// <summary>
    /// Library of pattern-checking functions for evaluating Mahjong hands
    /// RuleSets use these functions to determine winning conditions
    /// </summary>
    public static class HandEvaluator
    {
        /// <summary>
        /// Checks if a hand matches the basic winning pattern: N melds + 1 pair
        /// Used by Hong Kong (4 melds), Taiwan (5 melds), and other variants
        /// </summary>
        /// <param name="hand">The hand to evaluate</param>
        /// <param name="meldsRequired">Number of melds required (e.g., 4 for HK, 5 for Taiwan)</param>
        /// <returns>True if the hand has the required melds and a pair</returns>
        public static bool CheckBasicWinPattern(Hand hand, int meldsRequired)
        {
            // Count existing exposed melds
            int exposedMeldCount = hand.GetExposedMelds().Count;
            
            // Get concealed tiles
            List<TileInstance> concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            
            // Calculate how many more melds we need from concealed tiles
            int concealedMeldsNeeded = meldsRequired - exposedMeldCount;
            
            // Try to find valid combinations
            return CanFormMeldsAndPair(concealedTiles, concealedMeldsNeeded, needsPair: true);
        }

        /// <summary>
        /// Checks if a hand with a newly drawn tile matches the basic win pattern
        /// Temporarily adds the tile to check, then removes it
        /// </summary>
        /// <param name="hand">The current hand</param>
        /// <param name="newTile">The newly drawn tile</param>
        /// <param name="meldsRequired">Number of melds required</param>
        /// <returns>True if adding the new tile creates a winning hand</returns>
        public static bool CheckBasicWinPatternWithTile(Hand hand, TileInstance newTile, int meldsRequired)
        {
            // Temporarily add the tile
            hand.AddTile(newTile);
            bool isWinning = CheckBasicWinPattern(hand, meldsRequired);
            
            // Remove the tile to restore original state
            hand.RemoveTile(newTile);
            
            return isWinning;
        }

        /// <summary>
        /// Checks if a hand can win by claiming a discarded tile
        /// </summary>
        /// <param name="hand">The current hand</param>
        /// <param name="discardedTile">The discarded tile to potentially claim</param>
        /// <param name="meldsRequired">Number of melds required</param>
        /// <returns>True if claiming the discarded tile creates a winning hand</returns>
        public static bool CanWinWithDiscard(Hand hand, TileData discardedTile, int meldsRequired)
        {
            // Create a temporary tile instance
            TileInstance tempTile = new TileInstance(discardedTile);
            return CheckBasicWinPatternWithTile(hand, tempTile, meldsRequired);
        }

        /// <summary>
        /// Checks if tiles can form Seven Pairs (7 pairs of identical tiles)
        /// Special winning hand in some variants
        /// </summary>
        /// <param name="hand">The hand to evaluate</param>
        /// <returns>True if the hand consists of exactly 7 pairs</returns>
        public static bool CheckSevenPairs(Hand hand)
        {
            // Seven Pairs must be fully concealed (no exposed melds)
            if (hand.GetExposedMelds().Count > 0)
            {
                return false;
            }

            List<TileInstance> concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            
            // Must have exactly 14 tiles
            if (concealedTiles.Count != 14)
            {
                return false;
            }

            // Group tiles by type
            var groups = concealedTiles
                .GroupBy(t => new { t.Data.Suit, t.Data.Number, t.Data.Wind, t.Data.Dragon })
                .Select(g => g.Count())
                .OrderBy(count => count)
                .ToList();

            // Should have exactly 7 groups of 2
            return groups.Count == 7 && groups.All(count => count == 2);
        }

        /// <summary>
        /// Checks if hand is Thirteen Orphans (1 of each terminal/honor + 1 pair)
        /// Special winning hand: 1,9 of each suit + all winds + all dragons + one pair
        /// </summary>
        /// <param name="hand">The hand to evaluate</param>
        /// <returns>True if the hand is Thirteen Orphans</returns>
        public static bool CheckThirteenOrphans(Hand hand)
        {
            // Thirteen Orphans must be fully concealed
            if (hand.GetExposedMelds().Count > 0)
            {
                return false;
            }

            List<TileInstance> concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            
            // Must have exactly 14 tiles
            if (concealedTiles.Count != 14)
            {
                return false;
            }

            // Required tiles: 1&9 of each suit (6 tiles) + 4 winds + 3 dragons = 13 unique types
            HashSet<string> requiredTypes = new HashSet<string>
            {
                "1 Bamboo", "9 Bamboo",
                "1 Characters", "9 Characters",
                "1 Dots", "9 Dots",
                "East Wind", "South Wind", "West Wind", "North Wind",
                "Red Dragon", "Green Dragon", "White Dragon"
            };

            // Count occurrences of each tile type
            Dictionary<string, int> tileCounts = new Dictionary<string, int>();
            foreach (var tile in concealedTiles)
            {
                string key = tile.Data.ToString();
                if (!tileCounts.ContainsKey(key))
                {
                    tileCounts[key] = 0;
                }
                tileCounts[key]++;
            }

            // Check if we have all 13 required types
            foreach (var required in requiredTypes)
            {
                if (!tileCounts.ContainsKey(required))
                {
                    return false;
                }
            }

            // Check that exactly one type has 2 tiles (the pair) and all others have 1
            int pairCount = 0;
            foreach (var kvp in tileCounts)
            {
                if (!requiredTypes.Contains(kvp.Key))
                {
                    return false; // Has a tile that's not a terminal or honor
                }

                if (kvp.Value == 2)
                {
                    pairCount++;
                }
                else if (kvp.Value != 1)
                {
                    return false; // Has more than 2 of something
                }
            }

            return pairCount == 1;
        }

        #region Private Helper Methods

        /// <summary>
        /// Recursively checks if tiles can form the required number of melds and a pair
        /// </summary>
        private static bool CanFormMeldsAndPair(List<TileInstance> tiles, int meldsNeeded, bool needsPair)
        {
            // Base case: all requirements met
            if (meldsNeeded == 0 && !needsPair)
            {
                return tiles.Count == 0;
            }

            // Calculate expected tiles remaining
            int tilesNeeded = (meldsNeeded * 3) + (needsPair ? 2 : 0);
            if (tiles.Count != tilesNeeded)
            {
                return false;
            }

            if (tiles.Count == 0)
            {
                return false;
            }

            // Try to form a pair first (if needed)
            if (needsPair)
            {
                TileInstance first = tiles[0];
                TileInstance second = tiles.FirstOrDefault(t => t != first && t.Data.IsSameType(first.Data));
                
                if (second != null)
                {
                    // Found a pair, remove them and continue
                    List<TileInstance> remaining = new List<TileInstance>(tiles);
                    remaining.Remove(first);
                    remaining.Remove(second);
                    
                    if (CanFormMeldsAndPair(remaining, meldsNeeded, needsPair: false))
                    {
                        return true;
                    }
                }
            }

            // Try to form a meld (Pong or Chow)
            if (meldsNeeded > 0)
            {
                TileInstance first = tiles[0];
                
                // Try Pong (3 identical tiles)
                List<TileInstance> pongTiles = tiles
                    .Where(t => t.Data.IsSameType(first.Data))
                    .Take(3)
                    .ToList();
                
                if (pongTiles.Count == 3)
                {
                    List<TileInstance> remaining = new List<TileInstance>(tiles);
                    foreach (var tile in pongTiles)
                    {
                        remaining.Remove(tile);
                    }
                    
                    if (CanFormMeldsAndPair(remaining, meldsNeeded - 1, needsPair))
                    {
                        return true;
                    }
                }

                // Try Chow (3 consecutive tiles of same suit) - only if not honor tile
                if (!first.Data.IsHonor())
                {
                    List<TileInstance> chowTiles = TryFormChow(tiles, first);
                    
                    if (chowTiles != null && chowTiles.Count == 3)
                    {
                        List<TileInstance> remaining = new List<TileInstance>(tiles);
                        foreach (var tile in chowTiles)
                        {
                            remaining.Remove(tile);
                        }
                        
                        if (CanFormMeldsAndPair(remaining, meldsNeeded - 1, needsPair))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to form a Chow starting with the given tile
        /// </summary>
        private static List<TileInstance> TryFormChow(List<TileInstance> tiles, TileInstance startTile)
        {
            if (startTile.Data.IsHonor())
            {
                return null;
            }

            TileSuit suit = startTile.Data.Suit;
            int startNumber = startTile.Data.Number;

            // Can't form sequences starting at 8 or 9
            if (startNumber > 7)
            {
                return null;
            }

            // Look for n, n+1, n+2
            TileInstance tile1 = startTile;
            TileInstance tile2 = tiles.FirstOrDefault(t => 
                t != tile1 && 
                t.Data.Suit == suit && 
                t.Data.Number == startNumber + 1);
            
            if (tile2 == null)
            {
                return null;
            }

            TileInstance tile3 = tiles.FirstOrDefault(t => 
                t != tile1 && 
                t != tile2 && 
                t.Data.Suit == suit && 
                t.Data.Number == startNumber + 2);

            if (tile3 == null)
            {
                return null;
            }

            return new List<TileInstance> { tile1, tile2, tile3 };
        }

        #endregion
    }
}
