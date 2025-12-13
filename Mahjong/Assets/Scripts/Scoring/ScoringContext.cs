using MJ.Core.Tiles;

namespace MJ.Scoring
{
    /// <summary>
    /// Context information needed for scoring calculations
    /// Contains details about how the win occurred
    /// </summary>
    public class ScoringContext
    {
        /// <summary>
        /// Whether the winning tile was self-drawn
        /// </summary>
        public bool IsSelfDrawn { get; set; }

        /// <summary>
        /// The winning tile (either drawn or claimed)
        /// </summary>
        public TileData WinningTile { get; set; }

        /// <summary>
        /// Current prevailing wind (round wind)
        /// </summary>
        public WindType PrevailingWind { get; set; }

        /// <summary>
        /// Player's seat wind
        /// </summary>
        public WindType SeatWind { get; set; }

        /// <summary>
        /// Whether the player is the dealer
        /// </summary>
        public bool IsDealer { get; set; }

        /// <summary>
        /// Number of bonus tiles (flowers/seasons) the player has
        /// </summary>
        public int BonusTileCount { get; set; }

        /// <summary>
        /// Whether the win was on the last tile from the wall
        /// </summary>
        public bool IsLastTileFromWall { get; set; }

        /// <summary>
        /// Whether the win was on a replacement tile after a Kong
        /// </summary>
        public bool IsReplacementTile { get; set; }

        /// <summary>
        /// Whether the win was by robbing a Kong
        /// (claiming someone's Kong tile for a win)
        /// </summary>
        public bool IsRobbingKong { get; set; }

        public ScoringContext()
        {
            IsSelfDrawn = false;
            IsDealer = false;
            BonusTileCount = 0;
            IsLastTileFromWall = false;
            IsReplacementTile = false;
            IsRobbingKong = false;
        }
    }
}
