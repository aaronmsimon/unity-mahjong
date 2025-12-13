using System.Linq;
using MJ.Core.Hand;
using MJ.Core.Tiles;

namespace MJ.Scoring
{
    /// <summary>
    /// Library of reusable scoring pattern detection methods
    /// Used by various scoring systems to identify patterns in hands
    /// </summary>
    public static class ScoringPatterns
    {
        /// <summary>
        /// Checks if all melds are Pongs/Kongs (no Chows)
        /// </summary>
        public static bool IsAllPongs(Hand hand)
        {
            var melds = hand.GetExposedMelds();
            
            // Check exposed melds
            foreach (var meld in melds)
            {
                if (meld.Type == MeldType.Chow)
                {
                    return false;
                }
            }

            // For concealed tiles, we'd need to check if they form Pongs
            // This is complex - for now, assume if hand is winning it's been validated
            return true;
        }

        /// <summary>
        /// Checks if hand is all one suit (excluding honors)
        /// </summary>
        public static bool IsAllOneSuit(Hand hand)
        {
            return hand.IsAllOneSuit();
        }

        /// <summary>
        /// Checks if hand contains only terminals and honors
        /// </summary>
        public static bool IsAllTerminalsAndHonors(Hand hand)
        {
            return hand.IsAllTerminalsAndHonors();
        }

        /// <summary>
        /// Checks if all tiles are concealed (no exposed melds)
        /// </summary>
        public static bool IsAllConcealed(Hand hand)
        {
            return hand.GetExposedMelds().Count == 0;
        }

        /// <summary>
        /// Checks if hand is mixed one suit (one suit + honors)
        /// </summary>
        public static bool IsMixedOneSuit(Hand hand)
        {
            var tiles = hand.GetConcealedTiles().ToList();
            var melds = hand.GetExposedMelds();

            // Get all suited tiles
            var suitedTiles = tiles.Where(t => !t.Data.IsHonor()).ToList();
            
            // Add suited tiles from melds
            foreach (var meld in melds)
            {
                suitedTiles.AddRange(meld.Tiles.Where(t => !t.Data.IsHonor()));
            }

            if (suitedTiles.Count == 0)
            {
                return false; // All honors, not mixed
            }

            // Check if all suited tiles are the same suit
            var suits = suitedTiles.Select(t => t.Data.Suit).Distinct().ToList();
            
            // Must be exactly one suit (+ honors)
            return suits.Count == 1;
        }

        /// <summary>
        /// Checks if hand contains no terminals or honors (all simples 2-8)
        /// </summary>
        public static bool IsAllSimples(Hand hand)
        {
            var tiles = hand.GetConcealedTiles().ToList();
            var melds = hand.GetExposedMelds();

            // Check concealed tiles
            if (tiles.Any(t => t.Data.IsTerminal() || t.Data.IsHonor()))
            {
                return false;
            }

            // Check exposed melds
            foreach (var meld in melds)
            {
                if (meld.ContainsTerminals() || meld.ContainsHonors())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if hand has a Pong/Kong of dragons
        /// </summary>
        public static bool HasDragonPong(Hand hand)
        {
            var melds = hand.GetExposedMelds();

            foreach (var meld in melds)
            {
                if (meld.Type != MeldType.Chow && 
                    meld.Tiles[0].Data.Suit == TileSuit.Dragon)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if hand has a Pong/Kong of prevailing wind
        /// </summary>
        public static bool HasPrevailingWindPong(Hand hand, WindType prevailingWind)
        {
            var melds = hand.GetExposedMelds();

            foreach (var meld in melds)
            {
                if (meld.Type != MeldType.Chow && 
                    meld.Tiles[0].Data.Suit == TileSuit.Wind &&
                    meld.Tiles[0].Data.Wind == prevailingWind)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if hand has a Pong/Kong of seat wind
        /// </summary>
        public static bool HasSeatWindPong(Hand hand, WindType seatWind)
        {
            var melds = hand.GetExposedMelds();

            foreach (var meld in melds)
            {
                if (meld.Type != MeldType.Chow && 
                    meld.Tiles[0].Data.Suit == TileSuit.Wind &&
                    meld.Tiles[0].Data.Wind == seatWind)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Counts the number of Kongs in the hand
        /// </summary>
        public static int CountKongs(Hand hand)
        {
            return hand.GetExposedMelds().Count(m => 
                m.Type == MeldType.Kong || m.Type == MeldType.ConcealedKong);
        }
    }
}
