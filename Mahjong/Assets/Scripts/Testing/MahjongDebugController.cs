using UnityEngine;
using System.Linq;
using MJ.Input;
using MJ.Game;
using MJ.Rules;
using MJ.Player;

namespace MJ.Testing
{
    /// <summary>
    /// Dev-only debug controller for driving the Mahjong engine inside Unity.
    /// 
    /// - Holds a GameState
    /// - Can start a new round
    /// - Can print hands / wall / discards to the Console
    /// - Can force draws and discards
    /// 
    /// You can:
    /// - Call the public methods from UI Buttons
    /// - Or use the sample keyboard bindings in Update()
    /// - Or use ContextMenu items in the inspector
    /// </summary>
    public class MahjongDebugController : MonoBehaviour
    {
        [Header("Round Settings")]
        [Tooltip("0 = East, 1 = South, 2 = West, 3 = North")]
        [Range(0, 3)]
        public int dealerSeat = 0;

        [Tooltip("Set to -1 for random shuffle each time. Any other value makes the wall deterministic.")]
        public int shuffleSeed = -1;

        [Tooltip("If true, a new round will be started automatically in Start().")]
        public bool autoStartOnPlay = true;

        [Header("Keyboard Shortcuts (Dev Only)")]
        public bool enableKeyboardShortcuts = true;

        [Tooltip("Seat used by keyboard shortcuts like ForceDraw/ForceDiscard for quick testing.")]
        [Range(0, 3)]
        public int debugSeat = 0;

        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        /// <summary>
        /// Current game state for this debug controller.
        /// </summary>
        public GameState State { get; private set; }

        public int CurrentSeat
        {
            get
            {
                if (State == null) return 0;
                return State.CurrentSeat;
            }
        }

        private void Start()
        {
            inputReader.EnableDebugInput();
            inputReader.startNewRoundEvent += StartNewRound;
            inputReader.printHandsEvent += PrintHands;
            inputReader.printWallCountEvent += PrintWallCount;
            inputReader.printDiscardsEvent += PrintDiscards;
            inputReader.printTurnInfoEvent += PrintTurnInfo;
            inputReader.printCurrentHandEvent += PrintCurrentHand;
            
            if (autoStartOnPlay)
            {
                StartNewRound();
            }
        }

        // ---------- Core commands (callable from UI buttons / context menu) ----------

        [ContextMenu("Start New Round")]
        public void StartNewRound()
        {
            int? seed = shuffleSeed >= 0 ? shuffleSeed : (int?)null;

            State = RoundInitializer.StartNewRound(dealerSeat, seed);

            Debug.Log($"[MahjongDebug] Started new round. Dealer seat: {dealerSeat}, Seed: {(seed?.ToString() ?? "random")}");
            PrintHands();
            PrintWallCount();
        }

        [ContextMenu("Print Hands")]
        public void PrintHands()
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            foreach (var player in State.Players)
            {
                var tiles = string.Join(", ", player.Hand.Select(t => t.Type.ToString()));
                Debug.Log($"[MahjongDebug] Seat {player.SeatIndex} Hand ({player.Hand.Count}): {tiles}");
            }
        }

        [ContextMenu("Print Wall Count")]
        public void PrintWallCount()
        {
            PrintWallCount(5);
        }

        public void PrintWallCount(int preview)
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            int remaining = State.Wall.Count - State.WallIndex;
            Debug.Log($"[MahjongDebug] Wall has {remaining} tiles remaining. Next {preview}:");

            for (int i = 0; i < preview && State.WallIndex + i < State.Wall.Count; i++)
            {
                var tile = State.Wall[State.WallIndex + i];
                Debug.Log($"[MahjongDebug]   #{State.WallIndex + i} -> {tile}");
            }
        }

        public void ForceDraw(int seat)
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            if (seat < 0 || seat >= State.Players.Count)
            {
                Debug.LogWarning($"[MahjongDebug] Invalid seat {seat}.");
                return;
            }

            if (State.WallIndex >= State.Wall.Count)
            {
                Debug.Log("[MahjongDebug] Wall empty, cannot draw.");
                return;
            }

            var player = State.GetPlayer(seat);
            var tile = State.Wall[State.WallIndex++];
            player.AddToHand(tile);
            player.SortHand();

            Debug.Log($"[MahjongDebug] Seat {seat} drew {tile.Type} (Instance #{tile.InstanceId}). Hand now has {player.Hand.Count} tiles.");
        }

        public void ForceDiscardByIndex(int seat, int handIndex)
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            if (seat < 0 || seat >= State.Players.Count)
            {
                Debug.LogWarning($"[MahjongDebug] Invalid seat {seat}.");
                return;
            }

            var player = State.GetPlayer(seat);

            if (handIndex < 0 || handIndex >= player.Hand.Count)
            {
                Debug.LogWarning($"[MahjongDebug] Invalid hand index {handIndex} for seat {seat}.");
                return;
            }

            var tile = player.Hand[handIndex];
            player.Hand.RemoveAt(handIndex);
            State.GetDiscardsFor(seat).Add(tile);

            Debug.Log($"[MahjongDebug] Seat {seat} discarded {tile.Type} (Instance #{tile.InstanceId}). Hand now: {player.Hand.Count} tiles.");
        }

        [ContextMenu("Print Discards")]
        public void PrintDiscards()
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            for (int seat = 0; seat < State.Players.Count; seat++)
            {
                var discards = State.GetDiscardsFor(seat);
                var text = discards.Count == 0
                    ? "(none)"
                    : string.Join(", ", discards.Select(t => t.Type.ToString()));
                Debug.Log($"[MahjongDebug] Seat {seat} discards ({discards.Count}): {text}");
            }
        }

        [ContextMenu("Print Turn Info")]
        public void PrintTurnInfo()
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            Debug.Log($"[MahjongDebug] Dealer: {State.DealerSeat}, Current: {State.CurrentSeat}, Phase: {State.TurnPhase}");
        }

        [ContextMenu("Print Current Hand")]
        public void PrintCurrentHand() {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            var tiles = string.Join(", ", State.Players[State.CurrentSeat].Hand.Select(t => t.Type.ToString()));
            Debug.Log($"[MahjongDebug] Seat {State.Players[State.CurrentSeat].SeatIndex} Hand ({State.Players[State.CurrentSeat].Hand.Count}): {tiles}");
        }

        [ContextMenu("Print Legal Actions For Current Seat")]
        public void PrintLegalActionsForCurrentSeat()
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            var actions = RuleEngine.GetLegalActions(State, State.CurrentSeat);
            if (actions.Count == 0)
            {
                Debug.Log($"[MahjongDebug] No legal actions for seat {State.CurrentSeat} (round over? {State.IsRoundOver}).");
                return;
            }

            Debug.Log($"[MahjongDebug] Legal actions for seat {State.CurrentSeat}, phase {State.TurnPhase}:");
            foreach (var a in actions)
            {
                Debug.Log($"    {a}");
            }
        }

        [ContextMenu("Step Draw For Current Seat")]
        public void StepDrawForCurrentSeat()
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            if (State.IsRoundOver)
            {
                Debug.Log("[MahjongDebug] Round is over, cannot draw.");
                return;
            }

            if (State.TurnPhase != TurnPhase.Draw)
            {
                Debug.Log($"[MahjongDebug] Not in Draw phase (current phase is {State.TurnPhase}).");
                return;
            }

            var actions = RuleEngine.GetLegalActions(State, State.CurrentSeat);
            var draw = actions.Find(a => a.Type == ActionType.Draw);
            if (draw == null)
            {
                Debug.Log("[MahjongDebug] No Draw action available.");
                return;
            }

            Debug.Log($"[MahjongDebug] Applying action: {draw}");
            RuleEngine.ApplyAction(State, draw);

            // Show updated hand for that seat
            var player = State.GetPlayer(State.CurrentSeat);
            var tiles = string.Join(", ", player.Hand.Select(t => t.Type.ToString()));
            Debug.Log($"[MahjongDebug] Seat {player.SeatIndex} hand after draw ({player.Hand.Count}): {tiles}");
            PrintTurnInfo();
        }

        public void StepDiscardForCurrentSeatByIndex(int handIndex)
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            if (State.IsRoundOver)
            {
                Debug.Log("[MahjongDebug] Round is over, cannot discard.");
                return;
            }

            if (State.TurnPhase != TurnPhase.Discard)
            {
                Debug.Log($"[MahjongDebug] Not in Discard phase (current phase is {State.TurnPhase}).");
                return;
            }

            var player = State.GetPlayer(State.CurrentSeat);

            if (handIndex < 0 || handIndex >= player.Hand.Count)
            {
                Debug.LogWarning($"[MahjongDebug] Invalid hand index {handIndex} for seat {State.CurrentSeat}.");
                return;
            }

            var tileToDiscard = player.Hand[handIndex];

            var actions = RuleEngine.GetLegalActions(State, State.CurrentSeat);
            PlayerAction discardAction = actions.Find(a =>
                a.Type == ActionType.Discard &&
                a.Tile == tileToDiscard);

            if (discardAction == null)
            {
                Debug.Log("[MahjongDebug] No matching Discard action for that tile (unexpected).");
                return;
            }

            Debug.Log($"[MahjongDebug] Applying action: {discardAction}");
            RuleEngine.ApplyAction(State, discardAction);

            PrintDiscards();
            PrintTurnInfo();
        }

        [ContextMenu("Step Discard First Tile For Current Seat")]
        public void StepDiscardFirstTileForCurrentSeat()
        {
            StepDiscardForCurrentSeatByIndex(0);
        }

        [ContextMenu("Step Declare Win For Current Seat")]
        public void StepDeclareWinForCurrentSeat()
        {
            if (State == null)
            {
                Debug.Log("[MahjongDebug] No active GameState. Start a round first.");
                return;
            }

            if (State.IsRoundOver)
            {
                Debug.Log("[MahjongDebug] Round is already over.");
                return;
            }

            var seat = State.CurrentSeat;
            var player = State.GetPlayer(seat);

            if (!HandEvaluator.IsWinningHand(player))
            {
                Debug.Log($"[MahjongDebug] Seat {seat}'s hand is not a winning hand. Cannot declare win.");
                return;
            }

            // Find the DeclareWin action from the legal actions list.
            var actions = RuleEngine.GetLegalActions(State, seat);
            var winAction = actions.Find(a => a.Type == ActionType.DeclareWin);

            if (winAction == null)
            {
                Debug.Log($"[MahjongDebug] No DeclareWin action available for seat {seat} (unexpected).");
                return;
            }

            Debug.Log($"[MahjongDebug] Applying action: {winAction}");
            RuleEngine.ApplyAction(State, winAction);

            Debug.Log($"[MahjongDebug] Seat {seat} has declared win! Round over = {State.IsRoundOver}, WinnerSeat = {State.WinnerSeat}.");
            PrintHands();
            PrintDiscards();
        }
    }
}
