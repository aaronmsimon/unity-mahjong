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
                if (item.maxValue - item.minValue > 0) {
                    for (int i = item.minValue; i <= item.maxValue; i++) {
                        tiles.AddRange(CreateTileCopies(new TileType(item.suit, i), item.copies));
                    }
                } else if (item.winds.Length > 0) {
                    foreach (WindType wind in item.winds) {
                        tiles.AddRange(CreateTileCopies(new TileType(wind), item.copies));
                    }
                } else if (item.dragons.Length > 0) {
                    foreach (DragonType dragon in item.dragons) {
                        tiles.AddRange(CreateTileCopies(new TileType(dragon), item.copies));
                    }
                } else if (item.suit == TileSuit.Jokers) {
                    throw new System.Exception("Jokers not implemented yet.");
                }
            }
            return tiles;
        }

        private List<Tile> CreateTileCopies(TileType tileType, int copies) {
            List<Tile> tiles = new List<Tile>();

            for (int i = 0; i < copies; i++) {
                Tile test = new Tile(tileType);
                tiles.Add(test);
            }

            return tiles;
        }
    }
}
