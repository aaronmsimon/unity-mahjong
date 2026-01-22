using System.Collections.Generic;

namespace MJ.Core.Tiles
{
    public class TileSetFactory
    {
        /// <summary>
        /// Shuffles a list of tiles using Fisher-Yates algorithm
        /// </summary>
        /// <param name="tiles">List of tiles to shuffle in-place</param>
        /// <param name="seed">Optional seed for reproducible shuffles. If omitted, uses a random seed.</param>
        // public void ShuffleTiles(List<Tile> tiles, int? seed = null)
        // {
        //     System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            
        //     for (int i = tiles.Count - 1; i > 0; i--)
        //     {
        //         int randomIndex = rng.Next(0, i + 1);
        //         (tiles[i], tiles[randomIndex]) = (tiles[randomIndex], tiles[i]);
        //     }
        // }

        // private List<Tile> CreateTileCopies(TileType tileType, int copies) {
        //     List<Tile> tiles = new List<Tile>();

        //     for (int i = 0; i < copies; i++) {
        //         tiles.Add(new Tile(tileType));
        //     }

        //     return tiles;
        // }
    }
}
