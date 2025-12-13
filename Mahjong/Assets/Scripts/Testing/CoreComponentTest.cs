using UnityEngine;
using System.Collections.Generic;
using MJ.Core.Tiles;
using MJ.Core.Hand;
using MJ.Core.Wall;

namespace MJ.Testing
{
    /// <summary>
    /// Simple test script to verify core Mahjong components work
    /// Attach to a GameObject to run basic tests
    /// </summary>
    public class CoreComponentTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("=== Starting Mahjong Core Component Tests ===\n");

            TestTileCreation();
            TestMeldCreation();
            TestHandManagement();
            TestWallAndDistribution();

            Debug.Log("\n=== All Tests Complete ===");
        }

        private void TestTileCreation()
        {
            Debug.Log("--- Test 1: Tile Creation ---");

            // Test creating different tile types
            TileData bamboo5 = new TileData(TileSuit.Bamboo, 5, 0);
            TileData eastWind = new TileData(WindType.East, 1);
            TileData redDragon = new TileData(DragonType.Red, 2);

            Debug.Log($"Created: {bamboo5}");
            Debug.Log($"Created: {eastWind}");
            Debug.Log($"Created: {redDragon}");

            // Test classification methods
            Debug.Log($"Red Dragon IsHonor: {redDragon.IsHonor()}");
            Debug.Log($"5 Bamboo IsSimple: {bamboo5.IsSimple()}");
            Debug.Log($"5 Bamboo IsTerminal: {bamboo5.IsTerminal()}");

            // Test TileInstance
            TileInstance tile = new TileInstance(bamboo5);
            Debug.Log($"TileInstance: {tile}");
            Debug.Log($"TileInstance IsConcealed: {tile.IsConcealed}");

            Debug.Log("✓ Tile Creation Test Passed\n");
        }

        private void TestMeldCreation()
        {
            Debug.Log("--- Test 2: Meld Creation ---");

            // Create tiles for Pong
            List<TileInstance> pongTiles = new List<TileInstance>
            {
                new TileInstance(new TileData(TileSuit.Bamboo, 3, 0)),
                new TileInstance(new TileData(TileSuit.Bamboo, 3, 1)),
                new TileInstance(new TileData(TileSuit.Bamboo, 3, 2))
            };

            Meld pong = Meld.CreatePong(pongTiles);
            if (pong != null)
            {
                Debug.Log($"Created Pong: {pong}");
                Debug.Log($"Pong compact: {pong.ToCompactString()}");
            }

            // Create tiles for Chow
            List<TileInstance> chowTiles = new List<TileInstance>
            {
                new TileInstance(new TileData(TileSuit.Dots, 1, 3)),
                new TileInstance(new TileData(TileSuit.Dots, 2, 4)),
                new TileInstance(new TileData(TileSuit.Dots, 3, 5))
            };

            Meld chow = Meld.CreateChow(chowTiles);
            if (chow != null)
            {
                Debug.Log($"Created Chow: {chow}");
                Debug.Log($"Chow contains terminals: {chow.ContainsTerminals()}");
            }

            // Test invalid meld
            List<TileInstance> invalidTiles = new List<TileInstance>
            {
                new TileInstance(new TileData(TileSuit.Bamboo, 1, 6)),
                new TileInstance(new TileData(TileSuit.Bamboo, 3, 7)),
                new TileInstance(new TileData(TileSuit.Bamboo, 5, 8))
            };

            Meld invalid = Meld.CreateChow(invalidTiles);
            Debug.Log($"Invalid Chow (should be null): {invalid}");

            Debug.Log("✓ Meld Creation Test Passed\n");
        }

        private void TestHandManagement()
        {
            Debug.Log("--- Test 3: Hand Management ---");

            Hand hand = new Hand();

            // Add some tiles
            hand.AddTile(new TileInstance(new TileData(TileSuit.Bamboo, 1, 10)));
            hand.AddTile(new TileInstance(new TileData(TileSuit.Bamboo, 2, 11)));
            hand.AddTile(new TileInstance(new TileData(TileSuit.Bamboo, 3, 12)));
            hand.AddTile(new TileInstance(new TileData(TileSuit.Characters, 5, 13)));
            hand.AddTile(new TileInstance(new TileData(TileSuit.Characters, 5, 14)));
            hand.AddTile(new TileInstance(new TileData(WindType.East, 15)));

            Debug.Log($"Hand has {hand.ConcealedTileCount} tiles");

            // Test sorting
            hand.SortTiles();
            Debug.Log("Hand sorted");

            // Test querying
            TileData bamboo2 = new TileData(TileSuit.Bamboo, 2, 0);
            Debug.Log($"Has Bamboo 2: {hand.HasTileOfType(bamboo2)}");
            Debug.Log($"Count of Characters 5: {hand.CountTilesOfType(new TileData(TileSuit.Characters, 5, 0))}");

            // Test Pong formation check
            TileData char5 = new TileData(TileSuit.Characters, 5, 0);
            Debug.Log($"Can form Pong with Characters 5: {hand.CanFormPong(char5)}");

            // Test Chow formation check
            TileData bamboo4 = new TileData(TileSuit.Bamboo, 4, 0);
            Debug.Log($"Can form Chow with Bamboo 4: {hand.CanFormChow(bamboo4)}");

            // Add a meld
            List<TileInstance> meldTiles = new List<TileInstance>
            {
                new TileInstance(new TileData(TileSuit.Dots, 7, 20)),
                new TileInstance(new TileData(TileSuit.Dots, 7, 21)),
                new TileInstance(new TileData(TileSuit.Dots, 7, 22))
            };
            Meld testMeld = Meld.CreatePong(meldTiles);
            if (testMeld != null)
            {
                hand.AddExposedMeld(testMeld);
                Debug.Log($"Added exposed meld. Total tiles: {hand.TotalTileCount}");
            }

            // Debug print
            hand.DebugPrint();

            Debug.Log("✓ Hand Management Test Passed\n");
        }

        private void TestWallAndDistribution()
        {
            Debug.Log("--- Test 4: Wall and Distribution ---");

            // Create full tile set
            List<TileInstance> allTiles = TileFactory.CreateFullTileSet(includeFlowersAndSeasons: true);
            Debug.Log($"Created {allTiles.Count} tiles");

            // Shuffle tiles
            TileFactory.ShuffleTiles(allTiles);
            Debug.Log("Tiles shuffled");

            // Create wall
            Wall wall = new Wall(allTiles, deadWallSize: 14);
            Debug.Log($"Wall created: {wall}");

            // Deal initial hands
            List<List<TileInstance>> hands = wall.DealInitialHands(playerCount: 4, tilesPerPlayer: 13);
            Debug.Log($"Dealt hands to {hands.Count} players");

            for (int i = 0; i < hands.Count; i++)
            {
                Debug.Log($"Player {i + 1} has {hands[i].Count} tiles");
                
                // Show first 3 tiles for each player
                string preview = "";
                for (int j = 0; j < Mathf.Min(3, hands[i].Count); j++)
                {
                    preview += hands[i][j].Data.ToCompactString() + " ";
                }
                Debug.Log($"  Preview: {preview}...");
            }

            // Create Hand objects and add tiles
            Hand player1Hand = new Hand();
            player1Hand.AddTiles(hands[0]);
            player1Hand.SortTiles();
            Debug.Log($"\nPlayer 1 hand after adding tiles:");
            Debug.Log($"  Concealed: {player1Hand.ConcealedTileCount}");
            Debug.Log($"  Bonus: {player1Hand.GetBonusTiles().Count}");

            // Test drawing tiles
            Debug.Log($"\nTiles in wall before draw: {wall.DrawPileCount}");
            TileInstance drawn = wall.DrawTile();
            Debug.Log($"Drew tile: {drawn.Data}");
            Debug.Log($"Tiles in wall after draw: {wall.DrawPileCount}");

            // Test replacement tile
            TileInstance replacement = wall.DrawReplacementTile();
            Debug.Log($"Drew replacement tile: {replacement.Data}");
            Debug.Log($"Dead wall count: {wall.DeadWallCount}");

            wall.DebugPrint();

            Debug.Log("✓ Wall and Distribution Test Passed\n");
        }
    }
}
