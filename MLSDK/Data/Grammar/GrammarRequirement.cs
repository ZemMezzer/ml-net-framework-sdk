namespace MLSDK.Data.Grammar
{
    public enum GrammarRequirement
    {
        /// <summary>
        /// Appears only one time
        /// </summary>
        Required,
    
        /// <summary>
        /// Repeated zero or more times
        /// </summary>
        Optional,
    
        /// <summary>
        /// Repeated one or more times
        /// </summary>
        RequiredOptional,
    }

    internal static class RequirementsExtensions
    {
        public static string ToGrammarChar(this GrammarRequirement requirement)
        {
            return requirement switch
            {
                GrammarRequirement.Required => string.Empty,
                GrammarRequirement.Optional => "*",
                GrammarRequirement.RequiredOptional => "+",
                _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, null)
            };
        }
    }
}