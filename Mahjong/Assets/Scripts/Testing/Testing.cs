using System.Collections.Generic;
using System;
using UnityEngine;
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
            inputReader.clickEvent += OnClicked;
        }

        private void OnDisable() {
            inputReader.clickEvent -= OnClicked;
        }

        private void OnClicked() {
            // Debug.Log($"the screen has been clicked");
        }
    }
}
