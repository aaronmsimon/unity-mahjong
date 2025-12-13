using MJ.Core.Hand;

namespace MJ.Rules
{
    /// <summary>
    /// Hong Kong Style Mahjong rules implementation
    /// Basic implementation - can be expanded
    /// </summary>
    public class HongKongRules : IMahjongRuleSet
    {
        public string RuleSetName => "Hong Kong Style";
        public int HandSize => 13;

        public bool IsWinningHand(Hand hand)
        {
            // For now, just check basic pattern
            // In full implementation, would check minimum fan requirement
            return MJ.Evaluation.HandEvaluator.CheckBasicWinPattern(hand, meldsRequired: 4);
        }
    }
}
