using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using MJ.Core.Tiles;
using MJ.Core.Hand;

namespace MJ.UI
{
    /// <summary>
    /// Manages the overall table layout showing all 4 players and discard pile
    /// Positions: Bottom (Player 0 - Human), Right (Player 1), Top (Player 2), Left (Player 3)
    /// </summary>
    public class TableLayoutView : MonoBehaviour
    {
        [Header("Player Hand Views")]
        [SerializeField] private HandView bottomPlayerHand;  // Player 0 (Human)
        [SerializeField] private OpponentHandView rightPlayerHand;   // Player 1
        [SerializeField] private OpponentHandView topPlayerHand;     // Player 2
        [SerializeField] private OpponentHandView leftPlayerHand;    // Player 3

        [Header("Discard Pile")]
        [SerializeField] private Transform discardPileContainer;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private TileSpriteLibrarySO spriteLibrary;
        [SerializeField] private float discardTileSpacing = 60f;
        [SerializeField] private int discardTilesPerRow = 6;

        [Header("Turn Indicator")]
        [SerializeField] private GameObject turnIndicatorBottom;
        [SerializeField] private GameObject turnIndicatorRight;
        [SerializeField] private GameObject turnIndicatorTop;
        [SerializeField] private GameObject turnIndicatorLeft;

        [Header("Player Labels")]
        [SerializeField] private TMP_Text bottomPlayerLabel;
        [SerializeField] private TMP_Text rightPlayerLabel;
        [SerializeField] private TMP_Text topPlayerLabel;
        [SerializeField] private TMP_Text leftPlayerLabel;

        private List<TileView> discardPileTileViews = new List<TileView>();

        private void Awake()
        {
            // Setup player labels
            SetPlayerLabel(bottomPlayerLabel, "You (East)", 0);
            SetPlayerLabel(rightPlayerLabel, "Player 1 (South)", 1);
            SetPlayerLabel(topPlayerLabel, "Player 2 (West)", 2);
            SetPlayerLabel(leftPlayerLabel, "Player 3 (North)", 3);
        }

        #region Hand Display

        /// <summary>
        /// Updates a player's hand display
        /// </summary>
        public void UpdatePlayerHand(int playerIndex, Hand hand)
        {
            switch (playerIndex)
            {
                case 0:
                    if (bottomPlayerHand != null)
                        bottomPlayerHand.DisplayHand(hand);
                    break;
                case 1:
                    if (rightPlayerHand != null)
                        rightPlayerHand.UpdateHandDisplay(hand.ConcealedTileCount);
                    break;
                case 2:
                    if (topPlayerHand != null)
                        topPlayerHand.UpdateHandDisplay(hand.ConcealedTileCount);
                    break;
                case 3:
                    if (leftPlayerHand != null)
                        leftPlayerHand.UpdateHandDisplay(hand.ConcealedTileCount);
                    break;
            }
        }

        /// <summary>
        /// Gets the HandView for a specific player (for human player)
        /// </summary>
        public HandView GetPlayerHandView(int playerIndex)
        {
            if (playerIndex == 0)
                return bottomPlayerHand;
            return null;
        }

        #endregion

        #region Discard Pile

        /// <summary>
        /// Adds a discarded tile to the center pile
        /// </summary>
        public void AddDiscardedTile(TileInstance tile)
        {
            if (discardPileContainer == null || tilePrefab == null || spriteLibrary == null)
                return;

            int index = discardPileTileViews.Count;
            
            // Calculate position in grid
            int row = index / discardTilesPerRow;
            int col = index % discardTilesPerRow;

            // Create tile view
            GameObject tileObj = Instantiate(tilePrefab, discardPileContainer);
            RectTransform rectTransform = tileObj.GetComponent<RectTransform>();
            
            if (rectTransform != null)
            {
                // Center the grid and offset from center
                float xOffset = (col - discardTilesPerRow / 2f) * discardTileSpacing;
                float yOffset = -row * discardTileSpacing;
                rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
            }

            // Get sprite and setup
            Sprite sprite = spriteLibrary.GetSprite(tile.Data);
            TileView tileView = tileObj.GetComponent<TileView>();
            
            if (tileView != null)
            {
                tileView.Setup(tile, sprite, faceUp: true);
                // Disable button for discard pile tiles
                Button button = tileObj.GetComponent<Button>();
                if (button != null)
                    button.interactable = false;
            }

            discardPileTileViews.Add(tileView);
        }

        /// <summary>
        /// Clears the discard pile
        /// </summary>
        public void ClearDiscardPile()
        {
            foreach (var tileView in discardPileTileViews)
            {
                if (tileView != null)
                    Destroy(tileView.gameObject);
            }
            discardPileTileViews.Clear();
        }

        #endregion

        #region Turn Indicator

        /// <summary>
        /// Updates which player's turn it is
        /// </summary>
        public void SetCurrentTurn(int playerIndex)
        {
            // Hide all indicators
            if (turnIndicatorBottom != null) turnIndicatorBottom.SetActive(false);
            if (turnIndicatorRight != null) turnIndicatorRight.SetActive(false);
            if (turnIndicatorTop != null) turnIndicatorTop.SetActive(false);
            if (turnIndicatorLeft != null) turnIndicatorLeft.SetActive(false);

            // Show current player's indicator
            switch (playerIndex)
            {
                case 0:
                    if (turnIndicatorBottom != null) turnIndicatorBottom.SetActive(true);
                    break;
                case 1:
                    if (turnIndicatorRight != null) turnIndicatorRight.SetActive(true);
                    break;
                case 2:
                    if (turnIndicatorTop != null) turnIndicatorTop.SetActive(true);
                    break;
                case 3:
                    if (turnIndicatorLeft != null) turnIndicatorLeft.SetActive(true);
                    break;
            }
        }

        #endregion

        #region Helper Methods

        private void SetPlayerLabel(TMP_Text label, string text, int playerIndex)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        /// <summary>
        /// Updates player labels with wind positions
        /// </summary>
        public void UpdatePlayerLabels(int dealerIndex)
        {
            string[] winds = { "East", "South", "West", "North" };
            
            for (int i = 0; i < 4; i++)
            {
                int windIndex = (i - dealerIndex + 4) % 4;
                string windText = winds[windIndex];
                string playerText = i == 0 ? "You" : $"Player {i}";
                
                TMP_Text label = GetPlayerLabel(i);
                if (label != null)
                {
                    label.text = $"{playerText} ({windText})";
                }
            }
        }

        private TMP_Text GetPlayerLabel(int playerIndex)
        {
            return playerIndex switch
            {
                0 => bottomPlayerLabel,
                1 => rightPlayerLabel,
                2 => topPlayerLabel,
                3 => leftPlayerLabel,
                _ => null
            };
        }

        #endregion
    }
}
