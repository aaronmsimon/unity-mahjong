using UnityEngine;

namespace MJ.Core.Tiles
{
    [CreateAssetMenu(fileName = "New TileDefinition", menuName = "Mahjong/Tiles/Tile Definition")]
    public class TileDefinition : ScriptableObject
    {
        public TileID tileInfo;
        public Sprite Sprite;
        public string DisplayName;
    }
}
