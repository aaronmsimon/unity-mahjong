using System.Collections.Generic;
using System.Linq;
using MJ.Core.Tiles;

namespace MJ.Evaluation
{
    /// <summary>
    /// Validates whether tiles can form valid melds (Pong, Kong, Chow)
    /// Pure validation - doesn't create objects or modify state
    /// </summary>
    public static class MeldValidator
    {
        /// <summary>
        /// Checks if tiles can form a valid Pong (3 identical tiles)
        /// </summary>
        /// <param name="tiles">The tiles to validate</param>
        /// <returns>True if tiles form a valid Pong</returns>
        public static bool CanFormPong(List<TileInstance> tiles)
        {
            if (tiles == null || tiles.Count != 3)
            {
                return false;
            }

            // All three tiles must be the same type
            TileData first = tiles[0].Data;
            return tiles.All(t => t.Data.IsSameType(first));
        }

        /// <summary>
        /// Checks if tiles can form a valid Kong (4 identical tiles)
        /// </summary>
        /// <param name="tiles">The tiles to validate</param>
        /// <returns>True if tiles form a valid Kong</returns>
        public static bool CanFormKong(List<TileInstance> tiles)
        {
            if (tiles == null || tiles.Count != 4)
            {
                return false;
            }

            // All four tiles must be the same type
            TileData first = tiles[0].Data;
            return tiles.All(t => t.Data.IsSameType(first));
        }

        /// <summary>
        /// Checks if tiles can form a valid Chow (3 consecutive tiles of same suit)
        /// </summary>
        /// <param name="tiles">The tiles to validate (order doesn't matter)</param>
        /// <returns>True if tiles form a valid Chow</returns>
        public static bool CanFormChow(List<TileInstance> tiles)
        {
            if (tiles == null || tiles.Count != 3)
            {
                return false;
            }

            // Can't form Chow with honor tiles
            if (tiles.Any(t => t.Data.IsHonor()))
            {
                return false;
            }

            // All tiles must be same suit
            TileSuit suit = tiles[0].Data.Suit;
            if (!tiles.All(t => t.Data.Suit == suit))
            {
                return false;
            }

            // Get numbers and sort them
            List<int> numbers = tiles.Select(t => t.Data.Number).OrderBy(n => n).ToList();

            // Check if they form a consecutive sequence
            return numbers[1] == numbers[0] + 1 && numbers[2] == numbers[1] + 1;
        }

        /// <summary>
        /// Checks if a player's tiles can form a Pong with a discarded tile
        /// </summary>
        /// <param name="playerTiles">Tiles in the player's hand</param>
        /// <param name="discardedTile">The discarded tile</param>
        /// <returns>True if player has 2 matching tiles to form Pong</returns>
        public static bool CanClaimForPong(List<TileInstance> playerTiles, TileData discardedTile)
        {
            if (playerTiles == null)
            {
                return false;
            }

            // Need at least 2 matching tiles in hand
            int matchCount = playerTiles.Count(t => t.Data.IsSameType(discardedTile));
            return matchCount >= 2;
        }

        /// <summary>
        /// Checks if a player's tiles can form a Kong with a discarded tile
        /// </summary>
        /// <param name="playerTiles">Tiles in the player's hand</param>
        /// <param name="discardedTile">The discarded tile</param>
        /// <returns>True if player has 3 matching tiles to form Kong</returns>
        public static bool CanClaimForKong(List<TileInstance> playerTiles, TileData discardedTile)
        {
            if (playerTiles == null)
            {
                return false;
            }

            // Need at least 3 matching tiles in hand
            int matchCount = playerTiles.Count(t => t.Data.IsSameType(discardedTile));
            return matchCount >= 3;
        }

        /// <summary>
        /// Checks if a player's tiles can form a Chow with a discarded tile
        /// </summary>
        /// <param name="playerTiles">Tiles in the player's hand</param>
        /// <param name="discardedTile">The discarded tile</param>
        /// <returns>True if player has tiles to complete a sequence</returns>
        public static bool CanClaimForChow(List<TileInstance> playerTiles, TileData discardedTile)
        {
            if (playerTiles == null)
            {
                return false;
            }

            // Can't form Chow with honor tiles
            if (discardedTile.IsHonor())
            {
                return false;
            }

            int discardedNumber = discardedTile.Number;
            TileSuit suit = discardedTile.Suit;

            // Check for sequence patterns
            // Pattern 1: Player has n-1 and n-2 (discard would be highest)
            bool hasLower = playerTiles.Any(t => t.Data.Suit == suit && t.Data.Number == discardedNumber - 1) &&
                           playerTiles.Any(t => t.Data.Suit == suit && t.Data.Number == discardedNumber - 2);

            // Pattern 2: Player has n-1 and n+1 (discard would be middle)
            bool hasMiddle = playerTiles.Any(t => t.Data.Suit == suit && t.Data.Number == discardedNumber - 1) &&
                            playerTiles.Any(t => t.Data.Suit == suit && t.Data.Number == discardedNumber + 1);

            // Pattern 3: Player has n+1 and n+2 (discard would be lowest)
            bool hasUpper = playerTiles.Any(t => t.Data.Suit == suit && t.Data.Number == discardedNumber + 1) &&
                           playerTiles.Any(t => t.Data.Suit == suit && t.Data.Number == discardedNumber + 2);

            return hasLower || hasMiddle || hasUpper;
        }

        /// <summary>
        /// Checks if a player can upgrade an existing Pong to a Kong with a new tile
        /// </summary>
        /// <param name="existingPongTile">The tile type in the existing Pong</param>
        /// <param name="newTile">The newly drawn/acquired tile</param>
        /// <returns>True if the new tile matches the Pong</returns>
        public static bool CanUpgradePongToKong(TileData existingPongTile, TileData newTile)
        {
            return existingPongTile.IsSameType(newTile);
        }

        /// <summary>
        /// Gets all possible Pong formations from a list of tiles
        /// Useful for AI or showing available options
        /// </summary>
        /// <param name="tiles">Available tiles</param>
        /// <returns>List of tile groups that can form Pongs</returns>
        public static List<List<TileInstance>> GetPossiblePongs(List<TileInstance> tiles)
        {
            List<List<TileInstance>> pongs = new List<List<TileInstance>>();

            var groups = tiles
                .GroupBy(t => new { t.Data.Suit, t.Data.Number, t.Data.Wind, t.Data.Dragon })
                .Where(g => g.Count() >= 3);

            foreach (var group in groups)
            {
                pongs.Add(group.Take(3).ToList());
            }

            return pongs;
        }

        /// <summary>
        /// Gets all possible Kong formations from a list of tiles
        /// </summary>
        /// <param name="tiles">Available tiles</param>
        /// <returns>List of tile groups that can form Kongs</returns>
        public static List<List<TileInstance>> GetPossibleKongs(List<TileInstance> tiles)
        {
            List<List<TileInstance>> kongs = new List<List<TileInstance>>();

            var groups = tiles
                .GroupBy(t => new { t.Data.Suit, t.Data.Number, t.Data.Wind, t.Data.Dragon })
                .Where(g => g.Count() >= 4);

            foreach (var group in groups)
            {
                kongs.Add(group.Take(4).ToList());
            }

            return kongs;
        }

        /// <summary>
        /// Gets all possible Chow formations from a list of tiles
        /// </summary>
        /// <param name="tiles">Available tiles</param>
        /// <returns>List of tile groups that can form Chows</returns>
        public static List<List<TileInstance>> GetPossibleChows(List<TileInstance> tiles)
        {
            List<List<TileInstance>> chows = new List<List<TileInstance>>();

            // Group by suit (exclude honors)
            var bySuit = tiles
                .Where(t => !t.Data.IsHonor())
                .GroupBy(t => t.Data.Suit);

            foreach (var suitGroup in bySuit)
            {
                // For each tile in this suit, try to form a sequence starting with it
                var sortedTiles = suitGroup.OrderBy(t => t.Data.Number).ToList();

                for (int i = 0; i < sortedTiles.Count; i++)
                {
                    int startNum = sortedTiles[i].Data.Number;

                    // Can't start a sequence at 8 or 9
                    if (startNum > 7) continue;

                    // Look for n, n+1, n+2
                    var tile1 = sortedTiles[i];
                    var tile2 = sortedTiles.FirstOrDefault(t => t.Data.Number == startNum + 1);
                    var tile3 = sortedTiles.FirstOrDefault(t => t.Data.Number == startNum + 2);

                    if (tile2 != null && tile3 != null)
                    {
                        chows.Add(new List<TileInstance> { tile1, tile2, tile3 });
                    }
                }
            }

            return chows;
        }
    }
}
