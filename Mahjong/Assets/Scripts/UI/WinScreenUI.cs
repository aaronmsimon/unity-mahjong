using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MJ.Core.Tiles;
using MJ.Core.Hand;
using MJ.Scoring;

namespace MJ.UI
{
    /// <summary>
    /// Displays win screen with hand reveal and scoring breakdown
    /// </summary>
    public class WinScreenUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private TMP_Text winnerText;
        [SerializeField] private TMP_Text scoreText;
        
        [Header("Hand Display")]
        [SerializeField] private Transform handContainer;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private TileSpriteLibrarySO spriteLibrary;
        [SerializeField] private float tileSpacing = 70f;
        [SerializeField] private float meldSpacing = 100f;

        [Header("Score Breakdown")]
        [SerializeField] private Transform scoreBreakdownContainer;
        [SerializeField] private GameObject scoreLineItemPrefab; // Simple text prefab
        [SerializeField] private TMP_Text totalFanText;
        [SerializeField] private TMP_Text totalPointsText;

        [Header("Actions")]
        [SerializeField] private Button nextHandButton;
        [SerializeField] private Button quitButton;

        // Events
        public System.Action OnNextHandClicked;
        public System.Action OnQuitClicked;

        private List<GameObject> displayedTiles = new List<GameObject>();

        private void Awake()
        {
            if (nextHandButton != null)
                nextHandButton.onClick.AddListener(() => OnNextHandClicked?.Invoke());
            
            if (quitButton != null)
                quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());

            HideWinScreen();
        }

        #region Show/Hide

        /// <summary>
        /// Shows the win screen with scoring
        /// </summary>
        public void ShowWinScreen(int winnerIndex, Hand hand, ScoreResult scoreResult, bool isSelfDrawn)
        {
            if (winPanel != null)
                winPanel.SetActive(true);

            // Set winner text
            if (winnerText != null)
            {
                string winType = isSelfDrawn ? "Self-Drawn Win!" : "Win!";
                winnerText.text = $"Player {winnerIndex} - {winType}";
            }

            // Display hand
            DisplayWinningHand(hand);

            // Display scoring
            DisplayScoreBreakdown(scoreResult);

            Debug.Log($"Win screen shown for Player {winnerIndex}");
        }

        /// <summary>
        /// Hides the win screen
        /// </summary>
        public void HideWinScreen()
        {
            if (winPanel != null)
                winPanel.SetActive(false);

            ClearDisplayedTiles();
        }

        #endregion

        #region Hand Display

        private void DisplayWinningHand(Hand hand)
        {
            ClearDisplayedTiles();

            if (handContainer == null || tilePrefab == null || spriteLibrary == null)
                return;

            float currentX = 0;

            // Display exposed melds first
            var exposedMelds = hand.GetExposedMelds();
            foreach (var meld in exposedMelds)
            {
                foreach (var tile in meld.Tiles)
                {
                    CreateTileDisplay(tile, currentX);
                    currentX += tileSpacing;
                }
                currentX += meldSpacing; // Extra space between melds
            }

            // Display concealed tiles
            var concealedTiles = hand.GetConcealedTiles();
            foreach (var tile in concealedTiles)
            {
                CreateTileDisplay(tile, currentX);
                currentX += tileSpacing;
            }

            // Display bonus tiles (if any)
            var bonusTiles = hand.GetBonusTiles();
            if (bonusTiles.Count > 0)
            {
                currentX += meldSpacing;
                foreach (var tile in bonusTiles)
                {
                    CreateTileDisplay(tile, currentX);
                    currentX += tileSpacing;
                }
            }
        }

        private void CreateTileDisplay(TileInstance tile, float xPosition)
        {
            GameObject tileObj = Instantiate(tilePrefab, handContainer);
            
            RectTransform rectTransform = tileObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(xPosition, 0);
            }

            Sprite sprite = spriteLibrary.GetSprite(tile.Data);
            TileView tileView = tileObj.GetComponent<TileView>();
            
            if (tileView != null)
            {
                tileView.Setup(tile, sprite, faceUp: true);
            }

            // Disable button
            Button button = tileObj.GetComponent<Button>();
            if (button != null)
                button.interactable = false;

            displayedTiles.Add(tileObj);
        }

        private void ClearDisplayedTiles()
        {
            foreach (var tile in displayedTiles)
            {
                if (tile != null)
                    Destroy(tile);
            }
            displayedTiles.Clear();
        }

        #endregion

        #region Score Display

        private void DisplayScoreBreakdown(ScoreResult scoreResult)
        {
            if (scoreResult == null)
            {
                Debug.LogWarning("No score result to display");
                return;
            }

            // Clear previous breakdown
            if (scoreBreakdownContainer != null)
            {
                foreach (Transform child in scoreBreakdownContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // Display each pattern/fan contribution
            if (scoreResult.FanBreakdown != null && scoreBreakdownContainer != null)
            {
                foreach (var kvp in scoreResult.FanBreakdown)
                {
                    CreateScoreLineItem($"{kvp.Key}: +{kvp.Value} fan");
                }
            }

            // Display patterns list (if no fan breakdown)
            if ((scoreResult.FanBreakdown == null || scoreResult.FanBreakdown.Count == 0) 
                && scoreResult.Patterns != null && scoreResult.Patterns.Count > 0)
            {
                foreach (var pattern in scoreResult.Patterns)
                {
                    CreateScoreLineItem($"• {pattern}");
                }
            }

            // Display totals
            if (totalFanText != null && scoreResult.Fan.HasValue)
            {
                totalFanText.text = $"Total: {scoreResult.Fan.Value} fan";
            }

            if (totalPointsText != null)
            {
                totalPointsText.text = $"Score: {scoreResult.Points} points";
            }

            // Main score display
            if (scoreText != null)
            {
                if (scoreResult.Fan.HasValue)
                {
                    scoreText.text = $"{scoreResult.Fan.Value} fan = {scoreResult.Points} points";
                }
                else
                {
                    scoreText.text = $"{scoreResult.Points} points";
                }
            }
        }

        private void CreateScoreLineItem(string text)
        {
            if (scoreBreakdownContainer == null)
                return;

            GameObject lineItem;
            
            if (scoreLineItemPrefab != null)
            {
                // Use custom prefab
                lineItem = Instantiate(scoreLineItemPrefab, scoreBreakdownContainer);
                Text lineText = lineItem.GetComponent<Text>();
                if (lineText != null)
                    lineText.text = text;
            }
            else
            {
                // Create simple text object
                lineItem = new GameObject("ScoreLineItem");
                lineItem.transform.SetParent(scoreBreakdownContainer);
                
                Text lineText = lineItem.AddComponent<Text>();
                lineText.text = text;
                lineText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lineText.fontSize = 16;
                lineText.color = Color.white;
                
                RectTransform rectTransform = lineItem.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(400, 30);
            }
        }

        #endregion
    }
}
