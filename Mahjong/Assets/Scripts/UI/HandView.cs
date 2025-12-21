using UnityEngine;
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
        [SerializeField] private Transform bonusTileContainer;

        [Header("Settings")]
        [SerializeField] private bool isFaceUp = true;

        [Header("Layout")]
        [SerializeField] private float selectedOffset = 20f; // How much selected tile moves up

        // Current hand being displayed
        private Hand currentHand;
        private int seatIndex = 0;
        
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

        /// <summary>
        /// Flip all tiles face up
        /// </summary>
        public void RevealHand() {
            foreach (TileView tile in concealedTileViews) {
                tile.SetFaceUp(true);
            }
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
                TileView tileView = CreateTileView(tiles[i], tileContainer, i, isFaceUp);
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

            List<TileInstance> visibleTiles = new List<TileInstance>();
            visibleTiles.AddRange(bonusTiles);

            // Include exposed melds in the bonus area
            foreach (Meld meld in currentHand.GetExposedMelds()) {
                foreach (TileInstance tile in meld.Tiles) {
                    visibleTiles.Add(tile);
                }
            }
            
            // Create tile view for each
            for (int i = 0; i < visibleTiles.Count; i++)
            {
                TileView tileView = CreateTileView(visibleTiles[i], bonusTileContainer, i, true);
                // Bonus tiles are not clickable
                bonusTileViews.Add(tileView);
            }
        }

        private TileView CreateTileView(TileInstance tile, Transform parent, int index, bool isFaceUp)
        {
            GameObject tileObj = Instantiate(tilePrefab, parent);
            float totalSpace = parent.GetComponent<RectTransform>().rect.width;
            int maxHandSize = 14;
            float tileSizeNoSpacing = totalSpace / maxHandSize;
            float percentSpacing = 0.01f;
            float tileSpacing = tileSizeNoSpacing * percentSpacing;
            float tileSizeWithSpacing = (totalSpace - tileSpacing * (maxHandSize - 1)) / maxHandSize;
            float spacing = tileSpacing + tileSizeWithSpacing;
            
            // Position
            RectTransform rectTransform = tileObj.GetComponent<RectTransform>();
            // rectTransform.localScale = new Vector3(.5f, .5f, 1);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = Vector2.zero;
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
                tileView.Setup(tile, sprite, isFaceUp);
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
            Debug.Log(clickedTile.GetTileInstance().Data);
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

        /// <summary>
        /// Set if the hand view's tiles are face up or not
        /// </summary>
        public void SetFaceUp(bool faceUp) {
            isFaceUp = faceUp;
        }

        /// <summary>
        /// Assign the seat index of this hand view
        /// </summary>
        public void SetSeatIndex(int index) {
            seatIndex = index;
        }

        /// <summary>
        /// Getter for seat index
        /// </summary>
        public int GetSeatIndex() {
            return seatIndex;
        }

        #endregion
    }
}
