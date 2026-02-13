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
        [SerializeField] private TileView tilePrefab;
        [SerializeField] private Transform tileHolder;
        [SerializeField] private TileDefinitionSO[] tileDefinitions;
        [SerializeField] private bool faceUp;

        [Header("Settings")]
        [SerializeField] private InputReader inputReader;
        
        private void Start() {
            Debug.Log("-= TESTING =-");

            int nextInstanceID = 0;
            foreach (TileDefinitionSO tileDef in tileDefinitions) {
                TileView tileView = Instantiate(tilePrefab, tileHolder);
                TileInstance tileInstance = new TileInstance(nextInstanceID++, tileDef);
                tileView.Bind(tileInstance, faceUp);
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
}
