using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MJ.Core.Hand;
using MJ.Core.Tiles;

namespace MJ.UI
{
    /// <summary>
    /// Displays a player's hand with interactive tiles
    /// Handles tile selection and discard
    /// </summary>
    public class HandView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TileSpriteLibrarySO spriteLibrary;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform tileContainer;

        [Header("Layout")]
        [SerializeField] private float tileSpacing = 80f;
        [SerializeField] private float selectedOffset = 20f; // How much selected tile moves up

        [Header("Bonus Tiles")]
        [SerializeField] private Transform bonusTileContainer;
        [SerializeField] private float bonusTileSpacing = 60f;

        // Current hand being displayed
        private Hand currentHand;
        
        // Visual tiles
        private List<TileView> concealedTileViews = new List<TileView>();
        private List<TileView> bonusTileViews = new List<TileView>();
        
        // Selection
        private TileView selectedTile;

        // Events
        public System.Action<TileInstance> OnTileDiscarded;

        private void Awake()
        {
            if (tileContainer == null)
            {
                tileContainer = transform;
            }
        }

        #region Display Hand

        /// <summary>
        /// Displays a hand with tiles
        /// </summary>
        public void DisplayHand(Hand hand)
        {
            currentHand = hand;
            RefreshDisplay();
        }

        /// <summary>
        /// Refreshes the entire display
        /// </summary>
        public void RefreshDisplay()
        {
            if (currentHand == null) return;

            DisplayConcealedTiles();
            DisplayBonusTiles();
        }

        private void DisplayConcealedTiles()
        {
            // Clear existing tiles
            ClearTileViews(concealedTileViews);

            // Get sorted tiles
            var tiles = new List<TileInstance>(currentHand.GetConcealedTiles());
            
            // Create tile view for each
            for (int i = 0; i < tiles.Count; i++)
            {
                TileView tileView = CreateTileView(tiles[i], tileContainer, i, tileSpacing);
                tileView.OnTileClicked += OnTileClicked;
                concealedTileViews.Add(tileView);
            }
        }

        private void DisplayBonusTiles()
        {
            if (bonusTileContainer == null) return;

            // Clear existing bonus tiles
            ClearTileViews(bonusTileViews);

            // Get bonus tiles
            var bonusTiles = currentHand.GetBonusTiles();
            
            // Create tile view for each
            for (int i = 0; i < bonusTiles.Count; i++)
            {
                TileView tileView = CreateTileView(bonusTiles[i], bonusTileContainer, i, bonusTileSpacing);
                // Bonus tiles are not clickable
                bonusTileViews.Add(tileView);
            }
        }

        private TileView CreateTileView(TileInstance tile, Transform parent, int index, float spacing)
        {
            GameObject tileObj = Instantiate(tilePrefab, parent);
            
            // Position
            RectTransform rectTransform = tileObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(index * spacing, 0);
            }

            // Get sprite
            Sprite sprite = spriteLibrary.GetSprite(tile.Data);

            // Setup view
            TileView tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(tile, sprite, faceUp: true);
            }

            return tileView;
        }

        private void ClearTileViews(List<TileView> tileViews)
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

        #endregion

        #region Selection

        private void OnTileClicked(TileView clickedTile)
        {
            // Toggle selection
            if (selectedTile == clickedTile)
            {
                DeselectTile();
            }
            else
            {
                SelectTile(clickedTile);
            }
        }

        private void SelectTile(TileView tileView)
        {
            // Deselect previous
            if (selectedTile != null)
            {
                DeselectTile();
            }

            // Select new
            selectedTile = tileView;
            selectedTile.SetHighlight(true);

            // Move tile up
            RectTransform rectTransform = selectedTile.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 pos = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = new Vector2(pos.x, pos.y + selectedOffset);
            }
        }

        private void DeselectTile()
        {
            if (selectedTile == null) return;

            selectedTile.SetHighlight(false);

            // Move tile back down
            RectTransform rectTransform = selectedTile.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 pos = rectTransform.anchoredPosition;
                rectTransform.anchoredPosition = new Vector2(pos.x, pos.y - selectedOffset);
            }

            selectedTile = null;
        }

        /// <summary>
        /// Gets the currently selected tile instance
        /// </summary>
        public TileInstance GetSelectedTile()
        {
            return selectedTile?.GetTileInstance();
        }

        #endregion

        #region Actions

        /// <summary>
        /// Discards the currently selected tile
        /// </summary>
        public void DiscardSelectedTile()
        {
            if (selectedTile == null)
            {
                Debug.LogWarning("No tile selected to discard");
                return;
            }

            TileInstance tileToDiscard = selectedTile.GetTileInstance();
            
            // Remove from hand
            currentHand.RemoveTile(tileToDiscard);
            
            // Deselect
            DeselectTile();
            
            // Refresh display
            RefreshDisplay();
            
            // Notify listeners
            OnTileDiscarded?.Invoke(tileToDiscard);
            
            Debug.Log($"Discarded: {tileToDiscard.Data}");
        }

        /// <summary>
        /// Adds a tile to the hand (when drawing)
        /// </summary>
        public void AddTile(TileInstance tile)
        {
            currentHand.AddTile(tile);
            RefreshDisplay();
        }

        /// <summary>
        /// Clears the hand display
        /// </summary>
        public void Clear()
        {
            ClearTileViews(concealedTileViews);
            ClearTileViews(bonusTileViews);
            selectedTile = null;
        }

        #endregion

        #region Public Queries

        /// <summary>
        /// Gets the current hand
        /// </summary>
        public Hand GetHand()
        {
            return currentHand;
        }

        /// <summary>
        /// Returns true if a tile is selected
        /// </summary>
        public bool HasSelectedTile()
        {
            return selectedTile != null;
        }

        #endregion
    }
}
