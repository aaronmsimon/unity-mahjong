using UnityEngine;
using UnityEngine.UI;
using MJ.Core.Tiles;

namespace MJ.UI
{
    /// <summary>
    /// Visual representation of a Mahjong tile
    /// Links TileInstance to a GameObject with sprite
    /// </summary>
    public class TileView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image tileImage;
        [SerializeField] private Image tileBackground;
        [SerializeField] private Image backImage;  // For face-down tiles
        
        [Header("State")]
        [SerializeField] private bool isFaceUp = true;
        
        // Reference to the tile data this view represents
        private TileInstance tileInstance;
        
        // Event for when this tile is clicked
        public System.Action<TileView> OnTileClicked;

        private void Awake()
        {
            // Setup click handler
            Button button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }
            button.onClick.AddListener(HandleClick);
        }

        /// <summary>
        /// Sets up this view with a tile instance and sprite
        /// </summary>
        public void Setup(TileInstance tile, Sprite sprite, bool faceUp = true)
        {
            tileInstance = tile;
            
            if (tileImage != null) tileImage.sprite = sprite;

            SetFaceUp(faceUp);
        }

        /// <summary>
        /// Flips the tile face up or down
        /// </summary>
        public void SetFaceUp(bool faceUp)
        {
            isFaceUp = faceUp;
            
            if (tileImage != null) tileImage.gameObject.SetActive(faceUp);
            if (backImage != null) backImage.gameObject.SetActive(!faceUp);

            tileBackground.gameObject.SetActive(faceUp);
        }

        /// <summary>
        /// Gets the tile instance this view represents
        /// </summary>
        public TileInstance GetTileInstance()
        {
            return tileInstance;
        }

        /// <summary>
        /// Gets the tile data
        /// </summary>
        public TileData GetTileData()
        {
            return tileInstance?.Data ?? default;
        }

        private void HandleClick()
        {
            if (isFaceUp)
            {
                OnTileClicked?.Invoke(this);
            }
        }

        /// <summary>
        /// Highlights this tile (for selection)
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            if (tileImage != null)
            {
                tileImage.color = highlighted ? Color.yellow : Color.white;
            }
        }
    }
}
