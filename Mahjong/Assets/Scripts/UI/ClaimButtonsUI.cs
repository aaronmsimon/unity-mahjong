using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MJ.Core.Tiles;

namespace MJ.UI
{
    /// <summary>
    /// UI for claiming discarded tiles (Pong, Kong, Chow, Win)
    /// Shows buttons when a tile is discarded and player can claim
    /// </summary>
    public class ClaimButtonsUI : MonoBehaviour
    {
        [Header("Claim Buttons")]
        [SerializeField] private Button pongButton;
        [SerializeField] private Button kongButton;
        [SerializeField] private Button chowButton;
        [SerializeField] private Button winButton;
        [SerializeField] private Button passButton;

        [Header("Display")]
        [SerializeField] private GameObject claimPanel;
        [SerializeField] private TMP_Text discardedTileText;
        [SerializeField] private float claimWindowDuration = 5f; // Seconds to claim

        // Events
        public System.Action OnPongClaimed;
        public System.Action OnKongClaimed;
        public System.Action OnChowClaimed;
        public System.Action OnWinClaimed;
        public System.Action OnPassClaimed;

        private float claimWindowTimer;
        private bool isClaimWindowOpen;

        private void Awake()
        {
            // Setup button listeners
            if (pongButton != null)
                pongButton.onClick.AddListener(OnPongClicked);
            if (kongButton != null)
                kongButton.onClick.AddListener(OnKongClicked);
            if (chowButton != null)
                chowButton.onClick.AddListener(OnChowClicked);
            if (winButton != null)
                winButton.onClick.AddListener(OnWinClicked);
            if (passButton != null)
                passButton.onClick.AddListener(OnPassClicked);

            // Hide panel initially
            HideClaimPanel();
        }

        private void Update()
        {
            if (isClaimWindowOpen)
            {
                claimWindowTimer -= Time.deltaTime;
                if (claimWindowTimer <= 0)
                {
                    // Auto-pass if time runs out
                    OnPassClicked();
                }
            }
        }

        #region Public API

        /// <summary>
        /// Shows the claim panel with available options
        /// </summary>
        public void ShowClaimOptions(TileData discardedTile, bool canPong, bool canKong, bool canChow, bool canWin)
        {
            if (claimPanel != null)
                claimPanel.SetActive(true);

            // Update discarded tile display
            if (discardedTileText != null)
                discardedTileText.text = $"Discarded: {discardedTile}";

            // Enable/disable buttons based on what's possible
            if (pongButton != null)
                pongButton.interactable = canPong;
            if (kongButton != null)
                kongButton.interactable = canKong;
            if (chowButton != null)
                chowButton.interactable = canChow;
            if (winButton != null)
                winButton.interactable = canWin;
            if (passButton != null)
                passButton.interactable = true;

            // Start timer
            claimWindowTimer = claimWindowDuration;
            isClaimWindowOpen = true;

            Debug.Log($"Claim window opened for {discardedTile}. Can: Pong={canPong}, Kong={canKong}, Chow={canChow}, Win={canWin}");
        }

        /// <summary>
        /// Hides the claim panel
        /// </summary>
        public void HideClaimPanel()
        {
            if (claimPanel != null)
                claimPanel.SetActive(false);

            isClaimWindowOpen = false;
        }

        /// <summary>
        /// Gets remaining time in claim window
        /// </summary>
        public float GetRemainingTime()
        {
            return claimWindowTimer;
        }

        #endregion

        #region Button Handlers

        private void OnPongClicked()
        {
            Debug.Log("Player claimed Pong");
            HideClaimPanel();
            OnPongClaimed?.Invoke();
        }

        private void OnKongClicked()
        {
            Debug.Log("Player claimed Kong");
            HideClaimPanel();
            OnKongClaimed?.Invoke();
        }

        private void OnChowClicked()
        {
            Debug.Log("Player claimed Chow");
            HideClaimPanel();
            OnChowClaimed?.Invoke();
        }

        private void OnWinClicked()
        {
            Debug.Log("Player claimed Win");
            HideClaimPanel();
            OnWinClaimed?.Invoke();
        }

        private void OnPassClicked()
        {
            Debug.Log("Player passed on claim");
            HideClaimPanel();
            OnPassClaimed?.Invoke();
        }

        #endregion
    }
}
