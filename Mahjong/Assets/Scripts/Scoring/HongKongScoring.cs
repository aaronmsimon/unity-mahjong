using MJ.Core.Hand;

namespace MJ.Scoring
{
    /// <summary>
    /// Hong Kong Style Mahjong scoring system
    /// Uses fan-based scoring with minimum 3 fan to win
    /// </summary>
    public class HongKongScoring : IScoringSystem
    {
        public string SystemName => "Hong Kong Style";

        /// <summary>
        /// Minimum fan required to win in Hong Kong rules
        /// </summary>
        public const int MinimumFan = 3;

        public ScoreResult CalculateScore(Hand hand, ScoringContext context)
        {
            ScoreResult result = new ScoreResult();

            // Calculate total fan
            int totalFan = CalculateFan(hand, context, result);

            result.Fan = totalFan;
            result.Points = ConvertFanToPoints(totalFan);

            return result;
        }

        /// <summary>
        /// Calculates the fan value for a hand
        /// Also populates the result with patterns detected
        /// </summary>
        private int CalculateFan(Hand hand, ScoringContext context, ScoreResult result)
        {
            int fan = 0;

            // Self-drawn win: +1 fan
            if (context.IsSelfDrawn)
            {
                fan += 1;
                result.AddPattern("Self Draw", 1);
            }

            // All Pongs: +3 fan
            if (ScoringPatterns.IsAllPongs(hand))
            {
                fan += 3;
                result.AddPattern("All Pongs", 3);
            }

            // All Concealed: +1 fan (only if self-drawn)
            if (context.IsSelfDrawn && ScoringPatterns.IsAllConcealed(hand))
            {
                fan += 1;
                result.AddPattern("All Concealed", 1);
            }

            // Mixed One Suit (one suit + honors): +3 fan
            if (ScoringPatterns.IsMixedOneSuit(hand))
            {
                fan += 3;
                result.AddPattern("Mixed One Suit", 3);
            }

            // All One Suit (pure, no honors): +7 fan
            // Note: This overrides Mixed One Suit
            if (ScoringPatterns.IsAllOneSuit(hand) && !ScoringPatterns.IsMixedOneSuit(hand))
            {
                // Remove Mixed One Suit if it was added
                if (result.Patterns.Contains("Mixed One Suit"))
                {
                    fan -= 3;
                    result.Patterns.Remove("Mixed One Suit");
                    result.FanBreakdown.Remove("Mixed One Suit");
                }
                
                fan += 7;
                result.AddPattern("All One Suit (Pure)", 7);
            }

            // All Terminals and Honors: +10 fan
            if (ScoringPatterns.IsAllTerminalsAndHonors(hand))
            {
                fan += 10;
                result.AddPattern("All Terminals and Honors", 10);
            }

            // All Simples (no terminals or honors): +1 fan
            if (ScoringPatterns.IsAllSimples(hand))
            {
                fan += 1;
                result.AddPattern("All Simples", 1);
            }

            // Dragon Pong: +1 fan per dragon Pong
            if (ScoringPatterns.HasDragonPong(hand))
            {
                fan += 1;
                result.AddPattern("Dragon Pong", 1);
            }

            // Prevailing Wind Pong: +1 fan
            if (ScoringPatterns.HasPrevailingWindPong(hand, context.PrevailingWind))
            {
                fan += 1;
                result.AddPattern("Prevailing Wind Pong", 1);
            }

            // Seat Wind Pong: +1 fan
            if (ScoringPatterns.HasSeatWindPong(hand, context.SeatWind))
            {
                fan += 1;
                result.AddPattern("Seat Wind Pong", 1);
            }

            // Bonus tiles: +1 fan per bonus tile
            if (context.BonusTileCount > 0)
            {
                fan += context.BonusTileCount;
                result.AddPattern($"{context.BonusTileCount} Bonus Tile(s)", context.BonusTileCount);
            }

            // Kong: +1 fan per Kong
            int kongCount = ScoringPatterns.CountKongs(hand);
            if (kongCount > 0)
            {
                fan += kongCount;
                result.AddPattern($"{kongCount} Kong(s)", kongCount);
            }

            // Last tile from wall: +1 fan
            if (context.IsLastTileFromWall)
            {
                fan += 1;
                result.AddPattern("Last Tile from Wall", 1);
            }

            // Replacement tile (after Kong): +1 fan
            if (context.IsReplacementTile)
            {
                fan += 1;
                result.AddPattern("Replacement Tile", 1);
            }

            // Robbing Kong: +1 fan
            if (context.IsRobbingKong)
            {
                fan += 1;
                result.AddPattern("Robbing Kong", 1);
            }

            return fan;
        }

        /// <summary>
        /// Converts fan to point value using Hong Kong exponential scale
        /// </summary>
        private int ConvertFanToPoints(int fan)
        {
            return fan switch
            {
                0 => 0,
                1 => 0,
                2 => 0,
                3 => 1,      // Minimum to win
                4 => 2,
                5 => 4,
                6 => 8,
                7 => 16,
                8 => 32,
                9 => 64,
                10 => 128,
                >= 11 => 256,  // Maximum hand
                _ => 0
            };
        }
    }
}
