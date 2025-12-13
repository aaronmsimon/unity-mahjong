namespace MJ.GameLogic
{
    /// <summary>
    /// Result of action validation
    /// Contains whether action is valid and reason if not
    /// </summary>
    public class ActionValidationResult
    {
        public bool IsValid { get; private set; }
        public string Reason { get; private set; }

        private ActionValidationResult(bool isValid, string reason = null)
        {
            IsValid = isValid;
            Reason = reason;
        }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public static ActionValidationResult Success()
        {
            return new ActionValidationResult(true);
        }

        /// <summary>
        /// Creates a failed validation result with reason
        /// </summary>
        public static ActionValidationResult Fail(string reason)
        {
            return new ActionValidationResult(false, reason);
        }

        public override string ToString()
        {
            return IsValid ? "Valid" : $"Invalid: {Reason}";
        }
    }
}
