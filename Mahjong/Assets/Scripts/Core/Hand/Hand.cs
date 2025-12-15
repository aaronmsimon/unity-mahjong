using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MJ.Core.Tiles;
using MJ.Testing;

namespace MJ.Core.Hand
{
    /// <summary>
    /// Represents a player's hand of Mahjong tiles
    /// Manages concealed tiles, exposed melds, and bonus tiles
    /// </summary>
    public class Hand
    {
        // Concealed tiles in the player's hand (hidden from other players)
        private List<TileInstance> concealedTiles;
        
        // Exposed melds (Pong, Kong, Chow) - visible to all players
        private List<Meld> exposedMelds;
        
        // Bonus tiles (Flowers and Seasons) - set aside when drawn
        private List<TileInstance> bonusTiles;

        public Hand()
        {
            concealedTiles = new List<TileInstance>();
            exposedMelds = new List<Meld>();
            bonusTiles = new List<TileInstance>();
        }

        #region Tile Management

        /// <summary>
        /// Adds a tile to the hand
        /// Automatically separates bonus tiles
        /// </summary>
        public void AddTile(TileInstance tile)
        {
            if (tile.Data.IsBonus())
            {
                bonusTiles.Add(tile);
            }
            else
            {
                concealedTiles.Add(tile);
                tile.IsConcealed = true;
            }
        }

        /// <summary>
        /// Adds multiple tiles to the hand
        /// </summary>
        public void AddTiles(IEnumerable<TileInstance> tiles)
        {
            foreach (var tile in tiles)
            {
                AddTile(tile);
            }
        }

        /// <summary>
        /// Removes a specific tile from concealed tiles
        /// </summary>
        public bool RemoveTile(TileInstance tile)
        {
            return concealedTiles.Remove(tile);
        }

        /// <summary>
        /// Removes a tile by its ID
        /// </summary>
        public bool RemoveTileById(int tileId)
        {
            var tile = concealedTiles.FirstOrDefault(t => t.Data.TileId == tileId);
            if (tile != null)
            {
                return concealedTiles.Remove(tile);
            }
            return false;
        }

        /// <summary>
        /// Adds an exposed meld to the hand (Pong, Kong, Chow)
        /// Removes the corresponding tiles from concealed tiles
        /// </summary>
        public void AddExposedMeld(Meld meld)
        {
            exposedMelds.Add(meld);
            
            // Remove the tiles used in the meld from concealed tiles
            foreach (var tile in meld.Tiles)
            {
                concealedTiles.Remove(tile);
                tile.IsConcealed = false;
            }
        }

        /// <summary>
        /// Clears all tiles from the hand
        /// </summary>
        public void Clear()
        {
            concealedTiles.Clear();
            exposedMelds.Clear();
            bonusTiles.Clear();
        }

        #endregion

        #region Sorting

        /// <summary>
        /// Sorts concealed tiles by suit and number
        /// Order: Bamboo, Characters, Dots, Winds, Dragons
        /// </summary>
        public void SortTiles(bool enableLogging)
        {
            if (enableLogging) Debug.Log($"There are {concealedTiles.Count} tiles to sort");
            concealedTiles = concealedTiles
                .OrderBy(t => t.Data.Suit)
                .ThenBy(t => t.Data.Number)
                .ThenBy(t => t.Data.Wind)
                .ThenBy(t => t.Data.Dragon)
                .ToList();
        }

        #endregion

        #region Querying

        /// <summary>
        /// Gets all concealed tiles (read-only)
        /// </summary>
        public IReadOnlyList<TileInstance> GetConcealedTiles()
        {
            return concealedTiles.AsReadOnly();
        }

        /// <summary>
        /// Gets all exposed melds (read-only)
        /// </summary>
        public IReadOnlyList<Meld> GetExposedMelds()
        {
            return exposedMelds.AsReadOnly();
        }

        /// <summary>
        /// Gets all bonus tiles (read-only)
        /// </summary>
        public IReadOnlyList<TileInstance> GetBonusTiles()
        {
            return bonusTiles.AsReadOnly();
        }

        /// <summary>
        /// Gets the total number of concealed tiles
        /// </summary>
        public int ConcealedTileCount => concealedTiles.Count;

        /// <summary>
        /// Gets the total number of tiles including exposed melds
        /// </summary>
        public int TotalTileCount
        {
            get
            {
                int meldCount = exposedMelds.Sum(m => m.Tiles.Count);
                return concealedTiles.Count + meldCount;
            }
        }

        /// <summary>
        /// Finds all tiles of a specific type in concealed tiles
        /// </summary>
        public List<TileInstance> FindTilesOfType(TileData tileData)
        {
            return concealedTiles
                .Where(t => t.Data.IsSameType(tileData))
                .ToList();
        }

        /// <summary>
        /// Counts how many tiles of a specific type are in concealed tiles
        /// </summary>
        public int CountTilesOfType(TileData tileData)
        {
            return concealedTiles.Count(t => t.Data.IsSameType(tileData));
        }

        /// <summary>
        /// Checks if the hand contains at least one tile of a specific type
        /// </summary>
        public bool HasTileOfType(TileData tileData)
        {
            return concealedTiles.Any(t => t.Data.IsSameType(tileData));
        }

        /// <summary>
        /// Gets all unique tile types in the hand (for analysis)
        /// </summary>
        public List<TileData> GetUniqueTileTypes()
        {
            return concealedTiles
                .Select(t => t.Data)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Checks if all concealed tiles are of the same suit (excluding honors)
        /// Useful for determining "All in One Suit" hands
        /// </summary>
        public bool IsAllOneSuit()
        {
            var suits = concealedTiles
                .Where(t => !t.Data.IsHonor())
                .Select(t => t.Data.Suit)
                .Distinct()
                .ToList();

            return suits.Count == 1;
        }

        /// <summary>
        /// Checks if hand contains only terminals and honors
        /// </summary>
        public bool IsAllTerminalsAndHonors()
        {
            return concealedTiles.All(t => t.Data.IsTerminal() || t.Data.IsHonor());
        }

        /// <summary>
        /// Gets tiles organized by suit for display/analysis
        /// </summary>
        public Dictionary<TileSuit, List<TileInstance>> GetTilesBySuit()
        {
            var result = new Dictionary<TileSuit, List<TileInstance>>();
            
            foreach (TileSuit suit in System.Enum.GetValues(typeof(TileSuit)))
            {
                result[suit] = concealedTiles
                    .Where(t => t.Data.Suit == suit)
                    .ToList();
            }
            
            return result;
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Checks if we can form a Pong with a discarded tile
        /// (need 2 matching tiles in hand)
        /// </summary>
        public bool CanFormPong(TileData discardedTile)
        {
            return CountTilesOfType(discardedTile) >= 2;
        }

        /// <summary>
        /// Checks if we can form a Kong with a discarded tile
        /// (need 3 matching tiles in hand)
        /// </summary>
        public bool CanFormKong(TileData discardedTile)
        {
            return CountTilesOfType(discardedTile) >= 3;
        }

        /// <summary>
        /// Checks if we can form a concealed Kong
        /// (need 4 matching tiles in hand)
        /// </summary>
        public bool CanFormConcealedKong()
        {
            var groups = concealedTiles
                .GroupBy(t => new { t.Data.Suit, t.Data.Number, t.Data.Wind, t.Data.Dragon })
                .Where(g => g.Count() >= 4);

            return groups.Any();
        }

        /// <summary>
        /// Gets all possible concealed Kongs in the hand
        /// </summary>
        public List<List<TileInstance>> GetPossibleConcealedKongs()
        {
            var kongs = new List<List<TileInstance>>();
            
            var groups = concealedTiles
                .GroupBy(t => new { t.Data.Suit, t.Data.Number, t.Data.Wind, t.Data.Dragon })
                .Where(g => g.Count() >= 4);

            foreach (var group in groups)
            {
                kongs.Add(group.Take(4).ToList());
            }

            return kongs;
        }

        /// <summary>
        /// Checks if we can form a Chow with a discarded tile
        /// (need tiles to form a sequence like 1-2, 2-4, or 1-3)
        /// </summary>
        public bool CanFormChow(TileData discardedTile)
        {
            // Can't form Chow with honor tiles
            if (discardedTile.IsHonor())
            {
                return false;
            }

            int discardedNumber = discardedTile.Number;
            TileSuit suit = discardedTile.Suit;

            // Check for sequence patterns
            // Pattern 1: We have n-1 and n-2 (discard is highest)
            bool hasLower = HasTileOfType(new TileData(suit, discardedNumber - 1, 0)) &&
                           HasTileOfType(new TileData(suit, discardedNumber - 2, 0));

            // Pattern 2: We have n-1 and n+1 (discard is middle)
            bool hasMiddle = HasTileOfType(new TileData(suit, discardedNumber - 1, 0)) &&
                            HasTileOfType(new TileData(suit, discardedNumber + 1, 0));

            // Pattern 3: We have n+1 and n+2 (discard is lowest)
            bool hasUpper = HasTileOfType(new TileData(suit, discardedNumber + 1, 0)) &&
                           HasTileOfType(new TileData(suit, discardedNumber + 2, 0));

            return hasLower || hasMiddle || hasUpper;
        }

        #endregion

        #region Debug

        /// <summary>
        /// Returns a string representation of the hand for debugging
        /// </summary>
        public override string ToString()
        {
            string concealed = string.Join(", ", concealedTiles.Select(t => t.Data.ToString()));
            string melds = string.Join(", ", exposedMelds.Select(m => m.ToString()));
            string bonus = string.Join(", ", bonusTiles.Select(t => t.Data.ToString()));

            return $"Concealed: [{concealed}] | Melds: [{melds}] | Bonus: [{bonus}]";
        }

        /// <summary>
        /// Logs hand details to console
        /// </summary>
        public void DebugPrint()
        {
            Debug.Log($"=== Hand Debug ===");
            Debug.Log($"Concealed Tiles ({concealedTiles.Count}): {string.Join(", ", concealedTiles.Select(t => t.Data.ToString()))}");
            Debug.Log($"Exposed Melds ({exposedMelds.Count}): {string.Join(" | ", exposedMelds.Select(m => m.ToString()))}");
            Debug.Log($"Bonus Tiles ({bonusTiles.Count}): {string.Join(", ", bonusTiles.Select(t => t.Data.ToString()))}");
            Debug.Log($"Total Tiles: {TotalTileCount}");
        }

        #endregion
    }
}
