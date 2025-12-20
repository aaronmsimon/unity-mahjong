using UnityEngine;
using System.Collections.Generic;
using MJ.Core.Tiles;
using MJ.Testing;

namespace MJ.GameLogic
{
    /// <summary>
    /// Manages immutable game state with full history tracking
    /// Supports undo/redo and state replay
    /// Does NOT know about players, hands, or tiles directly
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int playerCount = 4;
        [SerializeField] private bool dealerRetainsSeat = true;
        [SerializeField] private bool enableStateHistory = true;
        [SerializeField] private int maxHistorySize = 1000;

        [Header("Debug")]
        [SerializeField] private DebugControllerSO debugController;

        // Current immutable state
        private GameState currentState;
        
        // State history for undo/redo/replay
        private List<GameStateChange> stateHistory;
        private int historyIndex; // For undo/redo

        public GameState State => currentState;
        public IReadOnlyList<GameStateChange> History => stateHistory?.AsReadOnly();

        private void Awake()
        {
            if (enableStateHistory)
            {
                stateHistory = new List<GameStateChange>();
                historyIndex = -1;
            }

            // Initialize with default state
            currentState = GameState.CreateInitialState(playerCount, dealerRetainsSeat);
        }

        #region State Transition (Private - Creates new state)

        /// <summary>
        /// Transitions to a new state and records the change
        /// </summary>
        private void TransitionTo(GameState newState, string actionType, string description)
        {
            GameState previousState = currentState;
            currentState = newState;

            // Record change in history
            if (enableStateHistory)
            {
                RecordStateChange(previousState, newState, actionType, description);
            }

            if (debugController.Transition) Debug.Log($"State Transition: {description}");
        }

        /// <summary>
        /// Records a state change in history
        /// </summary>
        private void RecordStateChange(GameState previous, GameState newState, string actionType, string description)
        {
            // If we're in the middle of history (after undo), discard "future" states
            if (historyIndex < stateHistory.Count - 1)
            {
                stateHistory.RemoveRange(historyIndex + 1, stateHistory.Count - historyIndex - 1);
            }

            // Add new change
            var change = new GameStateChange(previous, newState, actionType, description);
            stateHistory.Add(change);
            historyIndex = stateHistory.Count - 1;

            // Limit history size
            if (stateHistory.Count > maxHistorySize)
            {
                stateHistory.RemoveAt(0);
                historyIndex--;
            }

            if (debugController.History) Debug.Log($"History: {change}");
        }

        #endregion

        #region Game Initialization

        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartNewGame()
        {
            GameState newState = currentState.WithGameStarted();
            TransitionTo(newState, "GameStart", "New game started");
        }

        /// <summary>
        /// Starts a new hand
        /// </summary>
        public void StartNewHand()
        {
            GameState newState = currentState.WithPhase(GamePhase.Dealing);
            newState = newState.WithTurn(currentState.DealerIndex);
            
            TransitionTo(newState, "HandStart", $"Hand started. Dealer: P{newState.DealerIndex}");
        }

        /// <summary>
        /// Ends the current game
        /// </summary>
        public void EndGame()
        {
            GameState newState = currentState.WithGameEnded();
            TransitionTo(newState, "GameEnd", "Game ended");
        }

        #endregion

        #region Turn Management

        /// <summary>
        /// Advances to the next player's turn
        /// </summary>
        public void NextTurn()
        {
            GameState newState = currentState.WithNextTurn();
            TransitionTo(newState, "TurnAdvance", $"Turn advanced to P{newState.CurrentTurnIndex}");
        }

        /// <summary>
        /// Sets a specific player's turn
        /// </summary>
        public void SetTurn(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= currentState.PlayerCount)
            {
                Debug.LogError($"Invalid player index: {playerIndex}");
                return;
            }

            GameState newState = currentState.WithTurn(playerIndex);
            TransitionTo(newState, "TurnSet", $"Turn set to P{playerIndex}");
        }

        #endregion

        #region Phase Management

        /// <summary>
        /// Changes the current game phase
        /// </summary>
        public void SetPhase(GamePhase newPhase)
        {
            GameState newState = currentState.WithPhase(newPhase);
            TransitionTo(newState, "PhaseChange", $"Phase changed to {newPhase}");
        }

        /// <summary>
        /// Transitions to dealing phase
        /// </summary>
        public void BeginDealingPhase()
        {
            SetPhase(GamePhase.Dealing);
        }

        /// <summary>
        /// Opens claim window
        /// </summary>
        public void OpenClaimWindow()
        {
            GameState newState = currentState.WithPhase(GamePhase.WaitingForClaims);
            TransitionTo(newState, "ClaimWindowOpen", "Claim window opened");
        }

        /// <summary>
        /// Closes claim window
        /// </summary>
        public void CloseClaimWindow()
        {
            GameState newState = currentState.WithPhase(GamePhase.PlayerTurn);
            TransitionTo(newState, "ClaimWindowClose", "Claim window closed");
        }

        #endregion

        #region Round/Hand Completion

        /// <summary>
        /// Completes current hand
        /// </summary>
        public void CompleteHand(int winnerIndex = -1, bool dealerWon = false)
        {
            GameState newState = currentState.WithHandCompleted(dealerWon);
            string description = winnerIndex >= 0 
                ? $"Hand completed. Winner: P{winnerIndex}" 
                : "Hand completed. Draw.";
            
            TransitionTo(newState, "HandComplete", description);
        }

        #endregion

        #region Undo/Redo

        /// <summary>
        /// Undoes the last state change
        /// </summary>
        public bool Undo()
        {
            if (!enableStateHistory || historyIndex <= 0)
            {
                Debug.LogWarning("Cannot undo - no history available");
                return false;
            }

            historyIndex--;
            currentState = stateHistory[historyIndex].NewState;

            Debug.Log($"Undo: Restored to {stateHistory[historyIndex]}");
            return true;
        }

        /// <summary>
        /// Redoes the next state change
        /// </summary>
        public bool Redo()
        {
            if (!enableStateHistory || historyIndex >= stateHistory.Count - 1)
            {
                Debug.LogWarning("Cannot redo - no future history");
                return false;
            }

            historyIndex++;
            currentState = stateHistory[historyIndex].NewState;

            Debug.Log($"Redo: Applied {stateHistory[historyIndex]}");
            return true;
        }

        /// <summary>
        /// Clears state history
        /// </summary>
        public void ClearHistory()
        {
            if (enableStateHistory)
            {
                stateHistory.Clear();
                historyIndex = -1;
            }
        }

        #endregion

        #region State Queries

        public int GetCurrentTurn() => currentState.CurrentTurnIndex;
        public int GetDealer() => currentState.DealerIndex;
        public WindType GetSeatWind(int playerIndex) => currentState.GetSeatWind(playerIndex);
        public WindType GetPrevailingWind() => currentState.PrevailingWind;
        public bool IsPlayerTurn(int playerIndex) => currentState.CurrentTurnIndex == playerIndex;
        public GamePhase GetCurrentPhase() => currentState.CurrentPhase;

        /// <summary>
        /// Updates wall tile count
        /// </summary>
        public void SetTilesRemainingInWall(int count)
        {
            GameState newState = currentState.WithTilesInWall(count);
            TransitionTo(newState, "WallUpdate", $"Tiles in wall: {count}");
        }

        /// <summary>
        /// Records last discard player
        /// </summary>
        public void SetLastDiscardPlayer(int playerIndex)
        {
            GameState newState = currentState.WithLastDiscardPlayer(playerIndex);
            TransitionTo(newState, "DiscardRecord", $"P{playerIndex} discarded");
        }

        #endregion

        #region Debugging

        public void DebugPrintState()
        {
            Debug.Log("=== Game State ===");
            Debug.Log(currentState.ToString());
            Debug.Log($"Hands This Round: {currentState.HandsPlayedInRound}");
            Debug.Log($"Total Hands: {currentState.TotalHandsPlayed}");
            Debug.Log($"Tiles in Wall: {currentState.TilesRemainingInWall}");
            Debug.Log($"History Size: {stateHistory?.Count ?? 0}");
        }

        public void DebugPrintHistory(int lastN = 10)
        {
            if (!enableStateHistory || stateHistory == null)
            {
                Debug.Log("History not enabled");
                return;
            }

            Debug.Log($"=== State History (last {lastN}) ===");
            int start = Mathf.Max(0, stateHistory.Count - lastN);
            for (int i = start; i < stateHistory.Count; i++)
            {
                string marker = i == historyIndex ? " <- CURRENT" : "";
                Debug.Log($"{i}: {stateHistory[i]}{marker}");
            }
        }

        #endregion
    }
}
