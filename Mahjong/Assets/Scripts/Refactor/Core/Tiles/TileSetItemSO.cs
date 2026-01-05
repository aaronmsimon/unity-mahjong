using System.Collections.Generic;
using UnityEngine;
using MJ.Core.Tiles;

namespace MJ2.Core.Tiles
{
    [CreateAssetMenu(fileName = "TileSetItem", menuName = "Mahjong/Tile Sets/Tile Set Item", order = 0)]
    public class TileSetItemSO : ScriptableObject
    {
        public TileSuit suit;
        public int minValue;
        public int maxValue;
        public WindType[] winds;
        public DragonType[] dragons;
        public int copies;
        
        private List<TileInstance> tiles;

        public List<TileInstance> GetTiles() => tiles;
    }
}
