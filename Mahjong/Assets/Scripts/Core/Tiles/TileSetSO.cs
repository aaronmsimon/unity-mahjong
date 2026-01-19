using UnityEngine;

namespace MJ.Core.Tiles
{
    [CreateAssetMenu(fileName = "TileSet", menuName = "Mahjong/Tile Sets/Tile Set", order = 1)]
    public class TileSetSO : ScriptableObject
    {
        public TileSetItemSO[] tileSetItems;
    }
}
