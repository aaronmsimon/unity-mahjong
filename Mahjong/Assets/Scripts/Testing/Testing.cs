using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using MJ.Core.Tiles;
using MJ.Input;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        [SerializeField] private TileSetup[] tileSetups;
        [SerializeField] private bool faceUp;

        [Header("Settings")]
        [SerializeField] private InputReader inputReader;
        
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("create tile view");

            int nextInstanceID = 0;
            foreach (TileSetup tileSetup in tileSetups) {
                TileInstance tileInstance = new TileInstance(nextInstanceID++, tileSetup.tileDefinition);
                tileSetup.tileView.Bind(tileInstance, faceUp);
            }
        }

        private void OnEnable() {
            inputReader.submitEvent += OnSubmit;
            TileView.Clicked += OnClick;
        }

        private void OnDisable() {
            inputReader.submitEvent -= OnSubmit;
            TileView.Clicked -= OnClick;
        }

        private void OnSubmit(Vector2 screenPos, InputDevice device) {
        }

        private void OnClick(int instanceID) {
            Debug.Log($"Instance ID {instanceID} clicked");
        }
    }

    [Serializable]
    public class TileSetup
    {
        public TileDefinitionSO tileDefinition;
        public TileView tileView;        
    }
}
