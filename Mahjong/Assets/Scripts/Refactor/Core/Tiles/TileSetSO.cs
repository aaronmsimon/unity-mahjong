using System.Collections.Generic;
using UnityEngine;

namespace MJ2.Core.Tiles
{
    [CreateAssetMenu(fileName = "TileSet", menuName = "Mahjong/Tile Sets/Tile Set", order = 1)]
    public class TileSetSO : ScriptableObject
    {
        [SerializeField] private TileSetItemSO[] tileSetItems;

        private List<Tile> tiles;

        public List<Tile> CreateTileSet() {
            List<Tile> tiles = new List<Tile>();

            foreach (TileSetItemSO item in tileSetItems) {
                if (item.maxValue - item.minValue + 1 > 0) {
                    for (int i = item.minValue; i <= item.maxValue; i++) {
                        AddTileCopies(new TileType(item.suit, i), item.copies);
                    }
                } else if (item.winds != null) {
                    foreach (WindType wind in item.winds) {
                        AddTileCopies(new TileType(wind), item.copies);
                    }
                } else if (item.dragons != null) {
                    foreach (DragonType dragon in item.dragons) {
                        AddTileCopies(new TileType(dragon), item.copies);
                    }
                } else if (item.suit == TileSuit.Jokers) {
                    throw new System.Exception("Jokers not implemented yet.");
                }
            }
            return tiles;
        }

        private void AddTileCopies(TileType tileType, int copies) {
            Debug.Log($"tile type: {tileType}, copies: {copies}");
            for (int i = 0; i < copies; i++) {
                tiles.Add(new Tile(tileType));
            }
        }
    }
}
