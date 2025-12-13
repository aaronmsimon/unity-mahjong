using UnityEngine;
using UnityEngine.UI;
using MJ.Core.Tiles;
using MJ.UI;

namespace MJ.Testing
{
    /// <summary>
    /// Test component to visualize all unique tile sprites
    /// Displays one of each tile type in a grid
    /// </summary>
    public class TileSpriteTest : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TileSpriteLibrarySO spriteLibrary;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform tileContainer;
        
        [Header("Layout")]
        [SerializeField] private float tileSpacing = 120f;
        [SerializeField] private int tilesPerRow = 10;

        private void Start()
        {
            if (spriteLibrary == null)
            {
                Debug.LogError("TileSpriteLibrary not assigned!");
                return;
            }

            if (tilePrefab == null)
            {
                Debug.LogError("Tile Prefab not assigned!");
                return;
            }

            if (tileContainer == null)
            {
                tileContainer = transform;
            }

            DisplayAllUniqueTiles();
        }

        [ContextMenu("Display All Tiles")]
        public void DisplayAllUniqueTiles()
        {
            // Clear existing tiles
            foreach (Transform child in tileContainer)
            {
                Destroy(child.gameObject);
            }

            int tileIndex = 0;

            // Bamboo 1-9
            for (int i = 1; i <= 9; i++)
            {
                TileData tile = new TileData(TileSuit.Bamboo, i, tileIndex++);
                CreateTileView(tile, tileIndex++);
            }

            // Characters 1-9
            for (int i = 1; i <= 9; i++)
            {
                TileData tile = new TileData(TileSuit.Characters, i, tileIndex++);
                CreateTileView(tile, tileIndex++);
            }

            // Dots 1-9
            for (int i = 1; i <= 9; i++)
            {
                TileData tile = new TileData(TileSuit.Dots, i, tileIndex++);
                CreateTileView(tile, tileIndex++);
            }

            // Winds
            CreateTileView(new TileData(WindType.East, tileIndex++), tileIndex);
            CreateTileView(new TileData(WindType.South, tileIndex++), tileIndex);
            CreateTileView(new TileData(WindType.West, tileIndex++), tileIndex);
            CreateTileView(new TileData(WindType.North, tileIndex++), tileIndex);

            // Dragons
            CreateTileView(new TileData(DragonType.Red, tileIndex++), tileIndex);
            CreateTileView(new TileData(DragonType.Green, tileIndex++), tileIndex);
            CreateTileView(new TileData(DragonType.White, tileIndex++), tileIndex);

            // Flowers (if you have them)
            for (int i = 1; i <= 4; i++)
            {
                TileData tile = new TileData(TileSuit.Flower, i, tileIndex++);
                CreateTileView(tile, tileIndex++);
            }

            // Seasons (if you have them)
            for (int i = 1; i <= 4; i++)
            {
                TileData tile = new TileData(TileSuit.Season, i, tileIndex++);
                CreateTileView(tile, tileIndex++);
            }

            Debug.Log($"Created {tileIndex} unique tile views");
        }

        private void CreateTileView(TileData tileData, int index)
        {
            // Instantiate tile
            GameObject tileObj = Instantiate(tilePrefab, tileContainer);
            
            // Position in grid
            int row = index / tilesPerRow;
            int col = index % tilesPerRow;
            
            RectTransform rectTransform = tileObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(col * tileSpacing, -row * tileSpacing);
            }
            else
            {
                tileObj.transform.localPosition = new Vector3(col * tileSpacing, -row * tileSpacing, 0);
            }

            // Get sprite from library
            Sprite sprite = spriteLibrary.GetSprite(tileData);
            
            if (sprite == null)
            {
                Debug.LogWarning($"No sprite found for: {tileData}");
            }

            // Setup TileView
            TileView tileView = tileObj.GetComponent<TileView>();
            if (tileView != null)
            {
                TileInstance tileInstance = new TileInstance(tileData);
                tileView.Setup(tileInstance, sprite, faceUp: true);
            }
            else
            {
                // Fallback: just set the image directly
                Image image = tileObj.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = sprite;
                }
            }

            // Add label below tile (optional)
            CreateLabel(tileObj.transform, tileData.ToString());
        }

        private void CreateLabel(Transform parent, string text)
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(parent);
            
            RectTransform rectTransform = labelObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, -70);
            rectTransform.sizeDelta = new Vector2(100, 30);
            
            Text label = labelObj.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 12;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
        }
    }
}
