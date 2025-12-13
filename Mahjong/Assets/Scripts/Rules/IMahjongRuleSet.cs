using MJ.Core.Hand;

namespace MJ.Rules
{
    /// <summary>
    /// Base interface for all Mahjong rule sets
    /// Different variants (Hong Kong, Japanese, Taiwanese, etc.) implement this
    /// </summary>
    public interface IMahjongRuleSet
    {
        /// <summary>
        /// Name of the rule set
        /// </summary>
        string RuleSetName { get; }

        /// <summary>
        /// Number of tiles in a standard hand
        /// 13 for Hong Kong/Japanese, 16 for Taiwanese
        /// </summary>
        int HandSize { get; }

        /// <summary>
        /// Checks if a hand is a winning hand
        /// </summary>
        bool IsWinningHand(Hand hand);
    }
}
