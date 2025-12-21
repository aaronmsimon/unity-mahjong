using UnityEngine;
using System.Collections.Generic;
using TMPro;
using MJ.Core.Tiles;
using MJ.Core.Hand;

namespace MJ.UI
{
    /// <summary>
    /// Manages the overall table layout showing all 4 players and discard pile
    /// 
    /// **VIEW ROTATION SYSTEM:**
    /// - Active seat (controlled player) always appears at BOTTOM
    /// - Other seats rotate around the table: Right (next), Top (opposite), Left (previous)
    /// - When switching seats (debug or multiplayer), view rotates to keep active player at bottom
    /// 
    /// Physical positions: Bottom (active player), Right (next), Top (opposite), Left (previous)
    /// Logical seats: 0-3 (game seats, never change)
    /// </summary>
    public class TableLayoutView : MonoBehaviour
    {
        [Header("Player Hand Views - Physical Positions")]
        [SerializeField] private HandView bottomPlayerHand;  // Active player (rotates)
        [SerializeField] private HandView rightPlayerHand;   // Next player clockwise
        [SerializeField] private HandView topPlayerHand;     // Opposite player
        [SerializeField] private HandView leftPlayerHand;    // Previous player

        [Header("Discard Pile")]
        [SerializeField] private Transform discardPileContainer;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private TileSpriteLibrarySO spriteLibrary;

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
        
        // This determines how the view rotates - active seat always shows at bottom
        private int activeSeatIndex = 0;
        
        // We need to know which Hand object is at each visual position
        // so we can refresh displays when view rotates
        private Hand[] handsAtPositions = new Hand[4]; // [bottom, right, top, left]

        private void Awake()
        {
            // Setup initial player labels (will update when seat switches)
            UpdateAllPlayerLabels(0); // Assume dealer starts at seat 0
        }

        #region Hand Display

        /// <summary>
        /// Updates a player's hand display which maps logical seat → visual position based on active seat
        /// </summary>
        public void UpdatePlayerHand(int seatIndex, Hand hand)
        {
            // Get which visual position this seat should display at
            HandView view = GetHandViewForSeat(seatIndex);
            int visualPosition = GetVisualPositionForSeat(seatIndex);
            
            if (view != null)
            {
                // Cache the hand for this position
                handsAtPositions[visualPosition] = hand;
                
                // Update the display
                view.DisplayHand(hand);
                
                // Set face-up only if this is the active seat
                view.SetFaceUp(seatIndex == activeSeatIndex);
            }
        }

        /// <summary>
        /// Maps logical seat → HandView based on rotation**
        /// 
        /// Example: If activeSeatIndex = 2
        /// - Seat 2 → bottom (0 positions away from active)
        /// - Seat 3 → right  (1 position away)
        /// - Seat 0 → top    (2 positions away)
        /// - Seat 1 → left   (3 positions away)
        /// </summary>
        private HandView GetHandViewForSeat(int seatIndex)
        {
            int visualPosition = GetVisualPositionForSeat(seatIndex);
            
            return visualPosition switch
            {
                0 => bottomPlayerHand,  // Active player
                1 => rightPlayerHand,   // Next player clockwise
                2 => topPlayerHand,     // Opposite player
                3 => leftPlayerHand,    // Previous player
                _ => null
            };
        }

        /// <summary>
        /// Calculates visual position from logical seat
        /// </summary>
        private int GetVisualPositionForSeat(int seatIndex)
        {
            // How many seats clockwise from active seat?
            return (seatIndex - activeSeatIndex + 4) % 4;
        }

        /// <summary>
        /// Calculates logical seat from visual position
        /// </summary>
        private int GetSeatForVisualPosition(int visualPosition)
        {
            return (activeSeatIndex + visualPosition) % 4;
        }

        /// <summary>
        /// Gets the HandView for a specific player
        /// </summary>
        public HandView GetPlayerHandView(int seatIndex)
        {
            // Only return HandView if this is the active seat
            // (the one that should be interactive)
            if (seatIndex == activeSeatIndex)
                return bottomPlayerHand;
            return null;
        }

        /// <summary>
        /// Refresh all hand displays
        /// </summary>
        public void UpdateAllPlayerHands()
        {
            // Refresh each visual position using its cached hand
            if (handsAtPositions[0] != null && bottomPlayerHand != null)
                bottomPlayerHand.RefreshDisplay();
            if (handsAtPositions[1] != null && rightPlayerHand != null)
                rightPlayerHand.RefreshDisplay();
            if (handsAtPositions[2] != null && topPlayerHand != null)
                topPlayerHand.RefreshDisplay();
            if (handsAtPositions[3] != null && leftPlayerHand != null)
                leftPlayerHand.RefreshDisplay();
        }

        #endregion

        #region Discard Pile

        /// <summary>
        /// Adds a discarded tile to the center pile
        /// </summary>
        public void AddDiscardedTile(TileInstance tile)
        {
            if (discardPileContainer == null || tilePrefab == null || spriteLibrary == null) return;

            // Create tile view
            GameObject tileObj = Instantiate(tilePrefab, discardPileContainer);

            // Get sprite and setup
            Sprite sprite = spriteLibrary.GetSprite(tile.Data);
            TileView tileView = tileObj.GetComponent<TileView>();
            
            if (tileView != null)
            {
                tileView.Setup(tile, sprite, faceUp: true);
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
        public void SetCurrentTurn(int seatIndex)
        {
            // Hide all indicators
            if (turnIndicatorBottom != null) turnIndicatorBottom.SetActive(false);
            if (turnIndicatorRight != null) turnIndicatorRight.SetActive(false);
            if (turnIndicatorTop != null) turnIndicatorTop.SetActive(false);
            if (turnIndicatorLeft != null) turnIndicatorLeft.SetActive(false);

            // Calculate which visual position this seat is at
            int visualPosition = GetVisualPositionForSeat(seatIndex);

            // Show indicator at that position
            switch (visualPosition)
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

        #region Seat Controls

        /// <summary>
        /// Switches active seat and rotates view**
        /// </summary>
        public void SwitchToSeat(int newSeatIndex)
        {
            if (newSeatIndex < 0 || newSeatIndex > 3) return;
            if (newSeatIndex == activeSeatIndex) return; // Already at this seat
            
            Debug.Log($"[TableLayoutView] Switching from Seat {activeSeatIndex} to Seat {newSeatIndex}");
            
            // Store old active seat for logging
            int oldSeatIndex = activeSeatIndex;
            
            // Update active seat
            activeSeatIndex = newSeatIndex;
            
            // Now we need to redistribute all hands to new visual positions
            // Because the view has rotated, each hand needs to move to its new position
            
            // Strategy: We'll tell GameFlowController to refresh all hands
            // which will call UpdatePlayerHand() for each seat, and our new
            // GetHandViewForSeat() logic will put them in the right places
            
            // For now, just update face-up/face-down states for each position
            // The actual hand data will be refreshed by GameFlowController
            
            // Bottom is always face-up (active seat)
            if (bottomPlayerHand != null)
                bottomPlayerHand.SetFaceUp(true);
            
            // All others are face-down
            if (rightPlayerHand != null)
                rightPlayerHand.SetFaceUp(false);
            if (topPlayerHand != null)
                topPlayerHand.SetFaceUp(false);
            if (leftPlayerHand != null)
                leftPlayerHand.SetFaceUp(false);
            
            Debug.Log($"[TableLayoutView] View rotated - Seat {newSeatIndex} now at bottom (face-up)");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Show seat numbers relative to visual positions**
        /// </summary>
        public void UpdateAllPlayerLabels(int dealerIndex)
        {
            string[] winds = { "East", "South", "West", "North" };
            
            // Update label for each visual position
            for (int visualPos = 0; visualPos < 4; visualPos++)
            {
                // Which seat is at this visual position?
                int seatIndex = GetSeatForVisualPosition(visualPos);
                
                // Calculate wind for this seat
                int windIndex = (seatIndex - dealerIndex + 4) % 4;
                string windText = winds[windIndex];
                
                // Build label text
                string labelText;
                if (seatIndex == activeSeatIndex)
                {
                    // Active seat shows "You"
                    labelText = $"You - Seat {seatIndex} ({windText})";
                }
                else
                {
                    // Other seats show seat number
                    labelText = $"Seat {seatIndex} ({windText})";
                }
                
                // Set the label at this visual position
                TMP_Text label = GetLabelForVisualPosition(visualPos);
                if (label != null)
                {
                    label.text = labelText;
                }
            }
        }

        /// <summary>
        /// Gets label at visual position**
        /// </summary>
        private TMP_Text GetLabelForVisualPosition(int visualPosition)
        {
            return visualPosition switch
            {
                0 => bottomPlayerLabel,
                1 => rightPlayerLabel,
                2 => topPlayerLabel,
                3 => leftPlayerLabel,
                _ => null
            };
        }

        /// <summary>
        /// Gets which seat is currently active/controlled
        /// </summary>
        public int GetActiveSeat()
        {
            return activeSeatIndex;
        }

        #endregion
    }
}
