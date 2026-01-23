using UnityEngine;
using MJ.Core.Tiles;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        private void Start() {
            TileID tile1 = new TileID();
            tile1.Suit = Suit.Characters;
            tile1.Rank = 3;
            TileID tile2 = new TileID();
            tile2.Suit = Suit.Winds;
            tile2.Wind = Winds.East;
            tile2.Rank = 3;

            Debug.Log("-= TESTING =-");
            Debug.Log("for TileID equality");
            Debug.Log($"Tile 1 ({tile1}) == Tile 2 ({tile2})? => {tile1.Equals(tile2)}");
        }
    }
}
