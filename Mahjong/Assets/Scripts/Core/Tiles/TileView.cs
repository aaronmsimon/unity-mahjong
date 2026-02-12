using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MJ.Core.Tiles
{
    [RequireComponent(typeof(Collider2D))] 
    public class TileView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Renderers")]
        [SerializeField] private Image faceImage;
        [SerializeField] private Color faceColor;
        [SerializeField] private Color backColor;

        [Header("Optional Highlight")]
        [SerializeField] private GameObject highlightObject; // e.g., child ring/quad

        public int InstanceId { get; private set; } = -1;

        private TileInstance _boundInstance;
        private Image tileShape;
        private GameObject tileFace;
        private bool _isFaceUp = true;

        // Event raised when this tile is clicked.
        public static event Action<int> Clicked;

        private void Reset()
        {
            // Auto-wire common fields in editor when added
            faceImage = GetComponentInChildren<Image>();
        }

        /// <summary>
        /// Bind this view to a tile instance (model). This is the main entry point.
        /// </summary>
        public void Bind(TileInstance instance, bool faceUp = true)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            _boundInstance = instance;
            InstanceId = instance.InstanceID;

            tileShape = transform.Find("TileShape").GetComponent<Image>();
            tileFace = transform.Find("TileFace").gameObject;

            SetFaceUp(faceUp);
            ApplyDefinition(instance.Definition);
            SetHighlighted(false);
        }

        /// <summary>
        /// Unbind before returning to a pool (optional but good practice).
        /// </summary>
        public void Unbind()
        {
            _boundInstance = null;
            InstanceId = -1;
            SetHighlighted(false);

            if (faceImage != null)
                faceImage.sprite = null;
        }

        public void SetFaceUp(bool faceUp)
        {
            _isFaceUp = faceUp;

            if (tileShape != null) tileShape.color = faceUp ? faceColor : backColor;
            if (tileFace != null) tileFace.SetActive(faceUp);
        }

        public void SetHighlighted(bool on)
        {
            if (highlightObject != null)
                highlightObject.SetActive(on);
        }

        private void ApplyDefinition(TileDefinitionSO def)
        {
            if (faceImage == null) return;

            if (_isFaceUp)
            {
                faceImage.sprite = def != null ? def.Sprite : null;
            }
        }

        // --- Click handling (3D Collider) ---
        // Simplest approach: let a central input controller raycast and call TileView.OnClicked()
        // But if you want TileView to self-handle clicks, you can do OnMouseDown (works for mouse).

        public void OnPointerClick(PointerEventData eventData) {
            if (InstanceId >= 0)
                Clicked?.Invoke(InstanceId);
        }
    }
}
