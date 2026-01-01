using UnityEngine;
using UnityEngine.UI;
using MJ.UI;
using TMPro;
using RoboRyanTron.Unite2017.Events;
using RoboRyanTron.Unite2017.Variables;

namespace MJ.Testing
{
    /// <summary>
    /// Slide-out debug console panel for in-game testing and debugging
    /// Provides UI interface for debug commands
    /// </summary>
    public class DebugConsoleUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private RectTransform debugPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TMP_Text toggleButtonText;

        [Header("Animation")]
        [SerializeField] private float slideSpeed = 500f;
        [SerializeField] private bool startOpen = false;

        [Header("Section: Game Control")]
        [SerializeField] private GameEvent startNewGameEvent;
        [SerializeField] private GameEvent startNewRoundEvent;
        [SerializeField] private GameEvent switchToSeat;
        [SerializeField] private FloatVariable activeSeat;

        [Header("Section: Hand Editor")]
        [SerializeField] private Button exchangeTileButton;

        [Header("Section: Tile Swap")]
        [SerializeField] private TileSwapUI tileSwapUI;
        [SerializeField] private Button openTileSwapButton;

        // Panel state
        private bool isPanelOpen;
        private Vector2 openPosition;
        private Vector2 closedPosition;
        private bool isAnimating;

        private void Awake()
        {
            SetupPanel();
            SetupButtons();
            
            // Set initial state
            isPanelOpen = startOpen;
            if (!startOpen)
            {
                debugPanel.anchoredPosition = closedPosition;
            }
        }

        private void SetupPanel()
        {
            if (debugPanel == null)
            {
                Debug.LogError("Debug Panel not assigned!");
                return;
            }

            // Calculate open/closed positions
            // Assuming panel slides from the left
            float panelWidth = debugPanel.rect.width;
            openPosition = new Vector2(0, debugPanel.anchoredPosition.y);
            closedPosition = new Vector2(-panelWidth, debugPanel.anchoredPosition.y);
        }

        private void SetupButtons()
        {
            // Toggle button
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(TogglePanel);
            }

            if (openTileSwapButton != null)
            {
                openTileSwapButton.onClick.AddListener(OpenTileSwap);
            }
        }

        private void Update()
        {
            if (isAnimating)
            {
                AnimatePanel();
            }
        }

        #region Panel Control

        /// <summary>
        /// Toggles the debug panel open/closed
        /// </summary>
        public void TogglePanel()
        {
            if (isPanelOpen)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        /// <summary>
        /// Opens the debug panel
        /// </summary>
        public void OpenPanel()
        {
            isPanelOpen = true;
            isAnimating = true;
            UpdateToggleButtonText();
        }

        /// <summary>
        /// Closes the debug panel
        /// </summary>
        public void ClosePanel()
        {
            isPanelOpen = false;
            isAnimating = true;
            UpdateToggleButtonText();
        }

        private void AnimatePanel()
        {
            Vector2 targetPosition = isPanelOpen ? openPosition : closedPosition;
            Vector2 currentPosition = debugPanel.anchoredPosition;

            // Lerp towards target
            Vector2 newPosition = Vector2.MoveTowards(
                currentPosition, 
                targetPosition, 
                slideSpeed * Time.deltaTime
            );

            debugPanel.anchoredPosition = newPosition;

            // Check if reached target
            if (Vector2.Distance(newPosition, targetPosition) < 0.1f)
            {
                debugPanel.anchoredPosition = targetPosition;
                isAnimating = false;
            }
        }

        private void UpdateToggleButtonText()
        {
            if (toggleButtonText != null)
            {
                toggleButtonText.text = isPanelOpen ? "◄" : "►";
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Shows a status message in the console (for future expansion)
        /// </summary>
        public void ShowMessage(string message)
        {
            Debug.Log($"[Debug Console] {message}");
            // TODO: Add text display in UI
        }

        public void StartNewGame() {
            startNewGameEvent.Raise();
        }

        public void StartNewRound() {
            startNewRoundEvent.Raise();
        }
        
        public void SetActiveSeat(int activeSeat) {
            this.activeSeat.Value = activeSeat;
            switchToSeat.Raise();
        }

        public void OpenTileSwap()
        {
            if (tileSwapUI != null)
            {
                tileSwapUI.ShowPanel();
            }
            else
            {
                Debug.LogError("TileSwapUI not assigned!");
            }
        }

        #endregion
    }
}
