using UnityEngine;

namespace MJ.Core.Tiles
{
    public enum Suit {
        Characters,
        Bamboo,
        Dots,
        Winds,
        Dragons,
        Flowers,
        Seasons,
        Jokers,
        Blanks
    }

    public enum Winds {
        East,
        South,
        West,
        North
    }

    public enum Dragons {
        Red,
        Green,
        White
    }

    [CreateAssetMenu(fileName = "New TileDefinition", menuName = "Mahjong/Tiles/Tile Definition")]
    public class TileDefinition : ScriptableObject
    {
        public Suit Suit;
        public int Number;
        public Winds Wind;
        public Dragons Dragon;
        public Sprite Sprite;
        public string DisplayName;
    }
}
