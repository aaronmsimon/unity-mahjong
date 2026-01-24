using UnityEngine;
using MJ.Core.Tiles;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("TileID equality");

            TileID tile1 = new TileID(Suit.Characters, 3);
            TileID tile2 = new TileID(Winds.East);
            TestEquality(tile1, tile2);

            tile1 = new TileID(Suit.Characters, 3);
            tile2 = new TileID(Suit.Characters, 3);
            TestEquality(tile1, tile2);

            tile1 = new TileID(Suit.Characters, 5);
            tile2 = new TileID(Suit.Bamboo, 5);
            TestEquality(tile1, tile2);
        }

        private void TestEquality(TileID tile1, TileID tile2) {
            Debug.Log($"Tile 1 ({tile1}) == Tile 2 ({tile2})? => {tile1.Equals(tile2)}");
        }
    }
}
