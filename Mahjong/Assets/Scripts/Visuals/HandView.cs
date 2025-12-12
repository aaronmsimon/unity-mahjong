using System.Collections.Generic;
using UnityEngine;
using MJ.Core;
using MJ.Testing;

namespace MJ.Visuals
{
    /// <summary>
    /// Handles visual layout of a single player's hand.
    /// Attach this to an empty GameObject and assign a TileView prefab.
    /// </summary>
    public class HandView : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("Which seat this hand belongs to (0..3).")]
        public int seatIndex;

        [Tooltip("Prefab with SpriteRenderer + TileView attached.")]
        [SerializeField] private TileView tilePrefab;

        [Tooltip("Horizontal spacing between tiles.")]
        [SerializeField] private float tileSpacing = 1.1f;

        [Tooltip("Reference to the debug controller that will handle clicks.")]
        [SerializeField] private MahjongDebugController debugController;

        private readonly List<TileView> _tileViews = new List<TileView>();

        /// <summary>
        /// Rebuilds the hand visuals from the given tiles.
        /// </summary>
        public void RenderHand(IReadOnlyList<TileInstance> tiles)
        {
            Clear();

            if (tiles == null)
                return;

            for (int i = 0; i < tiles.Count; i++)
            {
                TileInstance instance = tiles[i];
                TileView tv = Instantiate(tilePrefab, transform);
                tv.Bind(instance);

                // Simple horizontal row, centered around the parent
                float offset = (tiles.Count - 1) * 0.5f * tileSpacing;
                tv.transform.localPosition = new Vector3(i * tileSpacing - offset, 0f, 0f);

                _tileViews.Add(tv);
            }
        }

        /// <summary>
        /// Destroys existing tile GameObjects and clears the list.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _tileViews.Count; i++)
            {
                if (_tileViews[i] != null)
                {
                    Destroy(_tileViews[i].gameObject);
                }
            }
            _tileViews.Clear();
        }

        private void HandleTileClicked(TileView tv)
        {
            if (debugController == null)
            {
                Debug.LogWarning("[HandView] No debugController assigned to handle tile clicks.");
                return;
            }

            if (tv.Instance == null)
            {
                Debug.LogWarning("[HandView] Clicked tile has no instance bound.");
                return;
            }

            debugController.OnTileClickedFromHand(seatIndex, tv.Instance);
        }
    }
}
