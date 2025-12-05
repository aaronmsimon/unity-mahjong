using System;
using System.Collections.Generic;

namespace MJ.TileModel
{
    public static class WallFactory
    {
        public static List<TileInstance> BuildHKWall(int? seed = null)
        {
            List<TileInstance> tileInstances = new List<TileInstance>(136);
            int nextId = 0;

            // --- Suited tiles: 1–9 in each suit, 4 copies each ---
            foreach (Suit suit in new[] { Suit.Dots, Suit.Bamboo, Suit.Characters })
            {
                for (int rank = 1; rank <= 9; rank++)
                {
                    TileType type = TileType.Suited(suit, rank);
                    nextId = AddCopiesToInstance(type, nextId, tileInstances, 4);
                }
            }

            // --- Winds: East, South, West, North (4 copies each) ---
            foreach (Wind wind in (Wind[])Enum.GetValues(typeof(Wind)))
            {
                TileType type = TileType.WindTile(wind);
                nextId = AddCopiesToInstance(type, nextId, tileInstances, 4);
            }

            // --- Dragons: Red, Green, White (4 copies each) ---
            foreach (Dragon dragon in (Dragon[])Enum.GetValues(typeof(Dragon)))
            {
                TileType type = TileType.DragonTile(dragon);
                nextId = AddCopiesToInstance(type, nextId, tileInstances, 4);
            }

            // TODO: if you decide to use flowers/seasons for Hong Kong rules,
            // you can add them here in a similar pattern.

            // --- Shuffle the wall ---
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();
            ShuffleInPlace(tileInstances, rng);

            return tileInstances;
        }

        // Fisher–Yates shuffle.
        private static void ShuffleInPlace<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static int AddCopiesToInstance(TileType type, int startingId, List<TileInstance> tileInstances, int copies) {
            for (int copy = 0; copy < copies; copy++)
            {
                tileInstances.Add(new TileInstance(type, startingId++));
            }
            return startingId;
        }
    }
}
