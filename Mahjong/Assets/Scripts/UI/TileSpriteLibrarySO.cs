using UnityEngine;
using System.Collections.Generic;
using MJ.Core.Tiles;

namespace MJ.UI
{
    /// <summary>
    /// Manages mapping between TileData and sprites
    /// Assign your tile sprites here in the Unity Inspector
    /// </summary>
    [CreateAssetMenu(fileName = "TileSpriteLibrary", menuName = "Mahjong/Tile Sprite Library")]
    public class TileSpriteLibrarySO : ScriptableObject
    {
        [Header("Bamboo Tiles (1-9)")]
        [SerializeField] private Sprite[] bambooSprites = new Sprite[9];
        
        [Header("Character Tiles (1-9)")]
        [SerializeField] private Sprite[] characterSprites = new Sprite[9];
        
        [Header("Dot Tiles (1-9)")]
        [SerializeField] private Sprite[] dotSprites = new Sprite[9];
        
        [Header("Wind Tiles")]
        [SerializeField] private Sprite eastWindSprite;
        [SerializeField] private Sprite southWindSprite;
        [SerializeField] private Sprite westWindSprite;
        [SerializeField] private Sprite northWindSprite;
        
        [Header("Dragon Tiles")]
        [SerializeField] private Sprite redDragonSprite;
        [SerializeField] private Sprite greenDragonSprite;
        [SerializeField] private Sprite whiteDragonSprite;
        
        [Header("Bonus Tiles")]
        [SerializeField] private Sprite[] flowerSprites = new Sprite[4];
        [SerializeField] private Sprite[] seasonSprites = new Sprite[4];
        
        [Header("Back/Face Down")]
        [SerializeField] private Sprite tileBackSprite;

        public Sprite TileBackSprite => tileBackSprite;

        /// <summary>
        /// Gets the sprite for a given tile
        /// </summary>
        public Sprite GetSprite(TileData tile)
        {
            switch (tile.Suit)
            {
                case TileSuit.Bamboo:
                    return GetSpriteFromArray(bambooSprites, tile.Number - 1);
                    
                case TileSuit.Characters:
                    return GetSpriteFromArray(characterSprites, tile.Number - 1);
                    
                case TileSuit.Dots:
                    return GetSpriteFromArray(dotSprites, tile.Number - 1);
                    
                case TileSuit.Wind:
                    return tile.Wind switch
                    {
                        WindType.East => eastWindSprite,
                        WindType.South => southWindSprite,
                        WindType.West => westWindSprite,
                        WindType.North => northWindSprite,
                        _ => null
                    };
                    
                case TileSuit.Dragon:
                    return tile.Dragon switch
                    {
                        DragonType.Red => redDragonSprite,
                        DragonType.Green => greenDragonSprite,
                        DragonType.White => whiteDragonSprite,
                        _ => null
                    };
                    
                case TileSuit.Flower:
                    return GetSpriteFromArray(flowerSprites, tile.Number - 1);
                    
                case TileSuit.Season:
                    return GetSpriteFromArray(seasonSprites, tile.Number - 1);
                    
                default:
                    Debug.LogWarning($"No sprite found for tile: {tile}");
                    return null;
            }
        }

        private Sprite GetSpriteFromArray(Sprite[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length)
            {
                return null;
            }
            return array[index];
        }

        /// <summary>
        /// Validates that all sprites are assigned (for debugging)
        /// </summary>
        [ContextMenu("Validate Sprites")]
        public void ValidateSprites()
        {
            int missingCount = 0;
            
            missingCount += ValidateArray(bambooSprites, "Bamboo", 9);
            missingCount += ValidateArray(characterSprites, "Characters", 9);
            missingCount += ValidateArray(dotSprites, "Dots", 9);
            
            if (eastWindSprite == null) { Debug.LogWarning("Missing: East Wind"); missingCount++; }
            if (southWindSprite == null) { Debug.LogWarning("Missing: South Wind"); missingCount++; }
            if (westWindSprite == null) { Debug.LogWarning("Missing: West Wind"); missingCount++; }
            if (northWindSprite == null) { Debug.LogWarning("Missing: North Wind"); missingCount++; }
            
            if (redDragonSprite == null) { Debug.LogWarning("Missing: Red Dragon"); missingCount++; }
            if (greenDragonSprite == null) { Debug.LogWarning("Missing: Green Dragon"); missingCount++; }
            if (whiteDragonSprite == null) { Debug.LogWarning("Missing: White Dragon"); missingCount++; }
            
            missingCount += ValidateArray(flowerSprites, "Flowers", 4);
            missingCount += ValidateArray(seasonSprites, "Seasons", 4);
            
            if (tileBackSprite == null) { Debug.LogWarning("Missing: Tile Back"); missingCount++; }
            
            if (missingCount == 0)
            {
                Debug.Log("✓ All sprites assigned!");
            }
            else
            {
                Debug.LogError($"Missing {missingCount} sprites!");
            }
        }

        private int ValidateArray(Sprite[] array, string name, int expectedCount)
        {
            int missing = 0;
            if (array == null || array.Length != expectedCount)
            {
                Debug.LogWarning($"{name} array should have {expectedCount} sprites");
                return expectedCount;
            }
            
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    Debug.LogWarning($"Missing: {name} {i + 1}");
                    missing++;
                }
            }
            return missing;
        }
    }
}
