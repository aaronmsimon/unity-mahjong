using System;
using MJ.Core.Tiles;

namespace MJ.GameLogic
{
    /// <summary>
    /// Immutable game state - each change creates a new instance
    /// Enables undo/replay, state history, and prevents accidental mutations
    /// </summary>
    [Serializable]
    public class GameState
    {
        // All fields are readonly - immutable after construction
        public readonly int DealerIndex;
        public readonly int CurrentTurnIndex;
        public readonly WindType PrevailingWind;
        public readonly int RoundNumber;
        public readonly int HandsPlayedInRound;
        public readonly int TotalHandsPlayed;
        public readonly int PlayerCount;
        public readonly bool IsGameActive;
        public readonly GamePhase CurrentPhase;
        public readonly int TilesRemainingInWall;
        public readonly int LastDiscardPlayerIndex;
        public readonly bool DealerRetainsSeat;

        /// <summary>
        /// Private constructor - only this class can create instances
        /// </summary>
        private GameState(
            int dealerIndex,
            int currentTurnIndex,
            WindType prevailingWind,
            int roundNumber,
            int handsPlayedInRound,
            int totalHandsPlayed,
            int playerCount,
            bool isGameActive,
            GamePhase currentPhase,
            int tilesRemainingInWall,
            int lastDiscardPlayerIndex,
            bool dealerRetainsSeat)
        {
            DealerIndex = dealerIndex;
            CurrentTurnIndex = currentTurnIndex;
            PrevailingWind = prevailingWind;
            RoundNumber = roundNumber;
            HandsPlayedInRound = handsPlayedInRound;
            TotalHandsPlayed = totalHandsPlayed;
            PlayerCount = playerCount;
            IsGameActive = isGameActive;
            CurrentPhase = currentPhase;
            TilesRemainingInWall = tilesRemainingInWall;
            LastDiscardPlayerIndex = lastDiscardPlayerIndex;
            DealerRetainsSeat = dealerRetainsSeat;
        }

        #region Factory Methods

        /// <summary>
        /// Creates initial game state
        /// </summary>
        public static GameState CreateInitialState(int playerCount = 4, bool dealerRetainsSeat = true)
        {
            return new GameState(
                dealerIndex: 0,
                currentTurnIndex: 0,
                prevailingWind: WindType.East,
                roundNumber: 0,
                handsPlayedInRound: 0,
                totalHandsPlayed: 0,
                playerCount: playerCount,
                isGameActive: false,
                currentPhase: GamePhase.NotStarted,
                tilesRemainingInWall: 0,
                lastDiscardPlayerIndex: -1,
                dealerRetainsSeat: dealerRetainsSeat
            );
        }

        #endregion

        #region Immutable Update Methods (Return new state)

        /// <summary>
        /// Creates a new state with the game started
        /// </summary>
        public GameState WithGameStarted()
        {
            return new GameState(
                dealerIndex: 0,
                currentTurnIndex: 0,
                prevailingWind: WindType.East,
                roundNumber: 0,
                handsPlayedInRound: 0,
                totalHandsPlayed: 0,
                playerCount: this.PlayerCount,
                isGameActive: true,
                currentPhase: GamePhase.Dealing,
                tilesRemainingInWall: this.TilesRemainingInWall,
                lastDiscardPlayerIndex: -1,
                dealerRetainsSeat: this.DealerRetainsSeat
            );
        }

        /// <summary>
        /// Creates a new state with updated phase
        /// </summary>
        public GameState WithPhase(GamePhase newPhase)
        {
            return new GameState(
                this.DealerIndex,
                this.CurrentTurnIndex,
                this.PrevailingWind,
                this.RoundNumber,
                this.HandsPlayedInRound,
                this.TotalHandsPlayed,
                this.PlayerCount,
                this.IsGameActive,
                newPhase,
                this.TilesRemainingInWall,
                this.LastDiscardPlayerIndex,
                this.DealerRetainsSeat
            );
        }

        /// <summary>
        /// Creates a new state with next turn
        /// </summary>
        public GameState WithNextTurn()
        {
            return new GameState(
                this.DealerIndex,
                (this.CurrentTurnIndex + 1) % this.PlayerCount,
                this.PrevailingWind,
                this.RoundNumber,
                this.HandsPlayedInRound,
                this.TotalHandsPlayed,
                this.PlayerCount,
                this.IsGameActive,
                GamePhase.PlayerTurn,
                this.TilesRemainingInWall,
                this.LastDiscardPlayerIndex,
                this.DealerRetainsSeat
            );
        }

        /// <summary>
        /// Creates a new state with specific turn
        /// </summary>
        public GameState WithTurn(int playerIndex)
        {
            return new GameState(
                this.DealerIndex,
                playerIndex,
                this.PrevailingWind,
                this.RoundNumber,
                this.HandsPlayedInRound,
                this.TotalHandsPlayed,
                this.PlayerCount,
                this.IsGameActive,
                GamePhase.PlayerTurn,
                this.TilesRemainingInWall,
                this.LastDiscardPlayerIndex,
                this.DealerRetainsSeat
            );
        }

        /// <summary>
        /// Creates a new state with next dealer
        /// </summary>
        public GameState WithNextDealer()
        {
            int newDealerIndex = (this.DealerIndex + 1) % this.PlayerCount;
            
            // Check if we need to advance prevailing wind (dealer wrapped to 0)
            bool shouldAdvanceWind = newDealerIndex == 0;
            
            return new GameState(
                newDealerIndex,
                this.CurrentTurnIndex,
                shouldAdvanceWind ? GetNextWind(this.PrevailingWind) : this.PrevailingWind,
                shouldAdvanceWind ? this.RoundNumber + 1 : this.RoundNumber,
                shouldAdvanceWind ? 0 : this.HandsPlayedInRound,
                this.TotalHandsPlayed,
                this.PlayerCount,
                this.IsGameActive,
                this.CurrentPhase,
                this.TilesRemainingInWall,
                this.LastDiscardPlayerIndex,
                this.DealerRetainsSeat
            );
        }

        /// <summary>
        /// Creates a new state with hand completed
        /// </summary>
        public GameState WithHandCompleted(bool dealerWon)
        {
            // Advance dealer unless dealer won and retains seat
            bool advanceDealer = !dealerWon || !this.DealerRetainsSeat;
            
            if (advanceDealer)
            {
                return WithNextDealer().WithHandCountIncremented();
            }
            else
            {
                return WithHandCountIncremented();
            }
        }

        /// <summary>
        /// Creates a new state with hand count incremented
        /// </summary>
        private GameState WithHandCountIncremented()
        {
            return new GameState(
                this.DealerIndex,
                this.CurrentTurnIndex,
                this.PrevailingWind,
                this.RoundNumber,
                this.HandsPlayedInRound + 1,
                this.TotalHandsPlayed + 1,
                this.PlayerCount,
                this.IsGameActive,
                GamePhase.HandComplete,
                this.TilesRemainingInWall,
                this.LastDiscardPlayerIndex,
                this.DealerRetainsSeat
            );
        }

        /// <summary>
        /// Creates a new state with updated wall tile count
        /// </summary>
        public GameState WithTilesInWall(int count)
        {
            return new GameState(
                this.DealerIndex,
                this.CurrentTurnIndex,
                this.PrevailingWind,
                this.RoundNumber,
                this.HandsPlayedInRound,
                this.TotalHandsPlayed,
                this.PlayerCount,
                this.IsGameActive,
                this.CurrentPhase,
                count,
                this.LastDiscardPlayerIndex,
                this.DealerRetainsSeat
            );
        }

        /// <summary>
        /// Creates a new state with last discard player recorded
        /// </summary>
        public GameState WithLastDiscardPlayer(int playerIndex)
        {
            return new GameState(
                this.DealerIndex,
                this.CurrentTurnIndex,
                this.PrevailingWind,
                this.RoundNumber,
                this.HandsPlayedInRound,
                this.TotalHandsPlayed,
                this.PlayerCount,
                this.IsGameActive,
                this.CurrentPhase,
                this.TilesRemainingInWall,
                playerIndex,
                this.DealerRetainsSeat
            );
        }

        /// <summary>
        /// Creates a new state with game ended
        /// </summary>
        public GameState WithGameEnded()
        {
            return new GameState(
                this.DealerIndex,
                this.CurrentTurnIndex,
                this.PrevailingWind,
                this.RoundNumber,
                this.HandsPlayedInRound,
                this.TotalHandsPlayed,
                this.PlayerCount,
                isGameActive: false,
                GamePhase.GameComplete,
                this.TilesRemainingInWall,
                this.LastDiscardPlayerIndex,
                this.DealerRetainsSeat
            );
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Gets the seat wind for a specific player index
        /// </summary>
        public WindType GetSeatWind(int playerIndex)
        {
            int relativePosition = (playerIndex - DealerIndex + PlayerCount) % PlayerCount;
            
            return relativePosition switch
            {
                0 => WindType.East,
                1 => WindType.South,
                2 => WindType.West,
                3 => WindType.North,
                _ => WindType.East
            };
        }

        /// <summary>
        /// Checks if a player is the dealer
        /// </summary>
        public bool IsDealer(int playerIndex)
        {
            return playerIndex == DealerIndex;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the next wind in sequence
        /// </summary>
        private static WindType GetNextWind(WindType currentWind)
        {
            return currentWind switch
            {
                WindType.East => WindType.South,
                WindType.South => WindType.West,
                WindType.West => WindType.North,
                WindType.North => WindType.East,
                _ => WindType.East
            };
        }

        #endregion

        #region Equality and ToString

        public override string ToString()
        {
            return $"GameState[Round:{RoundNumber}, Wind:{PrevailingWind}, Dealer:P{DealerIndex}, Turn:P{CurrentTurnIndex}, Phase:{CurrentPhase}]";
        }

        #endregion
    }

    /// <summary>
    /// Phases of gameplay
    /// </summary>
    public enum GamePhase
    {
        NotStarted,
        Dealing,
        PlayerTurn,
        WaitingForClaims,
        ResolvingClaims,
        HandComplete,
        RoundComplete,
        GameComplete
    }
}
