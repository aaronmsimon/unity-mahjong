using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MJ.Core.Hand;
using MJ.Core.Tiles;

namespace MJ.UI
{
    /// <summary>
    /// Displays an opponent's hand (face-down tiles)
    /// Shows actual tiles but face-down
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

        private List<TileView> tileViews = new List<TileView>();

        private void Awake()
        {
            if (tileContainer == null)
            {
                tileContainer = transform;
            }
        }

        /// <summary>
        /// Updates the display to show opponent's actual hand (face-down)
        /// </summary>
        public void DisplayHand(Hand hand)
        {
            // Clear existing tiles
            ClearTiles();

            // Get concealed tiles
            var tiles = hand.GetConcealedTiles();

            // Create face-down tile for each
            for (int i = 0; i < tiles.Count; i++)
            {
                CreateFaceDownTile(tiles[i], i);
            }
        }

        private void CreateFaceDownTile(TileInstance tile, int index)
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

            // Get sprite (even though it won't be shown when face-down)
            Sprite sprite = spriteLibrary != null ? spriteLibrary.GetSprite(tile.Data) : null;

            // Setup as face-down - TileView handles showing the back!
            TileView tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(tile, sprite, faceUp: false);
            }

            // Disable interaction
            Button button = tileObj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }

            tileViews.Add(tileView);
        }

        private void ClearTiles()
        {
            foreach (var tileView in tileViews)
            {
                if (tileView != null)
                {
                    Destroy(tileView.gameObject);
                }
            }
            tileViews.Clear();
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
