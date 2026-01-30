using UnityEngine;
using MJ.Core.Tiles;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        [SerializeField] private TileCatalog catalog;
        [SerializeField] private TileDefinition tileDef;
        
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("get TileDef by Passing TileID to TileCatalog");

            TileDefinition tile = catalog.GetTileDefinition(tileDef.TileInfo);

            Debug.Log($"Tile {tileDef.TileInfo} was found in the catalog as {tile.TileInfo}");
        }
    }
}
