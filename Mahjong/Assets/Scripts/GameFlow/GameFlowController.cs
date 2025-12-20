using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MJ.Core.Tiles;
using MJ.Core.Hand;
using MJ.Core.Wall;
using MJ.GameLogic;
using MJ.Rules;
using MJ.Evaluation;
using MJ.UI;
using MJ.Testing;
using MJ.Scoring;
using RoboRyanTron.Unite2017.Variables;

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

        [Header("UI References")]
        [SerializeField] private TableLayoutView tableLayoutView;
        [SerializeField] private ClaimButtonsUI claimButtonsUI;
        [SerializeField] private ClaimManager claimManager;
        [SerializeField] private WinScreenUI winScreenUI;

        [Header("Configuration")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int shuffleSeed = 12345; // Used when useRandomSeed is false
        [SerializeField] private FloatVariable activeSeat; // Which seat is currently being controlled

        [Header("Debug")]
        [SerializeField] private DebugControllerSO debugController;

        // Game components
        private Wall wall;
        private List<Hand> playerHands;
        private ActionValidator actionValidator;
        private IMahjongRuleSet ruleSet;
        private IScoringSystem scoringSystem;

        // Current game state
        private List<TileInstance> discardPile;
        
        // Track if waiting for player input
        private bool waitingForPlayerDiscard = false;
        
        // Random number generator for AI (seeded for reproducibility)
        private System.Random aiRandom;

        private void Awake()
        {
            // Initialize rule set
            ruleSet = new HongKongRules();
            
            // Initialize validator
            actionValidator = new ActionValidator(ruleSet);

            // Initialize scoring system
            scoringSystem = new HongKongScoring();

            // Initialize hands
            playerHands = new List<Hand>();
            for (int i = 0; i < 4; i++)
            {
                playerHands.Add(new Hand());
            }

            discardPile = new List<TileInstance>();

            // Setup claim manager
            if (claimManager != null)
            {
                claimManager.SetActionValidator(actionValidator);
                claimManager.OnClaimWindowOpened += OnClaimWindowOpened;
                claimManager.OnClaimWindowClosed += OnClaimWindowClosed;
                claimManager.OnClaimResolved += OnClaimResolved;
            }

            // Setup claim buttons UI
            if (claimButtonsUI != null)
            {
                claimButtonsUI.OnPongClaimed += () => OnPlayerClaim(ClaimType.Pong);
                claimButtonsUI.OnKongClaimed += () => OnPlayerClaim(ClaimType.Kong);
                claimButtonsUI.OnChowClaimed += () => OnPlayerClaim(ClaimType.Chow);
                claimButtonsUI.OnWinClaimed += () => OnPlayerClaim(ClaimType.Win);
                claimButtonsUI.OnPassClaimed += OnPlayerPass;
            }

            // Setup win screen UI
            if (winScreenUI != null)
            {
                winScreenUI.OnNextHandClicked += StartNextHand;
                winScreenUI.OnQuitClicked += QuitGame;
            }
        }

        #region Public API

        /// <summary>
        /// Starts a new game
        /// </summary>
        [ContextMenu("Start New Game")]
        public void StartGame()
        {
            activeSeat.Value = 0;
            stateManager.StartNewGame();

            DebugLog("=== GAME STARTED ===", debugController.StartGame);
            
            // Start first hand
            StartNextHand();
    
            // Subscribe to active seat's discard event
            if (tableLayoutView != null)
            {
                tableLayoutView.SetCurrentTurn(0);
                HandView activeView = tableLayoutView.GetPlayerHandView((int)activeSeat.Value);
                if (activeView != null)
                {
                    activeView.OnTileDiscarded += OnPlayerDiscardedTile;
                }
            }
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
            
            // Get a tile to discard - use seeded random for AI
            var tiles = new List<TileInstance>(hand.GetConcealedTiles());
            if (tiles.Count > 0)
            {
                TileInstance tileToDiscard;
                
                if (aiRandom != null)
                {
                    // Use seeded random (reproducible)
                    int randomIndex = aiRandom.Next(0, tiles.Count);
                    tileToDiscard = tiles[randomIndex];
                }
                else
                {
                    // Fallback to last tile
                    tileToDiscard = tiles[tiles.Count - 1];
                }
                
                DiscardTile(currentPlayer, tileToDiscard);
            }
            else
            {
                DebugLog($"Player {currentPlayer} has no tiles to discard!", true);
            }
        }

        #endregion

        #region Game Flow

        private void DealInitialHands()
        {
            DebugLog("Dealing tiles to players...", debugController.DealHands);

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
                
                playerHands[i].SortTiles(debugController.SortTiles);

                DebugLog($"Player {i} final hand: {playerHands[i].ConcealedTileCount} tiles (+ {playerHands[i].GetBonusTiles().Count} bonus)", debugController.DealHands);
                
                // Update display for this player
                if (tableLayoutView != null)
                {
                    tableLayoutView.UpdatePlayerHand(i, playerHands[i]);
                }
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
                DebugLog($"Player {playerIndex} got bonus tile: {tile.Data} - drawing replacement", debugController.BonusTileReplacement);
                
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
            DebugLog($"\n--- Player {currentPlayer}'s Turn ---", debugController.ChangeTurn);

            // Draw a tile (and handle any bonus tiles)
            TileInstance drawnTile = DrawAndHandleBonusTiles(currentPlayer);
            
            if (drawnTile == null)
            {
                DebugLog("Wall is empty! Hand ends in draw.", true);
                stateManager.CompleteHand(winnerIndex: -1, dealerWon: false);
                return;
            }

            // Add the non-bonus tile to hand
            playerHands[currentPlayer].AddTile(drawnTile);
            playerHands[currentPlayer].SortTiles(debugController.SortTiles);
            stateManager.SetTilesRemainingInWall(wall.DrawPileCount);

            DebugLog($"Player {currentPlayer} drew: {drawnTile.Data}", debugController.DrawTile);
            DebugLog($"Tiles in wall: {wall.DrawPileCount}", debugController.WallInfo);

            // Update UI for all players
            if (tableLayoutView != null)
            {
                tableLayoutView.UpdatePlayerHand(currentPlayer, playerHands[currentPlayer]);
            }

            // Check for win on draw
            if (HandEvaluator.CheckBasicWinPattern(playerHands[currentPlayer], meldsRequired: 4))
            {
                DebugLog($"★★★ Player {currentPlayer} WINS on self-draw! ★★★", true);
                playerHands[currentPlayer].DebugPrint();
                
                bool dealerWon = stateManager.State.IsDealer(currentPlayer);
                stateManager.CompleteHand(winnerIndex: currentPlayer, dealerWon: dealerWon);
                
                // Calculate score and show win screen
                ShowWinScreen(currentPlayer, playerHands[currentPlayer], isSelfDrawn: true);
                return;
            }

            // Show hand
            ShowPlayerHand(currentPlayer);
            
            // Handle discard based on whether it's the active seat's turn
            if (currentPlayer == activeSeat.Value)
            {
                // Human player - wait for them to click discard
                DebugLog($"Player {currentPlayer}: Select a tile and click Discard", debugController.HandleDiscard);
                waitingForPlayerDiscard = true;
            }
            else
            {
                // AI player - auto discard for now
                DebugLog($"Player {currentPlayer} must discard. Use 'Next Turn' to auto-discard.", debugController.HandleDiscard);
            }
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
                DebugLog($"Player {playerIndex} drew bonus tile: {drawnTile.Data} - drawing replacement from dead wall", debugController.BonusTileReplacement);
                
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
                DebugLog($"Cannot discard: {validation.Reason}", true);
                return;
            }

            // Remove from hand
            hand.RemoveTile(tileToDiscard);
            discardPile.Add(tileToDiscard);

            DebugLog($"Player {playerIndex} discarded: {tileToDiscard.Data}", debugController.DiscardTile);

            // Update Displayed Hand
            if (tableLayoutView != null)
            {
                tableLayoutView.UpdatePlayerHand(playerIndex, hand);
            }

            // Update discard pile display
            if (tableLayoutView != null)
            {
                tableLayoutView.AddDiscardedTile(tileToDiscard);
            }

            // Record discard
            stateManager.SetLastDiscardPlayer(playerIndex);

            // Open claim window instead of immediately advancing
            if (claimManager != null)
            {
                claimManager.OpenClaimWindow(tileToDiscard, playerIndex);
            }
            else
            {
                // No claim manager - just advance
                AdvanceToNextPlayer();
            }
        }

        private void AdvanceToNextPlayer()
        {
            stateManager.NextTurn();
            DrawTileForCurrentPlayer();

            // Update validator when turn changes
            actionValidator.UpdateGameState(stateManager.State);
            
            // Update turn indicator
            int newPlayerIndex = stateManager.GetCurrentTurn();
            if (tableLayoutView != null)
            {
                tableLayoutView.SetCurrentTurn(newPlayerIndex);
            }
            
            DebugLog($"Turn changed to Player {newPlayerIndex}", debugController.ChangeTurn);
        }

        #endregion

        #region Player Input Handlers

        private void OnPlayerDiscardedTile(TileInstance tile)
        {
            if (!waitingForPlayerDiscard)
            {
                DebugLog("Not waiting for player discard", debugController.DiscardTile);
                return;
            }

            int currentPlayer = stateManager.GetCurrentTurn();
            if (currentPlayer != activeSeat.Value)
            {
                DebugLog($"Not active seat's turn (active={activeSeat}, current={currentPlayer})", debugController.DiscardTile);
                return;
            }

            waitingForPlayerDiscard = false;

            // The tile was already removed from hand by HandView
            // Just add to discard pile and advance
            discardPile.Add(tile);
            DebugLog($"Player {currentPlayer} discarded: {tile.Data}", debugController.DiscardTile);
            // Update discard pile display
            if (tableLayoutView != null)
            {
                tableLayoutView.AddDiscardedTile(tile);
            }

            // Record discard
            stateManager.SetLastDiscardPlayer(currentPlayer);

            // Advance turn
            AdvanceToNextPlayer();
        }

        #endregion

        #region Claiming

        private void OnClaimWindowOpened(TileInstance discardedTile, int discardPlayerIndex)
        {
            DebugLog($"Claim window opened for {discardedTile.Data} from Player {discardPlayerIndex}", debugController.ClaimWindowOpened);

            // Check what the ACTIVE SEAT can claim
            ClaimOptions options = claimManager.GetValidClaims((int)activeSeat.Value, discardedTile.Data, playerHands[(int)activeSeat.Value]);

            DebugLog($"Claim options for Seat {activeSeat}: Pong={options.CanPong}, Kong={options.CanKong}, Chow={options.CanChow}, Win={options.CanWin}", debugController.ClaimOptions);

            // Show claim UI for human player
            if (claimButtonsUI != null && options.HasAnyClaim)
            {
                claimButtonsUI.ShowClaimOptions(
                    discardedTile.Data,
                    options.CanPong,
                    options.CanKong,
                    options.CanChow,
                    options.CanWin
                );
            }
            else if (claimButtonsUI != null)
            {
                // Can't claim anything - auto-pass
                DebugLog($"Seat {activeSeat} cannot claim - auto-passing", debugController.ClaimDecision);
                claimManager.SubmitPass((int)activeSeat.Value);
            }
            else
            {
                DebugLog("ERROR: claimButtonsUI is null!", debugController.ClaimDecision);
            }

            // TODO: Check AI players and auto-claim for them
            // For now, they auto-pass
        }

        private void OnClaimWindowClosed() {
            AdvanceToNextPlayer();
        }

        private void OnPlayerClaim(ClaimType claimType)
        {
            DebugLog($"Seat {activeSeat} claiming {claimType}", debugController.ClaimDecision);
            claimManager.SubmitClaim((int)activeSeat.Value, claimType);
        }

        private void OnPlayerPass()
        {
            DebugLog($"Seat {activeSeat} passed", debugController.ClaimDecision);
            claimManager.SubmitPass((int)activeSeat.Value);
        }

        private void OnClaimResolved(int winnerIndex, ClaimType claimType, TileInstance claimedTile)
        {
            DebugLog($"Claim resolved: Player {winnerIndex} claimed {claimType} with {claimedTile.Data}", debugController.ClaimResolved);

            // Form the meld based on claim type
            switch (claimType)
            {
                case ClaimType.Pong:
                    FormPongFromClaim(winnerIndex, claimedTile);
                    break;
                case ClaimType.Kong:
                    FormKongFromClaim(winnerIndex, claimedTile);
                    break;
                case ClaimType.Chow:
                    FormChowFromClaim(winnerIndex, claimedTile);
                    break;
                case ClaimType.Win:
                    HandleWinFromClaim(winnerIndex, claimedTile);
                    return; // Don't continue - game ends
            }

            // Update display
            if (tableLayoutView != null)
            {
                tableLayoutView.UpdatePlayerHand(winnerIndex, playerHands[winnerIndex]);
            }

            // Claimer's turn
            stateManager.SetTurn(winnerIndex);
            waitingForPlayerDiscard = (winnerIndex == 0);

            if (winnerIndex != 0)
            {
                // AI player - auto discard for now
                DebugLog($"Player {winnerIndex} must discard after claim", debugController.ClaimResolved);
            }
        }

        private void FormPongFromClaim(int playerIndex, TileInstance claimedTile)
        {
            Hand hand = playerHands[playerIndex];

            // Find 2 matching tiles in hand
            var concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            var matchingTiles = concealedTiles
                .Where(t => t.Data.IsSameType(claimedTile.Data))
                .Take(2)
                .ToList();

            if (matchingTiles.Count < 2)
            {
                DebugLog("ERROR: Not enough tiles to form Pong!", debugController.ClaimPong);
                return;
            }

            // Create meld
            var meldTiles = new List<TileInstance>(matchingTiles);
            meldTiles.Add(claimedTile);

            Meld pong = Meld.CreatePong(meldTiles, claimedTile, stateManager.State.LastDiscardPlayerIndex);

            if (pong != null)
            {
                // Remove tiles from hand
                foreach (var tile in matchingTiles)
                {
                    hand.RemoveTile(tile);
                }

                // Add exposed meld
                hand.AddExposedMeld(pong);

                DebugLog($"Player {playerIndex} formed Pong: {pong}", debugController.ClaimPong);
            }
        }

        private void FormKongFromClaim(int playerIndex, TileInstance claimedTile)
        {
            Hand hand = playerHands[playerIndex];

            // Find 3 matching tiles in hand
            var concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            var matchingTiles = concealedTiles
                .Where(t => t.Data.IsSameType(claimedTile.Data))
                .Take(3)
                .ToList();

            if (matchingTiles.Count < 3)
            {
                DebugLog("ERROR: Not enough tiles to form Kong!", debugController.ClaimKong);
                return;
            }

            // Create meld
            var meldTiles = new List<TileInstance>(matchingTiles);
            meldTiles.Add(claimedTile);

            Meld kong = Meld.CreateKong(meldTiles, false, claimedTile, stateManager.State.LastDiscardPlayerIndex);

            if (kong != null)
            {
                // Remove tiles from hand
                foreach (var tile in matchingTiles)
                {
                    hand.RemoveTile(tile);
                }

                // Add exposed meld
                hand.AddExposedMeld(kong);

                DebugLog($"Player {playerIndex} formed Kong: {kong}", debugController.ClaimKong);

                // Draw replacement tile
                TileInstance replacement = wall.DrawReplacementTile();
                if (replacement != null)
                {
                    hand.AddTile(replacement);
                    hand.SortTiles(debugController.SortTiles);
                    DebugLog($"Player {playerIndex} drew replacement: {replacement.Data}", debugController.BonusTileReplacement);
                }
            }
        }

        private void FormChowFromClaim(int playerIndex, TileInstance claimedTile)
        {
            Hand hand = playerHands[playerIndex];

            // Find tiles to complete the sequence
            var concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            
            // Try to find a valid chow combination
            List<TileInstance> chowTiles = FindChowTiles(concealedTiles, claimedTile);

            if (chowTiles == null || chowTiles.Count != 3)
            {
                DebugLog("ERROR: Cannot form valid Chow!", debugController.ClaimChow);
                return;
            }

            Meld chow = Meld.CreateChow(chowTiles, claimedTile, stateManager.State.LastDiscardPlayerIndex);

            if (chow != null)
            {
                // Remove tiles from hand (excluding claimed tile)
                foreach (var tile in chowTiles)
                {
                    if (tile != claimedTile)
                    {
                        hand.RemoveTile(tile);
                    }
                }

                // Add exposed meld
                hand.AddExposedMeld(chow);

                DebugLog($"Player {playerIndex} formed Chow: {chow}", debugController.ClaimChow);
            }
        }

        private List<TileInstance> FindChowTiles(List<TileInstance> concealedTiles, TileInstance claimedTile)
        {
            if (claimedTile.Data.IsHonor()) return null;

            int claimedNum = claimedTile.Data.Number;
            TileSuit suit = claimedTile.Data.Suit;

            // Try pattern: n-2, n-1, n (claimed is highest)
            var tile1 = concealedTiles.FirstOrDefault(t => t.Data.Suit == suit && t.Data.Number == claimedNum - 2);
            var tile2 = concealedTiles.FirstOrDefault(t => t.Data.Suit == suit && t.Data.Number == claimedNum - 1);
            if (tile1 != null && tile2 != null)
                return new List<TileInstance> { tile1, tile2, claimedTile };

            // Try pattern: n-1, n, n+1 (claimed is middle)
            tile1 = concealedTiles.FirstOrDefault(t => t.Data.Suit == suit && t.Data.Number == claimedNum - 1);
            tile2 = concealedTiles.FirstOrDefault(t => t.Data.Suit == suit && t.Data.Number == claimedNum + 1);
            if (tile1 != null && tile2 != null)
                return new List<TileInstance> { tile1, claimedTile, tile2 };

            // Try pattern: n, n+1, n+2 (claimed is lowest)
            tile1 = concealedTiles.FirstOrDefault(t => t.Data.Suit == suit && t.Data.Number == claimedNum + 1);
            tile2 = concealedTiles.FirstOrDefault(t => t.Data.Suit == suit && t.Data.Number == claimedNum + 2);
            if (tile1 != null && tile2 != null)
                return new List<TileInstance> { claimedTile, tile1, tile2 };

            return null;
        }

        private void HandleWinFromClaim(int winnerIndex, TileInstance claimedTile)
        {
            DebugLog($"★★★ Player {winnerIndex} WINS by claiming {claimedTile.Data}! ★★★", debugController.ClaimWin);
            
            // Add claimed tile to hand temporarily for scoring
            playerHands[winnerIndex].AddTile(claimedTile);
            
            bool dealerWon = stateManager.State.IsDealer(winnerIndex);
            stateManager.CompleteHand(winnerIndex, dealerWon);

            // Calculate score and show win screen
            ShowWinScreen(winnerIndex, playerHands[winnerIndex], isSelfDrawn: false);
        }

        #endregion

        #region Win Handling

        private void ShowWinScreen(int winnerIndex, Hand hand, bool isSelfDrawn)
        {
            if (winScreenUI == null)
            {
                DebugLog("No win screen UI - skipping display", true);
                return;
            }

            // Build scoring context
            TileInstance winningTile = null;
            var concealedTiles = hand.GetConcealedTiles();
            if (concealedTiles.Count > 0)
            {
                // Last tile in hand is the winning tile
                winningTile = concealedTiles[concealedTiles.Count - 1];
            }

            // Get player's wind
            WindType seatWind = stateManager.State.GetSeatWind(winnerIndex);
            WindType prevailingWind = stateManager.State.PrevailingWind;

            // Get bonus tiles
            var bonusTiles = hand.GetBonusTiles();

            // Build scoring context
            ScoringContext context = new ScoringContext
            {
                IsSelfDrawn = isSelfDrawn,
                WinningTile = winningTile?.Data ?? default,
                SeatWind = seatWind,
                PrevailingWind = prevailingWind,
                IsDealer = stateManager.State.IsDealer(winnerIndex),
                BonusTileCount = bonusTiles.Count,
                IsLastTileFromWall = false,
                IsReplacementTile = false,
                IsRobbingKong = false
            };

            // Calculate score
            ScoreResult scoreResult = scoringSystem.CalculateScore(hand, context);

            DebugLog($"Score calculated: {scoreResult.Points} points" + 
                     (scoreResult.Fan.HasValue ? $", {scoreResult.Fan.Value} fan" : ""), true);

            // Show win screen
            winScreenUI.ShowWinScreen(winnerIndex, hand, scoreResult, isSelfDrawn);
        }

        private void StartNextHand()
        {
            DebugLog("Starting next hand...", true);
            
            // Hide win screen
            if (winScreenUI != null)
            {
                winScreenUI.HideWinScreen();
            }

            // Clear discard pile display
            if (tableLayoutView != null)
            {
                tableLayoutView.ClearDiscardPile();
            }

            // Start new hand
            stateManager.StartNewHand();

            DebugLog("=== NEW HAND STARTED ===", debugController.StartHand);
            
            // Create and shuffle tiles
            List<TileInstance> allTiles = TileFactory.CreateFullTileSet(includeFlowersAndSeasons: true);
            DebugLog($"TileFactory: Created {allTiles.Count} tiles", debugController.TileCreation);
            
            // Shuffle with seed if specified
            int currentSeed;
            if (useRandomSeed)
            {
                currentSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                TileFactory.ShuffleTiles(allTiles);
                DebugLog("Shuffled with random seed", true);
            }
            else
            {
                currentSeed = shuffleSeed;
                TileFactory.ShuffleTiles(allTiles, shuffleSeed);
                DebugLog($"Shuffled with seed: {shuffleSeed}", debugController.Shuffle);
            }
            
            // Initialize AI random with same seed (for reproducible AI behavior)
            aiRandom = new System.Random(currentSeed);

            // Create wall
            wall = new Wall(allTiles, deadWallSize: 14);
            stateManager.SetTilesRemainingInWall(wall.DrawPileCount);

            // Deal tiles to players
            DealInitialHands();

            // Update labels with dealer/wind info
            if (tableLayoutView != null)
            {
                tableLayoutView.UpdateAllPlayerLabels(stateManager.State.DealerIndex);
            }

            // Update validator with current state
            actionValidator.UpdateGameState(stateManager.State);

            // Start first turn (dealer draws first)
            DrawTileForCurrentPlayer();
        }

        private void QuitGame()
        {
            DebugLog("Quitting game...", true);
            
            // Hide win screen
            if (winScreenUI != null)
            {
                winScreenUI.HideWinScreen();
            }

            // TODO: Return to main menu or quit application
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        #endregion

        #region Debug Helpers

        private void ShowPlayerHand(int playerIndex)
        {
            Hand hand = playerHands[playerIndex];
            var tiles = hand.GetConcealedTiles();
            
            DebugLog($"Player {playerIndex} hand ({tiles.Count} tiles):", debugController.ShowHand);
            
            // Group by suit for easier reading
            var grouped = hand.GetTilesBySuit();
            foreach (var kvp in grouped)
            {
                if (kvp.Value.Count > 0)
                {
                    string tilesStr = string.Join(", ", kvp.Value.Select(t => t.Data.ToCompactString()));
                    DebugLog($"  {kvp.Key}: {tilesStr}", debugController.ShowHand);
                }
            }
            
            // Show bonus tiles if any
            var bonusTiles = hand.GetBonusTiles();
            if (bonusTiles.Count > 0)
            {
                string bonusStr = string.Join(", ", bonusTiles.Select(t => t.Data.ToCompactString()));
                DebugLog($"  Bonus: {bonusStr}", debugController.ShowHand);
            }
        }

        private void DebugLog(string message, bool enabled)
        {
            if (enabled)
            {
                Debug.Log($"[GameFlow] {message}");
            }
        }

        public void SwitchToSeat()
        {
            int newSeatIndex = (int)activeSeat.Value;

            DebugLog($"[GameFlow] Switching to control Seat {newSeatIndex}", debugController.ChangeSeat);

            if (tableLayoutView != null)
            {
                // Tell TableLayoutView to rotate the view
                tableLayoutView.SwitchToSeat(newSeatIndex);
                
                // Update all labels to show correct seat numbers/winds
                tableLayoutView.UpdateAllPlayerLabels(stateManager.State.DealerIndex);
                
                // Refresh all hands - they'll now display at rotated positions
                for (int i = 0; i < playerHands.Count; i++)
                {
                    tableLayoutView.UpdatePlayerHand(i, playerHands[i]);
                }

                // Update turn indicator
                tableLayoutView.SetCurrentTurn(stateManager.GetCurrentTurn());
            }

            // Update event subscription
            // Unsubscribe from ALL seats first (to be safe)
            for (int i = 0; i < 4; i++)
            {
                HandView view = tableLayoutView.GetPlayerHandView(i);
                if (view != null)
                {
                    view.OnTileDiscarded -= OnPlayerDiscardedTile;
                }
            }
            
            // Subscribe to new active seat
            HandView newView = tableLayoutView.GetPlayerHandView(newSeatIndex);
            if (newView != null)
            {
                newView.OnTileDiscarded += OnPlayerDiscardedTile;
            }
            
            // Update waitingForPlayerDiscard based on whether active seat matches current turn
            int currentTurn = stateManager.GetCurrentTurn();
            waitingForPlayerDiscard = currentTurn == newSeatIndex;
            
            DebugLog($"Switched to Seat {newSeatIndex}. Current turn: Player {currentTurn}", debugController.ChangeSeat);
        }

        #endregion

        #region Testing

        [ContextMenu("Debug Print All Hands")]
        public void DebugPrintAllHands()
        {
            for (int i = 0; i < playerHands.Count; i++)
            {
                DebugLog($"\n=== Player {i} ===", true);
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
