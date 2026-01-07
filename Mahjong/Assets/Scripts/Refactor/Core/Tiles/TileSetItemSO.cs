using UnityEngine;

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
    }
}
