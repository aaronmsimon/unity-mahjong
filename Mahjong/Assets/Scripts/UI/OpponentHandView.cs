using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MJ.UI
{
    /// <summary>
    /// Displays an opponent's hand (face-down tiles)
    /// Shows tile count but not actual tiles
    /// </summary>
    public class OpponentHandView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TileSpriteLibrarySO spriteLibrary;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform tileContainer;

        [Header("Layout")]
        [SerializeField] private float tileSpacing = 70f;
        [SerializeField] private bool verticalLayout = false; // For left/right players

        private List<GameObject> tileBackViews = new List<GameObject>();

        private void Awake()
        {
            if (tileContainer == null)
            {
                tileContainer = transform;
            }
        }

        /// <summary>
        /// Updates the display to show the correct number of face-down tiles
        /// </summary>
        public void UpdateHandDisplay(int tileCount)
        {
            // Clear existing tiles
            ClearTiles();

            // Create face-down tiles
            for (int i = 0; i < tileCount; i++)
            {
                CreateFaceDownTile(i);
            }
        }

        private void CreateFaceDownTile(int index)
        {
            GameObject tileObj = Instantiate(tilePrefab, tileContainer);
            
            // Position based on layout
            RectTransform rectTransform = tileObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (verticalLayout)
                {
                    // Vertical layout for left/right players
                    rectTransform.anchoredPosition = new Vector2(0, -index * tileSpacing);
                }
                else
                {
                    // Horizontal layout for top player
                    rectTransform.anchoredPosition = new Vector2(index * tileSpacing, 0);
                }
            }

            // Setup as face-down
            TileView tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                // Use a dummy tile and show face-down
                var dummyTile = new MJ.Core.Tiles.TileInstance(
                    new MJ.Core.Tiles.TileData(MJ.Core.Tiles.TileSuit.Bamboo, 1, 1)
                );
                
                Sprite backSprite = spriteLibrary != null ? spriteLibrary.TileBackSprite : null;
                tileView.Setup(dummyTile, backSprite, faceUp: false);
            }

            // Disable interaction
            Button button = tileObj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }

            tileBackViews.Add(tileObj);
        }

        private void ClearTiles()
        {
            foreach (var tile in tileBackViews)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }
            tileBackViews.Clear();
        }

        /// <summary>
        /// Clears all tiles
        /// </summary>
        public void Clear()
        {
            ClearTiles();
        }
    }
}
