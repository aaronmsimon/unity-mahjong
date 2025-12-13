using UnityEngine;
using UnityEngine.Events;
using MJ.Core.Tiles;

namespace MJ.GameLogic
{
    /// <summary>
    /// ScriptableObject-based event system for game state changes
    /// Decouples systems through events
    /// </summary>
    [CreateAssetMenu(fileName = "GameEvents", menuName = "Mahjong/Game Events")]
    public class GameEvents : ScriptableObject
    {
        [Header("Game Lifecycle")]
        public UnityEvent OnGameStarted;
        public UnityEvent OnGameEnded;

        [Header("Round/Hand Events")]
        public UnityEvent OnRoundStarted;
        public UnityEvent OnRoundEnded;
        public UnityEvent OnHandStarted;
        public UnityEvent OnHandEnded;

        [Header("Turn Events")]
        public UnityEvent<int> OnTurnChanged;        // int = player index
        public UnityEvent<int> OnDealerChanged;      // int = dealer index
        public UnityEvent<WindType> OnPrevailingWindChanged;

        [Header("Phase Events")]
        public UnityEvent<GamePhase> OnPhaseChanged;

        [Header("Tile Events")]
        public UnityEvent<int> OnTileDrawn;          // int = player index
        public UnityEvent<int> OnTileDiscarded;      // int = player index
        
        [Header("Claim Events")]
        public UnityEvent OnClaimWindowOpened;
        public UnityEvent OnClaimWindowClosed;

        [Header("Win Events")]
        public UnityEvent<int> OnPlayerWon;          // int = player index

        public void Initialize()
        {
            // Initialize all events
            OnGameStarted ??= new UnityEvent();
            OnGameEnded ??= new UnityEvent();
            OnRoundStarted ??= new UnityEvent();
            OnRoundEnded ??= new UnityEvent();
            OnHandStarted ??= new UnityEvent();
            OnHandEnded ??= new UnityEvent();
            OnTurnChanged ??= new UnityEvent<int>();
            OnDealerChanged ??= new UnityEvent<int>();
            OnPrevailingWindChanged ??= new UnityEvent<WindType>();
            OnPhaseChanged ??= new UnityEvent<GamePhase>();
            OnTileDrawn ??= new UnityEvent<int>();
            OnTileDiscarded ??= new UnityEvent<int>();
            OnClaimWindowOpened ??= new UnityEvent();
            OnClaimWindowClosed ??= new UnityEvent();
            OnPlayerWon ??= new UnityEvent<int>();
        }

        private void OnEnable()
        {
            Initialize();
        }
    }
}
