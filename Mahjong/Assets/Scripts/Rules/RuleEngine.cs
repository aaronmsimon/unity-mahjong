using System.Collections.Generic;
using System;
using MJ.Player;
using MJ.Game;
using MJ.Core;

namespace MJ.Rules
{
    public static class RuleEngine
    {
        /// <summary>
        /// Returns the list of legal actions for a given seat, based on the current GameState.
        /// For now:
        /// - Only CurrentSeat may act.
        /// - In Draw phase: Draw (if wall not empty).
        /// - In Discard phase: Discard any tile in hand. DeclareWin is stubbed in.
        /// </summary>
        public static List<PlayerAction> GetLegalActions(GameState state, int seat)
        {
            var actions = new List<PlayerAction>();

            if (state == null) throw new ArgumentNullException(nameof(state));
            if (seat < 0 || seat >= state.Players.Count) return actions;

            if (state.IsRoundOver)
                return actions;

            if (seat != state.CurrentSeat)
                return actions;

            var player = state.GetPlayer(seat);

            switch (state.TurnPhase)
            {
                case TurnPhase.Draw:
                    if (state.WallIndex < state.Wall.Count)
                    {
                        actions.Add(PlayerAction.Draw(seat));
                    }
                    break;

                case TurnPhase.Discard:
                    // Discard any tile from hand is allowed
                    foreach (var tile in player.Hand)
                    {
                        actions.Add(PlayerAction.Discard(seat, tile));
                    }

                    // Stub: always allow DeclareWin to be visible for now.
                    // Later, gate this on HandEvaluator.IsWinningHand(...)
                    actions.Add(PlayerAction.DeclareWin(seat));
                    break;
            }

            return actions;
        }

        /// <summary>
        /// Applies a PlayerAction to the GameState, mutating it.
        /// Assumes the action is legal; callers should normally use GetLegalActions first.
        /// </summary>
        public static void ApplyAction(GameState state, PlayerAction action)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (state.IsRoundOver)
                return;

            if (action.Seat != state.CurrentSeat)
            {
                throw new InvalidOperationException(
                    $"It is not seat {action.Seat}'s turn. CurrentSeat={state.CurrentSeat}");
            }

            var player = state.GetPlayer(action.Seat);

            switch (action.Type)
            {
                case ActionType.Draw:
                    ApplyDraw(state, player);
                    break;

                case ActionType.Discard:
                    ApplyDiscard(state, player, action.Tile);
                    break;

                case ActionType.DeclareWin:
                    ApplyDeclareWin(state, player);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(action.Type), $"Unknown action type {action.Type}");
            }
        }

        private static void ApplyDraw(GameState state, PlayerState player)
        {
            if (state.TurnPhase != TurnPhase.Draw)
            {
                throw new InvalidOperationException($"Cannot draw in phase {state.TurnPhase}");
            }

            if (state.WallIndex >= state.Wall.Count)
            {
                // Wall exhausted → round ends in a draw (for now).
                state.IsRoundOver = true;
                // WinnerSeat stays -1.
                return;
            }

            var tile = state.Wall[state.WallIndex++];
            player.AddToHand(tile);
            player.SortHand();

            // After drawing, player must discard.
            state.TurnPhase = TurnPhase.Discard;
        }

        private static void ApplyDiscard(GameState state, PlayerState player, TileInstance tile)
        {
            if (state.TurnPhase != TurnPhase.Discard)
            {
                throw new InvalidOperationException($"Cannot discard in phase {state.TurnPhase}");
            }

            if (tile == null)
            {
                throw new ArgumentNullException(nameof(tile), "Discard action must include a tile.");
            }

            bool removed = player.RemoveFromHand(tile);
            if (!removed)
            {
                throw new InvalidOperationException("Player does not have the tile they are attempting to discard.");
            }

            state.GetDiscardsFor(player.SeatIndex).Add(tile);

            // Advance to next seat and reset phase to Draw.
            state.CurrentSeat = (state.CurrentSeat + 1) % state.Players.Count;
            state.TurnPhase = TurnPhase.Draw;
        }

        private static void ApplyDeclareWin(GameState state, PlayerState player)
        {
            // Later, call HandEvaluator here and validate the win.
            // For now, just mark the round as over and this player as winner.
            player.HasWon = true;
            state.IsRoundOver = true;
            state.WinnerSeat = player.SeatIndex;
        }
    }
}
