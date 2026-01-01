using System.Collections.Generic;
using UnityEngine;
using MJ.Core.Tiles;
using MJ.GameFlow;

namespace MJ.Core.Wall
{
    /// <summary>
    /// Represents the wall of tiles in Mahjong
    /// Manages the draw pile, dead wall, and tile distribution
    /// </summary>
    public class Wall
    {
        // The main draw pile (tiles available to draw)
        private List<TileInstance> drawPile;
        
        // The dead wall (reserved tiles at the end, used for Kong replacements)
        private List<TileInstance> deadWall;
        
        // Tiles that have been drawn from the wall
        private List<TileInstance> drawnTiles;
        
        // Configuration
        private int deadWallSize;
        
        public int DrawPileCount => drawPile.Count;
        public int DeadWallCount => deadWall.Count;
        public int TotalRemainingTiles => drawPile.Count + deadWall.Count;
        public bool IsEmpty => drawPile.Count == 0;

        /// <summary>
        /// Creates a new Wall with the specified tiles
        /// </summary>
        /// <param name="tiles">All tiles to put in the wall (should be shuffled)</param>
        /// <param name="deadWallSize">Size of the dead wall (default 14 for Hong Kong rules)</param>
        public Wall(List<TileInstance> tiles, int deadWallSize = 14)
        {
            if (tiles == null || tiles.Count == 0)
            {
                Debug.LogError("Cannot create wall with null or empty tile list");
                return;
            }

            this.deadWallSize = deadWallSize;
            drawnTiles = new List<TileInstance>();
            
            // Split tiles into draw pile and dead wall
            int drawPileSize = tiles.Count - deadWallSize;
            
            if (drawPileSize < 0)
            {
                Debug.LogError($"Not enough tiles for dead wall. Need at least {deadWallSize} tiles.");
                drawPileSize = tiles.Count;
                deadWallSize = 0;
            }

            drawPile = tiles.GetRange(0, drawPileSize);
            deadWall = tiles.GetRange(drawPileSize, tiles.Count - drawPileSize);

            foreach (var tile in drawPile)
            {
                tile.SetLocation(LocationType.Wall);
            }
            foreach (var tile in deadWall)
            {
                tile.SetLocation(LocationType.DeadWall);
            }

            Debug.Log($"Wall created: {drawPile.Count} in draw pile, {deadWall.Count} in dead wall");
        }

        #region Drawing Tiles

        /// <summary>
        /// Draws a single tile from the front of the wall
        /// </summary>
        /// <returns>The drawn tile, or null if draw pile is empty</returns>
        public TileInstance DrawTile()
        {
            if (drawPile.Count == 0)
            {
                Debug.LogWarning("Cannot draw tile: draw pile is empty");
                return null;
            }

            TileInstance tile = drawPile[0];
            drawPile.RemoveAt(0);
            drawnTiles.Add(tile);

            tile.SetLocation(LocationType.Unknown);
            
            return tile;
        }

        /// <summary>
        /// Draws multiple tiles from the front of the wall
        /// </summary>
        /// <param name="count">Number of tiles to draw</param>
        /// <returns>List of drawn tiles (may be less than requested if wall runs out)</returns>
        public List<TileInstance> DrawTiles(int count)
        {
            List<TileInstance> drawn = new List<TileInstance>();
            
            for (int i = 0; i < count && drawPile.Count > 0; i++)
            {
                drawn.Add(DrawTile());
            }

            if (drawn.Count < count)
            {
                Debug.LogWarning($"Requested {count} tiles but only {drawn.Count} available");
            }

            return drawn;
        }

        /// <summary>
        /// Draws a tile from the back of the wall (used for initial deal in some variants)
        /// </summary>
        /// <returns>The drawn tile, or null if draw pile is empty</returns>
        public TileInstance DrawTileFromBack()
        {
            if (drawPile.Count == 0)
            {
                Debug.LogWarning("Cannot draw tile from back: draw pile is empty");
                return null;
            }

            int lastIndex = drawPile.Count - 1;
            TileInstance tile = drawPile[lastIndex];
            drawPile.RemoveAt(lastIndex);
            drawnTiles.Add(tile);
            
            return tile;
        }

        #endregion

        #region Dead Wall Management

        /// <summary>
        /// Draws a replacement tile from the dead wall (used after declaring Kong)
        /// </summary>
        /// <returns>The replacement tile, or null if dead wall is empty</returns>
        public TileInstance DrawReplacementTile()
        {
            if (deadWall.Count == 0)
            {
                Debug.LogWarning("Cannot draw replacement tile: dead wall is empty");
                return null;
            }

            // Draw from the front of the dead wall
            TileInstance tile = deadWall[0];
            deadWall.RemoveAt(0);
            drawnTiles.Add(tile);

            tile.SetLocation(LocationType.Unknown);
            
            // Replenish dead wall from draw pile if possible
            if (drawPile.Count > 0)
            {
                TileInstance replenish = drawPile[drawPile.Count - 1];
                drawPile.RemoveAt(drawPile.Count - 1);
                deadWall.Add(replenish);
                replenish.SetLocation(LocationType.DeadWall);
            }

            return tile;
        }

        /// <summary>
        /// Peeks at the indicator tiles (dora indicators) without removing them
        /// Used in Japanese Mahjong for dora system
        /// </summary>
        /// <param name="count">Number of indicators to peek at</param>
        /// <returns>List of indicator tiles (read-only)</returns>
        public List<TileInstance> PeekIndicatorTiles(int count)
        {
            int availableCount = Mathf.Min(count, deadWall.Count);
            return deadWall.GetRange(0, availableCount);
        }

        #endregion

        #region Initial Distribution

        /// <summary>
        /// Deals initial hands to all players
        /// Returns a list of hands (one per player) with 13 tiles each
        /// </summary>
        /// <param name="playerCount">Number of players (typically 4)</param>
        /// <param name="tilesPerPlayer">Tiles per player (13 for Hong Kong, 16 for some variants)</param>
        /// <returns>List of tile lists, one for each player</returns>
        public List<List<TileInstance>> DealInitialHands(int playerCount = 4, int tilesPerPlayer = 13)
        {
            List<List<TileInstance>> hands = new List<List<TileInstance>>();
            
            // Initialize empty hands
            for (int i = 0; i < playerCount; i++)
            {
                hands.Add(new List<TileInstance>());
            }

            // Deal tiles in rounds (typically 4 tiles at a time, then singles)
            int totalTilesNeeded = playerCount * tilesPerPlayer;
            
            if (drawPile.Count < totalTilesNeeded)
            {
                Debug.LogError($"Not enough tiles to deal. Need {totalTilesNeeded}, have {drawPile.Count}");
                return hands;
            }

            // Deal in blocks of 4 tiles to each player
            int fullRounds = tilesPerPlayer / 4;
            for (int round = 0; round < fullRounds; round++)
            {
                for (int player = 0; player < playerCount; player++)
                {
                    hands[player].AddRange(DrawTiles(4));
                }
            }

            // Deal remaining tiles one at a time
            int remainingTiles = tilesPerPlayer % 4;
            for (int i = 0; i < remainingTiles; i++)
            {
                for (int player = 0; player < playerCount; player++)
                {
                    hands[player].Add(DrawTile());
                }
            }

            return hands;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if there are enough tiles to continue playing
        /// Some rules end the game if the wall gets too low
        /// </summary>
        /// <param name="minimumTiles">Minimum tiles required to continue</param>
        /// <returns>True if enough tiles remain</returns>
        public bool HasEnoughTilesToContinue(int minimumTiles = 0)
        {
            return drawPile.Count > minimumTiles;
        }

        /// <summary>
        /// Gets a read-only view of the current draw pile (for debugging)
        /// </summary>
        public IReadOnlyList<TileInstance> GetDrawPile()
        {
            return drawPile.AsReadOnly();
        }

        /// <summary>
        /// Gets a read-only view of the dead wall (for debugging)
        /// </summary>
        public IReadOnlyList<TileInstance> GetDeadWall()
        {
            return deadWall.AsReadOnly();
        }

        /// <summary>
        /// Gets all tiles that have been drawn from the wall
        /// </summary>
        public IReadOnlyList<TileInstance> GetDrawnTiles()
        {
            return drawnTiles.AsReadOnly();
        }

        /// <summary>
        /// Resets the wall with new tiles (for starting a new round)
        /// </summary>
        public void Reset(List<TileInstance> tiles)
        {
            int drawPileSize = tiles.Count - deadWallSize;
            
            drawPile = tiles.GetRange(0, drawPileSize);
            deadWall = tiles.GetRange(drawPileSize, tiles.Count - drawPileSize);
            drawnTiles.Clear();

            Debug.Log($"Wall reset: {drawPile.Count} in draw pile, {deadWall.Count} in dead wall");
        }
        
        /// <summary>
        /// Removes a specific tile from the draw pile (for debug/testing)
        /// </summary>
        public bool RemoveTileFromDrawPile(TileInstance tile)
        {
            if (drawPile.Contains(tile))
            {
                drawPile.Remove(tile);
                tile.SetLocation(LocationType.Unknown);
                Debug.Log($"Removed {tile.Data} from draw pile");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a specific tile from the dead wall (for debug/testing)
        /// </summary>
        public bool RemoveTileFromDeadWall(TileInstance tile)
        {
            if (deadWall.Contains(tile))
            {
                deadWall.Remove(tile);
                tile.SetLocation(LocationType.Unknown);
                Debug.Log($"Removed {tile.Data} from dead wall");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a tile to the draw pile (for debug/testing)
        /// Adds to the end of the pile
        /// </summary>
        public void AddTileToDrawPile(TileInstance tile)
        {
            drawPile.Add(tile);
            tile.SetLocation(LocationType.Wall);
            Debug.Log($"Added {tile.Data} to draw pile");
        }

        /// <summary>
        /// Adds a tile to the dead wall (for debug/testing)
        /// </summary>
        public void AddTileToDeadWall(TileInstance tile)
        {
            deadWall.Add(tile);
            tile.SetLocation(LocationType.DeadWall);
            Debug.Log($"Added {tile.Data} to dead wall");
        }

        #endregion

        #region Debug

        /// <summary>
        /// Prints the current state of the wall
        /// </summary>
        public void DebugPrint()
        {
            Debug.Log("=== Wall Debug ===");
            Debug.Log($"Draw Pile: {drawPile.Count} tiles");
            Debug.Log($"Dead Wall: {deadWall.Count} tiles");
            Debug.Log($"Drawn: {drawnTiles.Count} tiles");
            Debug.Log($"Total Remaining: {TotalRemainingTiles} tiles");
        }

        /// <summary>
        /// Returns a summary string of the wall state
        /// </summary>
        public override string ToString()
        {
            return $"Wall [Draw: {drawPile.Count}, Dead: {deadWall.Count}, Drawn: {drawnTiles.Count}]";
        }

        #endregion
    }
}
