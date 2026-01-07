using System.Collections.Generic;

namespace MJ2.Core.Tiles
{
    public class TileSetFactory
    {
        private TileSetSO tileSetSO;

        public TileSetFactory(TileSetSO tileSetSO) {
            this.tileSetSO = tileSetSO;
        }

        public List<Tile> CreateTileSet() {
            List<Tile> tiles = new List<Tile>();

            foreach (TileSetItemSO item in tileSetSO.tileSetItems) {
                if (item.maxValue - item.minValue > 0) {
                    for (int i = item.minValue; i <= item.maxValue; i++) {
                        tiles.AddRange(CreateTileCopies(new TileType(item.suit, i), item.copies));
                    }
                } else if (item.winds.Length > 0) {
                    foreach (WindType wind in item.winds) {
                        tiles.AddRange(CreateTileCopies(new TileType(wind), item.copies));
                    }
                } else if (item.dragons.Length > 0) {
                    foreach (DragonType dragon in item.dragons) {
                        tiles.AddRange(CreateTileCopies(new TileType(dragon), item.copies));
                    }
                } else if (item.suit == TileSuit.Jokers) {
                    throw new System.Exception("Jokers not implemented yet.");
                }
            }
            return tiles;
        }

        /// <summary>
        /// Shuffles a list of tiles using Fisher-Yates algorithm
        /// </summary>
        /// <param name="tiles">List of tiles to shuffle in-place</param>
        /// <param name="seed">Optional seed for reproducible shuffles. If omitted, uses a random seed.</param>
        public void ShuffleTiles(List<Tile> tiles, int? seed = null)
        {
            System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            
            for (int i = tiles.Count - 1; i > 0; i--)
            {
                int randomIndex = rng.Next(0, i + 1);
                (tiles[i], tiles[randomIndex]) = (tiles[randomIndex], tiles[i]);
            }
        }

        private List<Tile> CreateTileCopies(TileType tileType, int copies) {
            List<Tile> tiles = new List<Tile>();

            for (int i = 0; i < copies; i++) {
                tiles.Add(new Tile(tileType));
            }

            return tiles;
        }
    }
}
