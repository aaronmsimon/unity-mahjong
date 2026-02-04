using System.Collections.Generic;

namespace MJ.Core.Tiles
{
    public static class TileSetFactory
    {
        public static List<TileInstance> Build(TileCatalogSO catalog, TileRulesetConfigSO ruleset)
        {
            var result = new List<TileInstance>();
            int nextId = 0;

            foreach (TileDefinitionSO tileDef in catalog.tiles) {
                int copies = GetCopiesFor(tileDef.TileInfo, ruleset);
                if (copies <= 0) continue;

                for (int i = 0; i < copies; i++) {
                    result.Add(new TileInstance(nextId++, tileDef));
                }
            }

            return result;
        }

        private static int GetCopiesFor(TileID tileID, TileRulesetConfigSO ruleset)
        {
            if (tileID.IsSuited) return ruleset.IncludeSuits ? ruleset.CopiesOfSuits : 0;

            if (tileID.Suit == Suit.Winds) return ruleset.IncludeWinds ? ruleset.CopiesOfHonors : 0;

            if (tileID.Suit == Suit.Dragons) return ruleset.IncludeDragons ? ruleset.CopiesOfHonors : 0;

            if (tileID.Suit == Suit.Flowers) return ruleset.IncludeFlowers ? ruleset.CopiesOfBonus : 0;

            if (tileID.Suit == Suit.Seasons) return ruleset.IncludeSeasons ? ruleset.CopiesOfBonus : 0;

            // Jokers/Blanks/etc
            
            return 0;
        }
        
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
