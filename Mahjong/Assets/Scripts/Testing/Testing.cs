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
        [SerializeField] private TileDefinitionSO tileDefinition;
        [SerializeField] private TileView tileView;
        [SerializeField] private bool faceUp;

        [Header("Settings")]
        [SerializeField] private InputReader inputReader;
        
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("create tile view");

            TileInstance tileInstance = new TileInstance(1, tileDefinition);
            tileView.Bind(tileInstance, faceUp);
        }

        private void OnEnable() {
            inputReader.submitEvent += OnSubmit;
        }

        private void OnDisable() {
            inputReader.submitEvent -= OnSubmit;
        }

        private void OnSubmit(Vector2 screenPos, InputDevice device) {
            Debug.Log($"screen position: {screenPos} by {device}");
        }
    }
}
