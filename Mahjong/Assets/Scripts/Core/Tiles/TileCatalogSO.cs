using System.Collections.Generic;
using UnityEngine;

namespace MJ.Core.Tiles
{
    [CreateAssetMenu(fileName = "New TileCatalog", menuName = "Mahjong/Tiles/Tile Catalog")]
    public class TileCatalogSO : ScriptableObject
    {
        public List<TileDefinitionSO> tiles = new List<TileDefinitionSO>();

        private Dictionary<TileID, TileDefinitionSO> lookup;

        public TileDefinitionSO GetTileDefinition(TileID tileID) {
            if (lookup == null) BuildLookup();

            if (!lookup.TryGetValue(tileID, out TileDefinitionSO tileDef)) Debug.LogAssertion($"No TileDefinition found for {tileID}");

            return tileDef;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<TileID, TileDefinitionSO>();

            foreach (TileDefinitionSO tileDef in tiles)
            {
                TileID tileID = tileDef.TileInfo;

                if (!lookup.ContainsKey(tileID)) {
                    lookup.Add(tileID, tileDef);
                } else {
                    Debug.LogError($"Duplicate TileID: {tileID}");
                }
            }
        }
    }
}
