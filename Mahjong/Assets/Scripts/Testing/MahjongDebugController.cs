using UnityEngine;
using System.Linq;
using MJ.Game;

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

        /// <summary>
        /// Current game state for this debug controller.
        /// </summary>
        public GameState State { get; private set; }

        private void Start()
        {
            if (autoStartOnPlay)
            {
                StartNewRound();
            }
        }

        private void Update()
        {
            if (!enableKeyboardShortcuts)
                return;

            // These are just examples — tweak to taste.
            if (Input.GetKeyDown(KeyCode.F1))
            {
                StartNewRound();
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                PrintHands();
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                PrintWallCount();
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                PrintDiscards();
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                PrintTurnInfo();
            }

            // Force draw/discard for the debugSeat
            if (Input.GetKeyDown(KeyCode.G))  // G for "Grab" / draw
            {
                ForceDraw(debugSeat);
            }

            if (Input.GetKeyDown(KeyCode.H))  // H for "Hand discard" by first tile
            {
                ForceDiscardByIndex(debugSeat, 0);
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
    }
}
