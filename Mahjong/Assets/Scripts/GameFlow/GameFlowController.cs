using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MJ.Core.Tiles;
using MJ.Core.Hand;
using MJ.Core.Wall;
using MJ.GameLogic;
using MJ.Rules;
using MJ.Evaluation;

namespace MJ.GameFlow
{
    /// <summary>
    /// Orchestrates the main game loop
    /// Connects all systems together to create a playable game
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameStateManager stateManager;
        [SerializeField] private GameEvents gameEvents;

        [Header("Configuration")]
        [SerializeField] private bool enableDebugLogging = true;

        // Game components
        private Wall wall;
        private List<Hand> playerHands;
        private ActionValidator actionValidator;
        private IMahjongRuleSet ruleSet;

        // Current game state
        private List<TileInstance> discardPile;

        private void Awake()
        {
            // Initialize rule set
            ruleSet = new HongKongRules();
            
            // Initialize validator
            actionValidator = new ActionValidator(ruleSet);

            // Initialize hands
            playerHands = new List<Hand>();
            for (int i = 0; i < 4; i++)
            {
                playerHands.Add(new Hand());
            }

            discardPile = new List<TileInstance>();

            // Subscribe to events
            if (gameEvents != null)
            {
                gameEvents.OnGameStarted.AddListener(OnGameStarted);
                gameEvents.OnHandStarted.AddListener(OnHandStarted);
                gameEvents.OnTurnChanged.AddListener(OnTurnChanged);
            }
        }

        #region Public API

        /// <summary>
        /// Starts a new game
        /// </summary>
        [ContextMenu("Start New Game")]
        public void StartGame()
        {
            stateManager.StartNewGame();
        }

        /// <summary>
        /// Manually advance turn (for testing)
        /// </summary>
        [ContextMenu("Next Turn")]
        public void NextTurn()
        {
            // Current player needs to discard before advancing
            int currentPlayer = stateManager.GetCurrentTurn();
            Hand hand = playerHands[currentPlayer];
            
            // Get a tile to discard (just take the last one for now)
            var tiles = new List<TileInstance>(hand.GetConcealedTiles());
            if (tiles.Count > 0)
            {
                TileInstance tileToDiscard = tiles[tiles.Count - 1];
                DiscardTile(currentPlayer, tileToDiscard);
            }
            else
            {
                DebugLog($"Player {currentPlayer} has no tiles to discard!");
            }
        }

        #endregion

        #region Event Handlers

        private void OnGameStarted()
        {
            DebugLog("=== GAME STARTED ===");
            
            // Start first hand
            stateManager.StartNewHand();
        }

        private void OnHandStarted()
        {
            DebugLog("=== NEW HAND STARTED ===");
            
            // Create and shuffle tiles
            List<TileInstance> allTiles = TileFactory.CreateFullTileSet(includeFlowersAndSeasons: true);
            TileFactory.ShuffleTiles(allTiles);

            // Create wall
            wall = new Wall(allTiles, deadWallSize: 14);
            stateManager.SetTilesRemainingInWall(wall.DrawPileCount);

            // Deal tiles to players
            DealInitialHands();

            // Update validator with current state
            actionValidator.UpdateGameState(stateManager.State);

            // Start first turn (dealer draws first)
            DrawTileForCurrentPlayer();
        }

        private void OnTurnChanged(int newPlayerIndex)
        {
            // Update validator when turn changes
            actionValidator.UpdateGameState(stateManager.State);
            
            DebugLog($"Turn changed to Player {newPlayerIndex}");
        }

        #endregion

        #region Game Flow

        private void DealInitialHands()
        {
            DebugLog("Dealing tiles to players...");

            // Deal 13 tiles to each player
            var dealtHands = wall.DealInitialHands(playerCount: 4, tilesPerPlayer: 13);

            for (int i = 0; i < 4; i++)
            {
                playerHands[i].Clear();
                
                // Add tiles one by one and handle bonus tiles immediately
                foreach (var tile in dealtHands[i])
                {
                    AddTileAndHandleBonuses(i, tile);
                }
                
                playerHands[i].SortTiles();

                DebugLog($"Player {i} final hand: {playerHands[i].ConcealedTileCount} tiles (+ {playerHands[i].GetBonusTiles().Count} bonus)");
            }

            // Update state
            stateManager.SetTilesRemainingInWall(wall.DrawPileCount);
        }

        /// <summary>
        /// Adds a tile to player's hand and handles bonus tiles recursively
        /// If the tile is a bonus, draws replacement and checks that too
        /// </summary>
        private void AddTileAndHandleBonuses(int playerIndex, TileInstance tile)
        {
            if (tile == null) return;

            if (tile.Data.IsBonus())
            {
                // Add bonus tile
                playerHands[playerIndex].AddTile(tile);
                DebugLog($"Player {playerIndex} got bonus tile: {tile.Data} - drawing replacement");
                
                // Draw replacement from dead wall
                TileInstance replacement = wall.DrawReplacementTile();
                stateManager.SetTilesRemainingInWall(wall.DrawPileCount);
                
                // Recursively handle the replacement (might also be bonus!)
                AddTileAndHandleBonuses(playerIndex, replacement);
            }
            else
            {
                // Normal tile - just add it
                playerHands[playerIndex].AddTile(tile);
            }
        }

        private void DrawTileForCurrentPlayer()
        {
            int currentPlayer = stateManager.GetCurrentTurn();
            DebugLog($"\n--- Player {currentPlayer}'s Turn ---");

            // Draw a tile (and handle any bonus tiles)
            TileInstance drawnTile = DrawAndHandleBonusTiles(currentPlayer);
            
            if (drawnTile == null)
            {
                DebugLog("Wall is empty! Hand ends in draw.");
                stateManager.CompleteHand(winnerIndex: -1, dealerWon: false);
                return;
            }

            // Add the non-bonus tile to hand
            playerHands[currentPlayer].AddTile(drawnTile);
            stateManager.SetTilesRemainingInWall(wall.DrawPileCount);

            DebugLog($"Player {currentPlayer} drew: {drawnTile.Data}");
            DebugLog($"Tiles in wall: {wall.DrawPileCount}");

            // Check for win on draw
            if (HandEvaluator.CheckBasicWinPattern(playerHands[currentPlayer], meldsRequired: 4))
            {
                DebugLog($"★★★ Player {currentPlayer} WINS on self-draw! ★★★");
                playerHands[currentPlayer].DebugPrint();
                
                bool dealerWon = stateManager.State.IsDealer(currentPlayer);
                stateManager.CompleteHand(winnerIndex: currentPlayer, dealerWon: dealerWon);
                return;
            }

            // Show hand
            ShowPlayerHand(currentPlayer);
            
            DebugLog($"Player {currentPlayer} must discard. Use 'Next Turn' to auto-discard.");
        }

        /// <summary>
        /// Draws tiles and handles bonus tiles automatically
        /// Returns the first non-bonus tile drawn
        /// </summary>
        private TileInstance DrawAndHandleBonusTiles(int playerIndex)
        {
            TileInstance drawnTile = wall.DrawTile();
            
            if (drawnTile == null)
            {
                return null;
            }

            // If it's a bonus tile, add it and draw replacement
            while (drawnTile != null && drawnTile.Data.IsBonus())
            {
                playerHands[playerIndex].AddTile(drawnTile); // Hand will separate it automatically
                DebugLog($"Player {playerIndex} drew bonus tile: {drawnTile.Data} - drawing replacement from dead wall");
                
                // Draw replacement from dead wall
                drawnTile = wall.DrawReplacementTile();
                stateManager.SetTilesRemainingInWall(wall.DrawPileCount);
            }

            return drawnTile;
        }

        private void DiscardTile(int playerIndex, TileInstance tileToDiscard)
        {
            Hand hand = playerHands[playerIndex];

            // Update validator state before validation
            actionValidator.UpdateGameState(stateManager.State);

            // Validate discard
            var validation = actionValidator.CanDiscard(playerIndex, tileToDiscard.Data, hand);
            if (!validation.IsValid)
            {
                DebugLog($"Cannot discard: {validation.Reason}");
                return;
            }

            // Remove from hand
            hand.RemoveTile(tileToDiscard);
            discardPile.Add(tileToDiscard);

            DebugLog($"Player {playerIndex} discarded: {tileToDiscard.Data}");

            // Record discard
            stateManager.SetLastDiscardPlayer(playerIndex);

            // For now, skip claim window and just advance turn
            // In full implementation, would open claim window here
            AdvanceToNextPlayer();
        }

        private void AdvanceToNextPlayer()
        {
            stateManager.NextTurn();
            DrawTileForCurrentPlayer();
        }

        #endregion

        #region Debug Helpers

        private void ShowPlayerHand(int playerIndex)
        {
            if (!enableDebugLogging) return;

            Hand hand = playerHands[playerIndex];
            var tiles = hand.GetConcealedTiles();
            
            DebugLog($"Player {playerIndex} hand ({tiles.Count} tiles):");
            
            // Group by suit for easier reading
            var grouped = hand.GetTilesBySuit();
            foreach (var kvp in grouped)
            {
                if (kvp.Value.Count > 0)
                {
                    string tilesStr = string.Join(", ", kvp.Value.Select(t => t.Data.ToCompactString()));
                    DebugLog($"  {kvp.Key}: {tilesStr}");
                }
            }
            
            // Show bonus tiles if any
            var bonusTiles = hand.GetBonusTiles();
            if (bonusTiles.Count > 0)
            {
                string bonusStr = string.Join(", ", bonusTiles.Select(t => t.Data.ToCompactString()));
                DebugLog($"  Bonus: {bonusStr}");
            }
        }

        private void DebugLog(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[GameFlow] {message}");
            }
        }

        #endregion

        #region Testing

        [ContextMenu("Debug Print All Hands")]
        public void DebugPrintAllHands()
        {
            for (int i = 0; i < playerHands.Count; i++)
            {
                DebugLog($"\n=== Player {i} ===");
                ShowPlayerHand(i);
            }
        }

        [ContextMenu("Debug Print Game State")]
        public void DebugPrintGameState()
        {
            stateManager.DebugPrintState();
        }

        #endregion
    }
}
