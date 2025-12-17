using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        [SerializeField] private Button startNewGameButton;
        [SerializeField] private Button startNewRoundButton;

        [Header("Section: Player Control")]
        [SerializeField] private Button switchSeat0Button;
        [SerializeField] private Button switchSeat1Button;
        [SerializeField] private Button switchSeat2Button;
        [SerializeField] private Button switchSeat3Button;
        [SerializeField] private Button switchToActivePlayerButton;

        [Header("Section: Hand Editor")]
        [SerializeField] private Button exchangeTileButton;

        // Panel state
        private bool isPanelOpen;
        private Vector2 openPosition;
        private Vector2 closedPosition;
        private bool isAnimating;

        // Events for button clicks (GameFlowController will subscribe)
        public System.Action OnStartNewGame;
        public System.Action OnStartNewHand;
        public System.Action<int> OnSwitchToSeat;
        public System.Action OnSwitchToActivePlayer;
        public System.Action OnExchangeTile;

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
                UnityEngine.Debug.LogError("Debug Panel not assigned!");
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

            // Game Control
            if (startNewGameButton != null)
            {
                startNewGameButton.onClick.AddListener(() => OnStartNewGame?.Invoke());
            }
            if (startNewRoundButton != null)
            {
                startNewRoundButton.onClick.AddListener(() => OnStartNewHand?.Invoke());
            }

            // Player Control
            if (switchSeat0Button != null)
            {
                switchSeat0Button.onClick.AddListener(() => OnSwitchToSeat?.Invoke(0));
            }
            if (switchSeat1Button != null)
            {
                switchSeat1Button.onClick.AddListener(() => OnSwitchToSeat?.Invoke(1));
            }
            if (switchSeat2Button != null)
            {
                switchSeat2Button.onClick.AddListener(() => OnSwitchToSeat?.Invoke(2));
            }
            if (switchSeat3Button != null)
            {
                switchSeat3Button.onClick.AddListener(() => OnSwitchToSeat?.Invoke(3));
            }
            if (switchToActivePlayerButton != null)
            {
                switchToActivePlayerButton.onClick.AddListener(() => OnSwitchToActivePlayer?.Invoke());
            }

            // Hand Editor
            if (exchangeTileButton != null)
            {
                exchangeTileButton.onClick.AddListener(() => OnExchangeTile?.Invoke());
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
            UnityEngine.Debug.Log($"[Debug Console] {message}");
            // TODO: Add text display in UI
        }

        #endregion
    }
}
