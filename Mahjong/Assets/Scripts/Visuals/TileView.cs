using UnityEngine;
using MJ.Core;

namespace MJ.Visuals
{
    /// <summary>
    /// Visual component for a mahjong tile. Shows the correct sprite
    /// for the TileInstance assigned to it.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileView : MonoBehaviour
    {
        [SerializeField] private TileSpriteLibrarySO spriteLibrary;

        private SpriteRenderer spriteRenderer;

        public TileInstance Instance { get; private set; }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Assign the tile instance that this view should display.
        /// </summary>
        public void Bind(TileInstance instance)
        {
            Instance = instance;

            if (spriteLibrary == null)
            {
                Debug.LogError("[TileView] No TileSpriteLibrary assigned.");
                return;
            }

            if (Instance == null)
            {
                spriteRenderer.sprite = null;
                return;
            }

            Sprite sprite = spriteLibrary.GetSpriteFor(Instance.Type);
            spriteRenderer.sprite = sprite;
        }
    }
}
