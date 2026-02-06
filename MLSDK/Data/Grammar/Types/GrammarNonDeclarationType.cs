namespace MLSDK.Data.Grammar.Types
{
    public class GrammarNonDeclarationType : GrammarType
    {
        public GrammarNonDeclarationType(string name, string value) : base(name, value) { }
    
        internal override string GenerateGBNFType()
        {
            return $"{Name} {AssignmentOperator} {Declaration}\r\n";
        }
    }
}