using MJ.Core.Hand;

namespace MJ.Scoring
{
    /// <summary>
    /// Interface for Mahjong scoring systems
    /// Different rule sets implement different scoring logic
    /// </summary>
    public interface IScoringSystem
    {
        /// <summary>
        /// Name of the scoring system
        /// </summary>
        string SystemName { get; }

        /// <summary>
        /// Calculates the score for a winning hand
        /// </summary>
        /// <param name="hand">The winning hand</param>
        /// <param name="context">Context about how the win occurred</param>
        /// <returns>Score result with points, patterns, and details</returns>
        ScoreResult CalculateScore(Hand hand, ScoringContext context);
    }
}
