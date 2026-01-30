using System.Collections.Generic;
using UnityEngine;

namespace MJ.Core.Tiles
{
    [CreateAssetMenu(fileName = "New TileCatalog", menuName = "Mahjong/Tiles/Tile Catalog")]
    public class TileCatalog : ScriptableObject
    {
        public List<TileDefinition> tiles = new List<TileDefinition>();

        private Dictionary<TileID, TileDefinition> lookup;

        public TileDefinition GetTileDefinition(TileID tileID) {
            if (lookup == null) BuildLookup();

            if (!lookup.TryGetValue(tileID, out TileDefinition tileDef)) Debug.LogAssertion($"No TileDefinition found for {tileID}");

            return tileDef;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<TileID, TileDefinition>();

            foreach (TileDefinition tileDef in tiles)
            {
                if (!lookup.ContainsKey(tileDef.TileInfo)) {
                    lookup.Add(tileDef.TileInfo, tileDef);
                } else {
                    Debug.LogError($"Duplicate TileID: {tileDef.TileInfo}");
                }
            }
        }
    }
}
