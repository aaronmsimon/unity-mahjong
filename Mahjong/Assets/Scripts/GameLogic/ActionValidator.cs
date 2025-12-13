using System.Collections.Generic;
using System.Linq;
using MJ.Core.Hand;
using MJ.Core.Tiles;
using MJ.Evaluation;
using MJ.Rules;

namespace MJ.GameLogic
{
    /// <summary>
    /// Validates whether players can perform specific actions
    /// Uses injected rule set for rule-specific validation
    /// Generic validator that works with any rule set
    /// </summary>
    public class ActionValidator
    {
        private readonly IMahjongRuleSet ruleSet;
        private GameState currentState;

        public ActionValidator(IMahjongRuleSet ruleSet)
        {
            this.ruleSet = ruleSet;
        }

        /// <summary>
        /// Updates the current game state
        /// Call this whenever game state changes
        /// </summary>
        public void UpdateGameState(GameState state)
        {
            currentState = state;
        }

        #region Turn Actions

        /// <summary>
        /// Validates if a player can discard a tile
        /// </summary>
        public ActionValidationResult CanDiscard(int playerIndex, TileData tile, Hand hand)
        {
            // Must be player's turn
            if (!IsPlayerTurn(playerIndex))
            {
                return ActionValidationResult.Fail("Not player's turn");
            }

            // Must be in correct phase
            if (currentState.CurrentPhase != GamePhase.PlayerTurn)
            {
                return ActionValidationResult.Fail($"Cannot discard in {currentState.CurrentPhase} phase");
            }

            // Must have the tile in hand
            if (!hand.HasTileOfType(tile))
            {
                return ActionValidationResult.Fail("Tile not in hand");
            }

            return ActionValidationResult.Success();
        }

        /// <summary>
        /// Validates if a player can declare a concealed Kong
        /// </summary>
        public ActionValidationResult CanDeclareKong(int playerIndex, Hand hand)
        {
            // Must be player's turn
            if (!IsPlayerTurn(playerIndex))
            {
                return ActionValidationResult.Fail("Not player's turn");
            }

            // Must have 4 matching tiles for concealed Kong
            if (!hand.CanFormConcealedKong())
            {
                return ActionValidationResult.Fail("No valid Kong in hand");
            }

            return ActionValidationResult.Success();
        }

        /// <summary>
        /// Validates if a player can declare a win on self-draw
        /// </summary>
        public ActionValidationResult CanDeclareWinOnDraw(int playerIndex, Hand hand, TileInstance drawnTile)
        {
            // Must be player's turn
            if (!IsPlayerTurn(playerIndex))
            {
                return ActionValidationResult.Fail("Not player's turn");
            }

            // Must be in player turn phase
            if (currentState.CurrentPhase != GamePhase.PlayerTurn)
            {
                return ActionValidationResult.Fail("Cannot declare win in this phase");
            }

            // Check if hand is winning with drawn tile
            if (!HandEvaluator.CheckBasicWinPatternWithTile(hand, drawnTile, GetMeldsRequired()))
            {
                return ActionValidationResult.Fail("Hand is not winning");
            }

            // Rule-specific validation (e.g., minimum fan in Hong Kong)
            // This would require extending IMahjongRuleSet with validation methods
            // For now, we'll assume basic pattern check is sufficient

            return ActionValidationResult.Success();
        }

        #endregion

        #region Claim Actions

        /// <summary>
        /// Validates if a player can claim a discarded tile for Pong
        /// </summary>
        public ActionValidationResult CanClaimForPong(int playerIndex, TileData discardedTile, Hand hand)
        {
            // Cannot claim own discard
            if (playerIndex == currentState.LastDiscardPlayerIndex)
            {
                return ActionValidationResult.Fail("Cannot claim own discard");
            }

            // Must be in claim window
            if (currentState.CurrentPhase != GamePhase.WaitingForClaims)
            {
                return ActionValidationResult.Fail("Claim window not open");
            }

            // Must have 2 matching tiles
            var concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            if (!MeldValidator.CanClaimForPong(concealedTiles, discardedTile))
            {
                return ActionValidationResult.Fail("Not enough matching tiles for Pong");
            }

            return ActionValidationResult.Success();
        }

        /// <summary>
        /// Validates if a player can claim a discarded tile for Kong
        /// </summary>
        public ActionValidationResult CanClaimForKong(int playerIndex, TileData discardedTile, Hand hand)
        {
            // Cannot claim own discard
            if (playerIndex == currentState.LastDiscardPlayerIndex)
            {
                return ActionValidationResult.Fail("Cannot claim own discard");
            }

            // Must be in claim window
            if (currentState.CurrentPhase != GamePhase.WaitingForClaims)
            {
                return ActionValidationResult.Fail("Claim window not open");
            }

            // Must have 3 matching tiles
            var concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            if (!MeldValidator.CanClaimForKong(concealedTiles, discardedTile))
            {
                return ActionValidationResult.Fail("Not enough matching tiles for Kong");
            }

            return ActionValidationResult.Success();
        }

        /// <summary>
        /// Validates if a player can claim a discarded tile for Chow
        /// </summary>
        public ActionValidationResult CanClaimForChow(int playerIndex, TileData discardedTile, Hand hand)
        {
            // Cannot claim own discard
            if (playerIndex == currentState.LastDiscardPlayerIndex)
            {
                return ActionValidationResult.Fail("Cannot claim own discard");
            }

            // Must be in claim window
            if (currentState.CurrentPhase != GamePhase.WaitingForClaims)
            {
                return ActionValidationResult.Fail("Claim window not open");
            }

            // In Hong Kong rules, can only Chow from left player
            // This is rule-specific - could be injected via rule set
            if (!IsLeftPlayer(playerIndex, currentState.LastDiscardPlayerIndex))
            {
                return ActionValidationResult.Fail("Can only Chow from left player (Hong Kong rules)");
            }

            // Must be able to form a Chow
            var concealedTiles = new List<TileInstance>(hand.GetConcealedTiles());
            if (!MeldValidator.CanClaimForChow(concealedTiles, discardedTile))
            {
                return ActionValidationResult.Fail("Cannot form Chow with this tile");
            }

            return ActionValidationResult.Success();
        }

        /// <summary>
        /// Validates if a player can claim a discarded tile for win
        /// </summary>
        public ActionValidationResult CanClaimForWin(int playerIndex, TileData discardedTile, Hand hand)
        {
            // Cannot claim own discard for win
            if (playerIndex == currentState.LastDiscardPlayerIndex)
            {
                return ActionValidationResult.Fail("Cannot win on own discard");
            }

            // Must be in claim window
            if (currentState.CurrentPhase != GamePhase.WaitingForClaims)
            {
                return ActionValidationResult.Fail("Claim window not open");
            }

            // Check if hand wins with this tile
            if (!HandEvaluator.CanWinWithDiscard(hand, discardedTile, GetMeldsRequired()))
            {
                return ActionValidationResult.Fail("Hand does not win with this tile");
            }

            return ActionValidationResult.Success();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if it's the specified player's turn
        /// </summary>
        private bool IsPlayerTurn(int playerIndex)
        {
            return currentState != null && currentState.CurrentTurnIndex == playerIndex;
        }

        /// <summary>
        /// Checks if player is to the left of the discard player
        /// (In seat order: 0, 1, 2, 3 - player 1 is left of player 0)
        /// </summary>
        private bool IsLeftPlayer(int playerIndex, int discardPlayerIndex)
        {
            int leftPlayerIndex = (discardPlayerIndex + 1) % currentState.PlayerCount;
            return playerIndex == leftPlayerIndex;
        }

        /// <summary>
        /// Gets the number of melds required based on rule set
        /// This is a placeholder - ideally RuleSet would provide this
        /// </summary>
        private int GetMeldsRequired()
        {
            // Default to Hong Kong (4 melds)
            // In a full implementation, this would come from ruleSet
            return ruleSet.HandSize == 16 ? 5 : 4;
        }

        #endregion

        #region Priority Handling

        /// <summary>
        /// Determines which claim has priority when multiple players claim
        /// Win > Kong > Pong > Chow (standard priority)
        /// </summary>
        public int GetClaimPriority(ClaimType claimType)
        {
            return claimType switch
            {
                ClaimType.Win => 0,    // Highest priority
                ClaimType.Kong => 1,
                ClaimType.Pong => 2,
                ClaimType.Chow => 3,   // Lowest priority
                _ => 99
            };
        }

        /// <summary>
        /// Resolves multiple claims to determine which one proceeds
        /// Returns the player index who gets to claim
        /// </summary>
        public int ResolveClaims(List<PendingClaim> claims)
        {
            if (claims == null || claims.Count == 0)
            {
                return -1;
            }

            // Sort by priority (lower number = higher priority)
            claims.Sort((a, b) => GetClaimPriority(a.Type).CompareTo(GetClaimPriority(b.Type)));

            // If top priority is unique, return that player
            if (claims.Count == 1 || GetClaimPriority(claims[0].Type) < GetClaimPriority(claims[1].Type))
            {
                return claims[0].PlayerIndex;
            }

            // If multiple players have same priority (e.g., 2 people want Pong)
            // Closest to discarder in turn order wins
            var samePriorityClaims = claims
                .Where(c => GetClaimPriority(c.Type) == GetClaimPriority(claims[0].Type))
                .ToList();

            return GetClosestPlayerToDiscarder(samePriorityClaims);
        }

        /// <summary>
        /// Gets the player closest to discarder in turn order
        /// </summary>
        private int GetClosestPlayerToDiscarder(List<PendingClaim> claims)
        {
            int discardPlayer = currentState.LastDiscardPlayerIndex;
            int closestDistance = currentState.PlayerCount;
            int closestPlayer = -1;

            foreach (var claim in claims)
            {
                int distance = (claim.PlayerIndex - discardPlayer + currentState.PlayerCount) % currentState.PlayerCount;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = claim.PlayerIndex;
                }
            }

            return closestPlayer;
        }

        #endregion
    }

    /// <summary>
    /// Types of claims
    /// </summary>
    public enum ClaimType
    {
        Win,
        Kong,
        Pong,
        Chow
    }

    /// <summary>
    /// Represents a pending claim from a player
    /// </summary>
    public class PendingClaim
    {
        public int PlayerIndex { get; set; }
        public ClaimType Type { get; set; }
        public TileData ClaimedTile { get; set; }

        public PendingClaim(int playerIndex, ClaimType type, TileData claimedTile)
        {
            PlayerIndex = playerIndex;
            Type = type;
            ClaimedTile = claimedTile;
        }
    }
}
