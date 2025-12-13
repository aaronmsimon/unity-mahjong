using System.Collections.Generic;
using UnityEngine;

namespace MJ.Core.Tiles
{
    /// <summary>
    /// Static factory class for creating Mahjong tiles
    /// </summary>
    public static class TileFactory
    {
        /// <summary>
        /// Creates a complete set of 144 Hong Kong Style Mahjong tiles
        /// Includes: 4 sets of suited tiles (108), honor tiles (28), and optional bonus tiles (8)
        /// </summary>
        /// <param name="includeFlowersAndSeasons">Whether to include the 8 bonus tiles</param>
        /// <returns>List of all TileInstance objects</returns>
        public static List<TileInstance> CreateFullTileSet(bool includeFlowersAndSeasons = true)
        {
            List<TileInstance> tiles = new List<TileInstance>();
            int nextTileId = 0;

            // Create 4 copies of each suited tile (Bamboo, Characters, Dots: 1-9)
            tiles.AddRange(CreateSuitedTiles(TileSuit.Bamboo, 4, ref nextTileId));
            tiles.AddRange(CreateSuitedTiles(TileSuit.Characters, 4, ref nextTileId));
            tiles.AddRange(CreateSuitedTiles(TileSuit.Dots, 4, ref nextTileId));

            // Create 4 copies of each Wind tile
            tiles.AddRange(CreateWindTiles(4, ref nextTileId));

            // Create 4 copies of each Dragon tile
            tiles.AddRange(CreateDragonTiles(4, ref nextTileId));

            // Optionally create bonus tiles (1 of each)
            if (includeFlowersAndSeasons)
            {
                tiles.AddRange(CreateBonusTiles(ref nextTileId));
            }

            Debug.Log($"TileFactory: Created {tiles.Count} tiles");
            return tiles;
        }

        /// <summary>
        /// Creates tiles for a specific suit (Bamboo, Characters, or Dots)
        /// </summary>
        private static List<TileInstance> CreateSuitedTiles(TileSuit suit, int copiesPerTile, ref int nextTileId)
        {
            List<TileInstance> tiles = new List<TileInstance>();

            for (int number = 1; number <= 9; number++)
            {
                for (int copy = 0; copy < copiesPerTile; copy++)
                {
                    TileData data = new TileData(suit, number, nextTileId++);
                    tiles.Add(new TileInstance(data));
                }
            }

            return tiles;
        }

        /// <summary>
        /// Creates Wind tiles (East, South, West, North)
        /// </summary>
        private static List<TileInstance> CreateWindTiles(int copiesPerTile, ref int nextTileId)
        {
            List<TileInstance> tiles = new List<TileInstance>();

            foreach (WindType wind in System.Enum.GetValues(typeof(WindType)))
            {
                for (int copy = 0; copy < copiesPerTile; copy++)
                {
                    TileData data = new TileData(wind, nextTileId++);
                    tiles.Add(new TileInstance(data));
                }
            }

            return tiles;
        }

        /// <summary>
        /// Creates Dragon tiles (Red, Green, White)
        /// </summary>
        private static List<TileInstance> CreateDragonTiles(int copiesPerTile, ref int nextTileId)
        {
            List<TileInstance> tiles = new List<TileInstance>();

            foreach (DragonType dragon in System.Enum.GetValues(typeof(DragonType)))
            {
                for (int copy = 0; copy < copiesPerTile; copy++)
                {
                    TileData data = new TileData(dragon, nextTileId++);
                    tiles.Add(new TileInstance(data));
                }
            }

            return tiles;
        }

        /// <summary>
        /// Creates bonus tiles (Flowers and Seasons) - typically 1 of each
        /// In Hong Kong Style, these are: 4 Flowers + 4 Seasons
        /// </summary>
        private static List<TileInstance> CreateBonusTiles(ref int nextTileId)
        {
            List<TileInstance> tiles = new List<TileInstance>();

            // Create 4 Flower tiles (numbered 1-4)
            for (int number = 1; number <= 4; number++)
            {
                TileData data = new TileData(TileSuit.Flower, number, nextTileId++);
                tiles.Add(new TileInstance(data));
            }

            // Create 4 Season tiles (numbered 1-4)
            for (int number = 1; number <= 4; number++)
            {
                TileData data = new TileData(TileSuit.Season, number, nextTileId++);
                tiles.Add(new TileInstance(data));
            }

            return tiles;
        }

        /// <summary>
        /// Shuffles a list of tiles using Fisher-Yates algorithm
        /// </summary>
        public static void ShuffleTiles(List<TileInstance> tiles)
        {
            for (int i = tiles.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                TileInstance temp = tiles[i];
                tiles[i] = tiles[randomIndex];
                tiles[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Helper method to print tile set summary for debugging
        /// </summary>
        public static void PrintTileSetSummary(List<TileInstance> tiles)
        {
            Dictionary<string, int> summary = new Dictionary<string, int>();

            foreach (TileInstance tile in tiles)
            {
                string key = tile.Data.ToString();
                if (!summary.ContainsKey(key))
                {
                    summary[key] = 0;
                }
                summary[key]++;
            }

            Debug.Log("=== Tile Set Summary ===");
            foreach (var kvp in summary)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value} tiles");
            }
            Debug.Log($"Total: {tiles.Count} tiles");
        }
    }
}
