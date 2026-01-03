using UnityEngine;
using MJ.Core.Tiles2;

namespace MJ.RefactorTesting
{
    public class RefactorTesting : MonoBehaviour
    {
        private void Start() {
            TileType bamboo5_a = new TileType(TileSuit.Bamboo, 5);
            TileType bamboo5_b = new TileType(TileSuit.Bamboo, 5);
            TileType numbers5 = new TileType(TileSuit.Characters, 5);

            Debug.Log($"compare bamboo 5: {bamboo5_a == bamboo5_b}");
            Debug.Log($"compare 5s: {bamboo5_a == numbers5}");
        }
    }
}
