using System.Collections.Generic;

namespace MJ.Scoring
{
    /// <summary>
    /// Result of scoring calculation
    /// Contains points, fan (if applicable), and patterns that contributed
    /// </summary>
    public class ScoreResult
    {
        /// <summary>
        /// Final point value/score
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Fan value (for fan-based systems like Hong Kong)
        /// Null for systems that don't use fan
        /// </summary>
        public int? Fan { get; set; }

        /// <summary>
        /// List of scoring patterns that were detected
        /// e.g., "All Pongs", "Mixed One Suit", "Self Draw"
        /// </summary>
        public List<string> Patterns { get; set; }

        /// <summary>
        /// Breakdown of fan contributions (for debugging/display)
        /// e.g., "All Pongs: +3 fan", "Self Draw: +1 fan"
        /// </summary>
        public Dictionary<string, int> FanBreakdown { get; set; }

        public ScoreResult()
        {
            Patterns = new List<string>();
            FanBreakdown = new Dictionary<string, int>();
        }

        /// <summary>
        /// Adds a pattern and its fan contribution
        /// </summary>
        public void AddPattern(string patternName, int fanValue)
        {
            Patterns.Add(patternName);
            FanBreakdown[patternName] = fanValue;
        }

        public override string ToString()
        {
            if (Fan.HasValue)
            {
                return $"Score: {Points} ({Fan} fan) - Patterns: {string.Join(", ", Patterns)}";
            }
            return $"Score: {Points} - Patterns: {string.Join(", ", Patterns)}";
        }
    }
}
