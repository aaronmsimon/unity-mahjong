using System.Collections.Generic;
using UnityEngine;
using MJ.Core.Tiles;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("TileID sorting");

            List<TileID> tiles = new List<TileID> {
                new(Dragons.Green),
                new(Suit.Flowers, 2),
                new(Winds.East),
                new(Suit.Characters, 5),
                new(Suit.Bamboo, 5),
                new(Suit.Seasons, 4),
                new(Suit.Characters, 3),
            };

            tiles.Sort();

            for (int i = 0; i < tiles.Count; i++) {
                Debug.Log($"Tile {i}: {tiles[i]}");
            }
        }
    }
}
