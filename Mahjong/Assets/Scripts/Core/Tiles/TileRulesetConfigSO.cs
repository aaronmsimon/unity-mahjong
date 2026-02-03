using UnityEngine;

namespace MJ.Core.Tiles
{
    [CreateAssetMenu(fileName = "New TileRulesetConfig", menuName = "Mahjong/Tiles/Tile Ruleset Config")]
    public class TileRulesetConfigSO : ScriptableObject
    {
        public bool IncludeSuits;
        public bool IncludeWinds;
        public bool IncludeDragons;
        public bool IncludeFlowers;
        public bool IncludeSeasons;
        public bool IncludeJokers;
        public bool IncludeBlanks;

        public int CopiesOfSuits;
        public int CopiesOfHonors;
        public int CopiesOfBonus;
        public int CopiesOfJokers;
        public int CopiesOfBlanks;
        public int CopiesOfRed5;
    }
}
