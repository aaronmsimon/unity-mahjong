using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MJ.Core.Tiles;
using MJ.GameFlow;
using MJ.Core.Hand;

namespace MJ.UI
{
    /// <summary>
    /// Debug UI for swapping tiles between locations
    /// Uses persistent location tracking for instant tile lookup
    /// </summary>
    public class TileSwapUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameFlowController gameFlowController;
        [SerializeField] private TableLayoutView tableLayoutView;

        [Header("Panel")]
        [SerializeField] private GameObject swapPanel;

        [Header("Step 1: Tile Type Selection")]
        [SerializeField] private TMP_Dropdown suitDropdown;
        [SerializeField] private TMP_Dropdown rankDropdown;
        [SerializeField] private Button searchButton;

        [Header("Step 2: Source Selection")]
        [SerializeField] private Transform sourceListContainer;
        [SerializeField] private GameObject tileLocationItemPrefab;
        [SerializeField] private TMP_Text sourceSelectedText;

        [Header("Step 3: Destination Selection")]
        [SerializeField] private Transform destListContainer;
        [SerializeField] private TMP_Text destSelectedText;

        [Header("Step 4: Execute")]
        [SerializeField] private Button swapButton;
        [SerializeField] private Button cancelButton;

        // Current state
        private TileData searchedTileType;
        private List<TileLocation> foundLocations;
        private TileLocation selectedSource;
        private TileLocation selectedDestination;
        private List<GameObject> sourceItems = new List<GameObject>();
        private List<GameObject> destItems = new List<GameObject>();

        private void Awake()
        {
            SetupDropdowns();
            SetupButtons();
            HidePanel();
        }

        #region Setup

        private void SetupDropdowns()
        {
            // Setup suit dropdown
            if (suitDropdown != null)
            {
                suitDropdown.ClearOptions();
                suitDropdown.AddOptions(new List<string> 
                { 
                    "Characters", "Bamboo", "Dots", 
                    "Winds", "Dragons", 
                    "Flowers", "Seasons" 
                });
                suitDropdown.onValueChanged.AddListener(OnSuitChanged);
            }

            // Rank dropdown will be populated based on suit selection
            OnSuitChanged(0); // Initialize with first suit
        }

        private void SetupButtons()
        {
            if (searchButton != null)
                searchButton.onClick.AddListener(OnSearchClicked);

            if (swapButton != null)
            {
                swapButton.onClick.AddListener(OnSwapClicked);
                swapButton.interactable = false;
            }

            if (cancelButton != null)
                cancelButton.onClick.AddListener(HidePanel);
        }

        #endregion

        #region Panel Control

        public void ShowPanel()
        {
            if (swapPanel != null)
                swapPanel.SetActive(true);

            ResetPanel();
        }

        public void HidePanel()
        {
            if (swapPanel != null)
                swapPanel.SetActive(false);
        }

        private void ResetPanel()
        {
            selectedSource = null;
            selectedDestination = null;
            foundLocations = null;
            ClearSourceList();
            ClearDestList();
            UpdateUI();
            DisplayDestinations();
        }

        #endregion

        #region Step 1: Tile Type Selection

        private void OnSuitChanged(int index)
        {
            if (rankDropdown == null) return;

            rankDropdown.ClearOptions();

            List<string> options = new List<string>();

            switch (index)
            {
                case 0: // Characters
                case 1: // Bamboo
                case 2: // Dots
                    for (int i = 1; i <= 9; i++)
                        options.Add(i.ToString());
                    break;

                case 3: // Winds
                    options.AddRange(new[] { "East", "South", "West", "North" });
                    break;

                case 4: // Dragons
                    options.AddRange(new[] { "Red", "Green", "White" });
                    break;

                case 5: // Flowers
                case 6: // Seasons
                    for (int i = 1; i <= 4; i++)
                        options.Add(i.ToString());
                    break;
            }

            rankDropdown.AddOptions(options);
        }

        private void OnSearchClicked()
        {
            if (gameFlowController == null)
            {
                Debug.LogError("GameFlowController not assigned!");
                return;
            }

            // Build tile type from dropdown selections
            searchedTileType = BuildTileDataFromDropdowns();

            // if (searchedTileType.Equals(default(TileData)))
            // {
            //     Debug.LogError("Failed to build tile data from dropdowns");
            //     return;
            // }

            // Search for all instances of this tile type (instant with location tracking!)
            foundLocations = gameFlowController.FindAllTilesOfType(searchedTileType);

            Debug.Log($"Found {foundLocations.Count} instances of {searchedTileType}");

            // Display results in both source and destination lists
            DisplayLocations();
        }

        private TileData BuildTileDataFromDropdowns()
        {
            int suitIndex = suitDropdown.value;
            int rankIndex = rankDropdown.value;

            switch (suitIndex)
            {
                case 0: // Characters
                    return new TileData(TileSuit.Characters, rankIndex + 1, 0);

                case 1: // Bamboo
                    return new TileData(TileSuit.Bamboo, rankIndex + 1, 0);

                case 2: // Dots
                    return new TileData(TileSuit.Dots, rankIndex + 1, 0);

                case 3: // Winds
                    WindType wind = (WindType)rankIndex;
                    return new TileData(wind, 0);

                case 4: // Dragons
                    DragonType dragon = (DragonType)rankIndex;
                    return new TileData(dragon, 0);

                case 5: // Flowers
                    return new TileData(TileSuit.Flower, rankIndex + 1, 0);

                case 6: // Seasons
                    return new TileData(TileSuit.Season, rankIndex + 1, 0);

                default:
                    return default;
            }
        }

        #endregion

        #region Step 2 & 3: Display and Select Locations

        private void DisplayLocations()
        {
            ClearSourceList();

            if (foundLocations == null || foundLocations.Count == 0)
            {
                Debug.LogWarning("No locations found");
                return;
            }

            // Create list items for both source and destination lists
            foreach (var location in foundLocations)
            {
                // Skip melds - cannot swap from/to melds
                if (location.Type == LocationType.PlayerMeld)
                {
                    Debug.Log($"Skipping meld location: {location.DisplayString}");
                    continue;
                }

                CreateLocationItem(location, sourceListContainer, sourceItems, true);
            }
        }

        private void DisplayDestinations() {
            if (tableLayoutView != null) {
                int activeSeat = tableLayoutView.GetActiveSeat();
                HandView handView = tableLayoutView.GetHandViewForSeat(activeSeat);
                Hand hand = handView.GetHand();
                List<TileInstance> tiles = hand.GetAllTiles();

                foreach (TileInstance tileInstance in tiles) {
                    CreateLocationItem(new TileLocation(tileInstance), destListContainer, destItems, false);
                }
            } else {
                Debug.Log("Table Layout View not set!");
            }
        }

        private void CreateLocationItem(TileLocation location, Transform container, List<GameObject> itemsList, bool isSource)
        {
            if (tileLocationItemPrefab == null)
            {
                Debug.LogError("Tile location item prefab not assigned!");
                return;
            }

            GameObject item = Instantiate(tileLocationItemPrefab, container);
            itemsList.Add(item);

            // Setup text
            TMP_Text text = item.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = location.DisplayString;
            }

            // Setup button
            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                if (isSource)
                {
                    button.onClick.AddListener(() => OnSourceSelected(location));
                }
                else
                {
                    button.onClick.AddListener(() => OnDestinationSelected(location));
                }
            }
        }

        private void OnSourceSelected(TileLocation location)
        {
            selectedSource = location;
            Debug.Log($"Selected source: {location.DisplayString}");
            UpdateUI();
        }

        private void OnDestinationSelected(TileLocation location)
        {
            selectedDestination = location;
            Debug.Log($"Selected destination: {location.DisplayString}");
            UpdateUI();
        }

        private void ClearSourceList()
        {
            foreach (var item in sourceItems)
            {
                if (item != null)
                    Destroy(item);
            }
            sourceItems.Clear();
        }

        private void ClearDestList()
        {
            foreach (var item in destItems)
            {
                if (item != null)
                    Destroy(item);
            }
            destItems.Clear();
        }

        #endregion

        #region Step 4: Execute Swap

        private void OnSwapClicked()
        {
            if (selectedSource == null || selectedDestination == null)
            {
                Debug.LogWarning("Must select both source and destination");
                return;
            }

            if (selectedSource.Tile == selectedDestination.Tile)
            {
                Debug.LogWarning("Cannot swap a tile with itself!");
                return;
            }

            // Execute swap via GameFlowController
            bool success = gameFlowController.SwapTiles(selectedSource, selectedDestination);

            if (success)
            {
                Debug.Log("Swap successful!");
                HidePanel();
            }
            else
            {
                Debug.LogError("Swap failed!");
            }
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            // Update selected text displays
            if (sourceSelectedText != null)
            {
                sourceSelectedText.text = selectedSource != null 
                    ? $"Source: {selectedSource.DisplayString}" 
                    : "Source: None";
            }

            if (destSelectedText != null)
            {
                destSelectedText.text = selectedDestination != null 
                    ? $"Dest: {selectedDestination.DisplayString}" 
                    : "Dest: None";
            }

            // Enable swap button only if both source and dest are selected
            if (swapButton != null)
            {
                swapButton.interactable = selectedSource != null && selectedDestination != null;
            }
        }

        #endregion
    }
}
