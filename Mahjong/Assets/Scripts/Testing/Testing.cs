using System.Collections.Generic;
using UnityEngine;
using MJ.Core.Tiles;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        [SerializeField] private TileCatalogSO catalog;
        
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("create tile instances from the tilecatalog");

            List<TileInstance> tiles = new List<TileInstance>();

            // int instanceID = 0;

            // foreach (TileDefinitionSO tileDefinition in catalog.tiles) {
            //     for (int i = 0; i < tileDefinition.Copies; i++) {
            //         tiles.Add(new TileInstance(instanceID++, tileDefinition));
            //     }
            // }

            foreach (TileInstance tile in tiles) {
                Debug.Log($"Tile Instance ID {tile.InstanceID}: Tile ID {tile.ID}");
            }
        }
    }
}
