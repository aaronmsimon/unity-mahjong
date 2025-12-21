using UnityEngine;
using System.Collections.Generic;
using MJ.Core.Tiles;
using MJ.Core.Hand;
using MJ.GameLogic;

namespace MJ.GameFlow
{
    /// <summary>
    /// Manages the claiming process - collecting claims from players and resolving priority
    /// </summary>
    public class ClaimManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameStateManager stateManager;

        [Header("Configuration")]
        [SerializeField] private float claimWindowDuration = 5f;
        [Tooltip("If false, claim window stays open until player makes a decision")]
        [SerializeField] private bool autoCloseWindow = true;

        // Current claim state
        private bool isClaimWindowOpen;
        private TileInstance lastDiscardedTile;
        private int lastDiscardPlayerIndex;
        private List<PendingClaim> pendingClaims;
        private float claimWindowTimer;
        
        // Reference to action validator (set by GameFlowController)
        private ActionValidator actionValidator;

        // Events
        public System.Action<TileInstance, int> OnClaimWindowOpened; // tile, discarder index
        public System.Action OnClaimWindowClosed;
        public System.Action<int, ClaimType, TileInstance> OnClaimResolved; // winner index, claim type, tile

        private void Awake()
        {
            pendingClaims = new List<PendingClaim>();
        }

        /// <summary>
        /// Sets the action validator (called by GameFlowController)
        /// </summary>
        public void SetActionValidator(ActionValidator validator)
        {
            actionValidator = validator;
        }

        private void Update()
        {
            if (isClaimWindowOpen && autoCloseWindow)
            {
                claimWindowTimer -= Time.deltaTime;
                if (claimWindowTimer <= 0)
                {
                    // Time's up - resolve claims
                    ResolveClaims();
                }
            }
        }

        #region Claim Window

        /// <summary>
        /// Opens the claim window for a discarded tile
        /// </summary>
        public void OpenClaimWindow(TileInstance discardedTile, int discardPlayerIndex)
        {
            lastDiscardedTile = discardedTile;
            lastDiscardPlayerIndex = discardPlayerIndex;
            pendingClaims.Clear();
            claimWindowTimer = claimWindowDuration;
            isClaimWindowOpen = true;

            stateManager.OpenClaimWindow();
            OnClaimWindowOpened?.Invoke(discardedTile, discardPlayerIndex);

            Debug.Log($"Claim window opened for {discardedTile.Data} from Player {discardPlayerIndex}");
        }

        /// <summary>
        /// Closes the claim window
        /// </summary>
        public void CloseClaimWindow()
        {
            isClaimWindowOpen = false;
            stateManager.CloseClaimWindow();
            OnClaimWindowClosed?.Invoke();
        }

        #endregion

        #region Submit Claims

        /// <summary>
        /// Player submits a claim for the discarded tile
        /// </summary>
        public void SubmitClaim(int playerIndex, ClaimType claimType)
        {
            if (!isClaimWindowOpen)
            {
                Debug.LogWarning("Cannot submit claim - window is closed");
                return;
            }

            if (playerIndex == lastDiscardPlayerIndex)
            {
                Debug.LogWarning("Cannot claim own discard");
                return;
            }

            // Add to pending claims
            PendingClaim claim = new PendingClaim(playerIndex, claimType, lastDiscardedTile.Data);
            pendingClaims.Add(claim);

            Debug.Log($"Player {playerIndex} claimed {claimType}");

            // For single-player game, resolve immediately
            // In multiplayer, would wait for all players
            ResolveClaims();
        }

        /// <summary>
        /// Player passes on claiming
        /// </summary>
        public void SubmitPass(int playerIndex)
        {
            Debug.Log($"Player {playerIndex} passed");

            // In single-player against AI, if human passes, close window
            // In multiplayer, would track all passes
            if (playerIndex == 0)
            {
                ResolveClaims();
            }
        }

        #endregion

        #region Claim Resolution

        /// <summary>
        /// Resolves all pending claims using priority
        /// </summary>
        private void ResolveClaims()
        {
            if (pendingClaims.Count == 0)
            {
                // No claims - notify with sentinal value (-1)
                Debug.Log("No claims - continuing");
                OnClaimResolved?.Invoke(-1, ClaimType.Pong, lastDiscardedTile); // -1 = no winner, ClaimType doesn't matter
                CloseClaimWindow();
                return;
            }

            // Use ActionValidator to determine winner
            int winningPlayerIndex = actionValidator.ResolveClaims(pendingClaims);
            
            if (winningPlayerIndex < 0)
            {
                Debug.LogError("Failed to resolve claims");
                CloseClaimWindow();
                return;
            }

            // Find the winning claim
            PendingClaim winningClaim = pendingClaims.Find(c => c.PlayerIndex == winningPlayerIndex);
            
            if (winningClaim == null)
            {
                Debug.LogError("Could not find winning claim");
                CloseClaimWindow();
                return;
            }

            Debug.Log($"Claim resolved: Player {winningPlayerIndex} wins with {winningClaim.Type}");

            // Notify listeners
            OnClaimResolved?.Invoke(winningPlayerIndex, winningClaim.Type, lastDiscardedTile);

            CloseClaimWindow();
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// Checks what claims are valid for a player
        /// </summary>
        public ClaimOptions GetValidClaims(int playerIndex, TileData discardedTile, Hand hand)
        {
            if (actionValidator == null)
            {
                return new ClaimOptions();
            }

            actionValidator.UpdateGameState(stateManager.State);

            return new ClaimOptions
            {
                CanPong = actionValidator.CanClaimForPong(playerIndex, discardedTile, hand).IsValid,
                CanKong = actionValidator.CanClaimForKong(playerIndex, discardedTile, hand).IsValid,
                CanChow = actionValidator.CanClaimForChow(playerIndex, discardedTile, hand).IsValid,
                CanWin = actionValidator.CanClaimForWin(playerIndex, discardedTile, hand).IsValid
            };
        }

        #endregion

        #region Public Queries

        public bool IsClaimWindowOpen => isClaimWindowOpen;
        public TileInstance GetLastDiscardedTile() => lastDiscardedTile;
        public int GetLastDiscardPlayerIndex() => lastDiscardPlayerIndex;

        #endregion
    }

    /// <summary>
    /// Struct to hold what claims are valid for a player
    /// </summary>
    public struct ClaimOptions
    {
        public bool CanPong;
        public bool CanKong;
        public bool CanChow;
        public bool CanWin;

        public bool HasAnyClaim => CanPong || CanKong || CanChow || CanWin;
    }
}
