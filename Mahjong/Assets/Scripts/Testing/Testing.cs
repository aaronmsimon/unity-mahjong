using System.Collections.Generic;
using UnityEngine;
using MJ.Core.Tiles;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        [SerializeField] private TileRulesetConfigSO config;
        [SerializeField] private TileCatalogSO catalog;
        
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("create tile instances from the factory");

            List<TileInstance> tiles = TileSetFactory.Build(catalog, config);

            foreach (TileInstance tile in tiles) {
                Debug.Log($"Tile Instance ID {tile.InstanceID}: Tile ID {tile.ID}");
            }
        }
    }
}
