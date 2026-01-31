using UnityEngine;
using MJ.Core.Tiles;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        [SerializeField] private TileCatalogSO catalog;
        [SerializeField] private TileDefinitionSO tileDef;
        
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("get TileDef by Passing TileID to TileCatalog");

            TileID id = tileDef.TileInfo;
            TileDefinitionSO result = catalog.GetTileDefinition(id);

            Debug.Assert(result != null, "Catalog returned null TileDefinition");
            Debug.Assert(
                result.TileInfo.Equals(id),
                $"Catalog returned wrong tile. Expected {id}, got {result.TileInfo}"
            );

            Debug.Log($"PASS: {id} resolved to {result.name}");
        }
    }
}
