using UnityEngine;

namespace MJ.Core.Tiles
{
    [CreateAssetMenu(fileName = "New TileDefinition", menuName = "Mahjong/Tiles/Tile Definition")]
    public class TileDefinitionSO : ScriptableObject
    {
        public TileID TileInfo;
        public Sprite Sprite;
        public string DisplayName;
    }
}
