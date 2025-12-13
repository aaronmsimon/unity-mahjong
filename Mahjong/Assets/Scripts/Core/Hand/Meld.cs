using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MJ.Core.Tiles;
using MJ.Evaluation;

namespace MJ.Core.Hand
{
    /// <summary>
    /// Types of melds in Mahjong
    /// </summary>
    public enum MeldType
    {
        Pong,           // 3 identical tiles (碰)
        Kong,           // 4 identical tiles, exposed (槓)
        ConcealedKong,  // 4 identical tiles, concealed (暗槓)
        Chow            // 3 consecutive tiles of same suit (吃/上)
    }

    /// <summary>
    /// Represents a meld (set) of tiles in Mahjong
    /// Immutable once created - use factory methods to create
    /// </summary>
    public class Meld
    {
        public MeldType Type { get; private set; }
        public List<TileInstance> Tiles { get; private set; }
        public bool IsConcealed { get; private set; }
        
        // The tile that was claimed from another player (null if self-drawn)
        public TileInstance ClaimedTile { get; private set; }
        
        // Which player the tile was claimed from (0-3, null if self-drawn)
        public int? ClaimedFromPlayer { get; private set; }

        private Meld(MeldType type, List<TileInstance> tiles, bool isConcealed = false, 
                     TileInstance claimedTile = null, int? claimedFromPlayer = null)
        {
            Type = type;
            Tiles = new List<TileInstance>(tiles); // Defensive copy
            IsConcealed = isConcealed;
            ClaimedTile = claimedTile;
            ClaimedFromPlayer = claimedFromPlayer;
        }

        #region Factory Methods

        /// <summary>
        /// Creates a Pong meld (3 identical tiles)
        /// </summary>
        /// <param name="tiles">The 3 tiles forming the Pong</param>
        /// <param name="claimedTile">The tile claimed from another player (null if self-formed)</param>
        /// <param name="claimedFromPlayer">Which player the tile was claimed from (0-3)</param>
        /// <returns>A Pong meld, or null if invalid</returns>
        public static Meld CreatePong(List<TileInstance> tiles, TileInstance claimedTile = null, int? claimedFromPlayer = null)
        {
            // Use MeldValidator to check if tiles can form a Pong
            if (!MeldValidator.CanFormPong(tiles))
            {
                Debug.LogError($"Invalid Pong: tiles do not form a valid Pong");
                return null;
            }

            bool isConcealed = claimedTile == null;
            return new Meld(MeldType.Pong, tiles, isConcealed, claimedTile, claimedFromPlayer);
        }

        /// <summary>
        /// Creates a Kong meld (4 identical tiles)
        /// </summary>
        /// <param name="tiles">The 4 tiles forming the Kong</param>
        /// <param name="isConcealed">Whether this Kong is concealed</param>
        /// <param name="claimedTile">The tile claimed from another player (null if self-drawn)</param>
        /// <param name="claimedFromPlayer">Which player the tile was claimed from (0-3)</param>
        /// <returns>A Kong meld, or null if invalid</returns>
        public static Meld CreateKong(List<TileInstance> tiles, bool isConcealed = false, 
                                      TileInstance claimedTile = null, int? claimedFromPlayer = null)
        {
            // Use MeldValidator to check if tiles can form a Kong
            if (!MeldValidator.CanFormKong(tiles))
            {
                Debug.LogError($"Invalid Kong: tiles do not form a valid Kong");
                return null;
            }

            MeldType type = isConcealed ? MeldType.ConcealedKong : MeldType.Kong;
            return new Meld(type, tiles, isConcealed, claimedTile, claimedFromPlayer);
        }

        /// <summary>
        /// Creates a Chow meld (3 consecutive tiles of same suit)
        /// </summary>
        /// <param name="tiles">The 3 tiles forming the Chow (will be sorted)</param>
        /// <param name="claimedTile">The tile claimed from another player</param>
        /// <param name="claimedFromPlayer">Which player the tile was claimed from (0-3)</param>
        /// <returns>A Chow meld, or null if invalid</returns>
        public static Meld CreateChow(List<TileInstance> tiles, TileInstance claimedTile = null, int? claimedFromPlayer = null)
        {
            // Use MeldValidator to check if tiles can form a Chow
            if (!MeldValidator.CanFormChow(tiles))
            {
                Debug.LogError($"Invalid Chow: tiles do not form a valid sequence");
                return null;
            }

            // Sort tiles by number for consistent display
            List<TileInstance> sortedTiles = tiles.OrderBy(t => t.Data.Number).ToList();

            // Chows are always exposed in standard rules
            return new Meld(MeldType.Chow, sortedTiles, false, claimedTile, claimedFromPlayer);
        }

        #endregion

        #region Querying

        /// <summary>
        /// Gets the base tile type of this meld (useful for scoring)
        /// For Chow, returns the lowest tile in the sequence
        /// </summary>
        public TileData GetBaseTile()
        {
            if (Type == MeldType.Chow)
            {
                return Tiles.OrderBy(t => t.Data.Number).First().Data;
            }
            return Tiles[0].Data;
        }

        /// <summary>
        /// Gets the suit of this meld (null for honor tiles)
        /// </summary>
        public TileSuit? GetSuit()
        {
            if (Tiles[0].Data.IsHonor()) return null;
            return Tiles[0].Data.Suit;
        }

        /// <summary>
        /// Checks if this meld contains terminals (1 or 9)
        /// </summary>
        public bool ContainsTerminals()
        {
            return Tiles.Any(t => t.Data.IsTerminal());
        }

        /// <summary>
        /// Checks if this meld contains only terminals
        /// </summary>
        public bool IsAllTerminals()
        {
            return Tiles.All(t => t.Data.IsTerminal());
        }

        /// <summary>
        /// Checks if this meld contains honor tiles (Winds or Dragons)
        /// </summary>
        public bool ContainsHonors()
        {
            return Tiles.Any(t => t.Data.IsHonor());
        }

        /// <summary>
        /// Checks if this meld is all honor tiles
        /// </summary>
        public bool IsAllHonors()
        {
            return Tiles.All(t => t.Data.IsHonor());
        }

        /// <summary>
        /// Checks if this meld is all simple tiles (2-8)
        /// </summary>
        public bool IsAllSimples()
        {
            return Tiles.All(t => t.Data.IsSimple());
        }

        #endregion

        #region Modification

        /// <summary>
        /// Converts a concealed Kong to an exposed Kong
        /// Used when revealing a concealed Kong
        /// </summary>
        public void ConvertToExposedKong()
        {
            if (Type == MeldType.ConcealedKong)
            {
                Type = MeldType.Kong;
                IsConcealed = false;
            }
            else
            {
                Debug.LogWarning("Can only convert ConcealedKong to Kong");
            }
        }

        /// <summary>
        /// Adds a 4th tile to a Pong to create a Kong
        /// Returns true if successful
        /// </summary>
        public bool UpgradePongToKong(TileInstance newTile)
        {
            if (Type != MeldType.Pong)
            {
                Debug.LogError("Can only upgrade Pong to Kong");
                return false;
            }

            // Use MeldValidator to check if upgrade is valid
            if (!MeldValidator.CanUpgradePongToKong(Tiles[0].Data, newTile.Data))
            {
                Debug.LogError("New tile doesn't match the Pong");
                return false;
            }

            Tiles.Add(newTile);
            Type = MeldType.Kong;
            // Keep the same concealed status as the original Pong
            return true;
        }

        #endregion

        #region String Representation

        /// <summary>
        /// Returns a detailed string representation of the meld
        /// </summary>
        public override string ToString()
        {
            string typeStr = Type.ToString();
            if (IsConcealed)
            {
                typeStr = "Concealed " + typeStr;
            }

            string tilesStr = string.Join("-", Tiles.Select(t => t.Data.ToCompactString()));
            
            if (ClaimedTile != null && ClaimedFromPlayer.HasValue)
            {
                tilesStr += $" (claimed from P{ClaimedFromPlayer})";
            }

            return $"{typeStr}: {tilesStr}";
        }

        /// <summary>
        /// Gets a compact display format for UI
        /// Examples: "Pong: 3B", "Chow: 1-2-3D", "Kong: RD"
        /// </summary>
        public string ToCompactString()
        {
            string typeStr = Type switch
            {
                MeldType.Pong => "Pong",
                MeldType.Kong => "Kong",
                MeldType.ConcealedKong => "Hidden Kong",
                MeldType.Chow => "Chow",
                _ => Type.ToString()
            };

            if (Type == MeldType.Chow)
            {
                var sortedTiles = Tiles.OrderBy(t => t.Data.Number);
                string tilesStr = string.Join("-", sortedTiles.Select(t => t.Data.ToCompactString()));
                return $"{typeStr}: {tilesStr}";
            }
            else
            {
                TileData baseTile = GetBaseTile();
                return $"{typeStr}: {baseTile.ToCompactString()}";
            }
        }

        #endregion
    }
}
