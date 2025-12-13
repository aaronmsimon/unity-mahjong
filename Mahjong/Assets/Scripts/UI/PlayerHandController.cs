using UnityEngine;
using UnityEngine.UI;
using MJ.Core.Tiles;
using MJ.Core.Hand;
using MJ.GameLogic;

namespace MJ.UI
{
    /// <summary>
    /// Controls the player's hand UI and handles user input
    /// Bridges between HandView (UI) and GameFlowController (logic)
    /// </summary>
    public class PlayerHandController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandView handView;
        [SerializeField] private Button discardButton;
        
        [Header("Game References")]
        [SerializeField] private GameStateManager stateManager;

        // Current player index this controller represents
        private int playerIndex = 0; // Default to player 0 (human player)

        private void Awake()
        {
            // Setup discard button
            if (discardButton != null)
            {
                discardButton.onClick.AddListener(OnDiscardButtonClicked);
                UpdateDiscardButton();
            }

            // Subscribe to hand view events
            if (handView != null)
            {
                handView.OnTileDiscarded += OnTileDiscarded;
            }
        }

        private void Update()
        {
            UpdateDiscardButton();
        }

        #region Setup

        /// <summary>
        /// Sets which player this controller represents
        /// </summary>
        public void SetPlayerIndex(int index)
        {
            playerIndex = index;
        }

        /// <summary>
        /// Sets the hand to display
        /// </summary>
        public void SetHand(Hand hand)
        {
            if (handView != null)
            {
                handView.DisplayHand(hand);
            }
        }

        #endregion

        #region Input Handling

        private void OnDiscardButtonClicked()
        {
            if (!CanDiscard())
            {
                Debug.LogWarning("Cannot discard right now");
                return;
            }

            handView.DiscardSelectedTile();
        }

        private void OnTileDiscarded(TileInstance tile)
        {
            Debug.Log($"Player {playerIndex} discarded {tile.Data}");
            
            // This event will be picked up by GameFlowController
            // For now, just log it
        }

        #endregion

        #region UI Updates

        private void UpdateDiscardButton()
        {
            if (discardButton == null) return;

            bool canDiscard = CanDiscard();
            discardButton.interactable = canDiscard;

            // Update button text
            Text buttonText = discardButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                if (!IsPlayerTurn())
                {
                    buttonText.text = "Not Your Turn";
                }
                else if (!handView.HasSelectedTile())
                {
                    buttonText.text = "Select a Tile";
                }
                else
                {
                    buttonText.text = "Discard";
                }
            }
        }

        private bool CanDiscard()
        {
            // Check if it's player's turn
            if (!IsPlayerTurn()) return false;

            // Check if a tile is selected
            if (!handView.HasSelectedTile()) return false;

            // Check if in correct phase
            if (stateManager != null && stateManager.State.CurrentPhase != GamePhase.PlayerTurn)
            {
                return false;
            }

            return true;
        }

        private bool IsPlayerTurn()
        {
            if (stateManager == null) return true;
            if (stateManager.State == null) return true;
            return stateManager.State.CurrentTurnIndex == playerIndex;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Adds a tile to the hand (when drawing)
        /// </summary>
        public void AddTile(TileInstance tile)
        {
            handView.AddTile(tile);
        }

        /// <summary>
        /// Gets the hand view
        /// </summary>
        public HandView GetHandView()
        {
            return handView;
        }

        /// <summary>
        /// Gets the current hand
        /// </summary>
        public Hand GetHand()
        {
            return handView.GetHand();
        }

        #endregion
    }
}
