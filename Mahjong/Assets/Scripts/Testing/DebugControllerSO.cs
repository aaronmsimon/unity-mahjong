using UnityEngine;

namespace MJ.Testing
{
    [CreateAssetMenu(fileName = "DebugController", menuName = "Mahjong/Debug Controller")]
    public class DebugControllerSO : ScriptableObject
    {
        [Header("Hand")]
        public bool SortTiles;

        [Header("Meld")]
        [SerializeField] private bool pongValidation;
        [SerializeField] private bool kongValidation;
        [SerializeField] private bool chowValidation;
        [SerializeField] private bool kongConversion;

        [Header("Tile Factory")]
        public bool TileCreation;

        [Header("Wall")]
        public bool WallCreation;

        [Header("Claim Manager")]
        [SerializeField] private bool openClaimWindow;
        [SerializeField] private bool submitClaim;
        [SerializeField] private bool resolveClaim;

        [Header("Game Flow Controller")]
        public bool StartGame;
        public bool StartHand;
        public bool Shuffle;
        public bool ChangeTurn;
        public bool DealHands;
        public bool BonusTileReplacement;
        public bool ShowHand;
        public bool DrawTile;
        public bool WallInfo;
        public bool HandleDiscard;
        public bool DiscardTile;
        public bool ClaimWindowOpened;
        public bool ClaimOptions;
        public bool ClaimDecision;
        public bool ClaimResolved;
        public bool ClaimPong;
        public bool ClaimKong;
        public bool ClaimChow;
        public bool ClaimWin;

        [Header("Game State")]
        public bool Transition;
        public bool History;
        [SerializeField] private bool undo;
        [SerializeField] private bool redo;

        [Header("User Interface")]
        [SerializeField] private bool claimWindowOpenedUi;
        [SerializeField] private bool buttonPong;
        [SerializeField] private bool buttonKong;
        [SerializeField] private bool buttonChow;
        [SerializeField] private bool buttonWin;
        [SerializeField] private bool buttonPass;

        [Header("Player Hand View")]
        [SerializeField] private bool discardTileHv;

        [Header("Player Hand Controller")]
        [SerializeField] private bool discardTilePhc;       
    }
}
