using UnityEngine;
using MJ2.Core.Tiles;
using System.Collections.Generic;

namespace MJ2.RefactorTesting
{
    public class RefactorTesting : MonoBehaviour
    {
        [SerializeField] private TileSetSO tileSetSO;

        private void Start() {
            TileSetFactory factory = new TileSetFactory(tileSetSO);
            List<Tile> tiles = factory.CreateTileSet();
            // factory.ShuffleTiles(tiles);

            foreach (Tile tile in tiles) {
                Debug.Log($"{tile.tileType.ToString()} - {tile.tileType.ToCompactString()}");
            }
        }
    }
}
