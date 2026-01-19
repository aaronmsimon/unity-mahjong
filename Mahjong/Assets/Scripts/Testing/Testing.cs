using UnityEngine;
using System.Collections.Generic;
using MJ.Core.Tiles;
using MJ.Input;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        [SerializeField] private TileSetSO tileSetSO;
        [SerializeField] private GameObject tileContainer;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private InputReader inputReader;

        private List<Tile> tiles;

        private void Awake() {
            inputReader.EnableDebugInput();
        }

        private void Start() {
            TileSetFactory factory = new TileSetFactory(tileSetSO);
            tiles = factory.CreateTileSet();
            // factory.ShuffleTiles(tiles);
        }

        private void OnEnable() {
            inputReader.startNewGameEvent += OnStartNewGame;
        }

        private void OnDisable() {
            inputReader.startNewGameEvent -= OnStartNewGame;
        }

        private void OnStartNewGame() {
            int tilesToDeal = 5;

            for (int i = 0; i < tilesToDeal; i++) {
                Instantiate(tilePrefab, tileContainer.transform);
            }
        }
    }
}
