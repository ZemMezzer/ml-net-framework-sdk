namespace MLSDK.Data.Grammar.Types;

public class GrammarNonDeclarationType : GrammarType
{
    public GrammarNonDeclarationType(string name, string value) : base(name, value) { }
    
    internal override string GenerateType()
    {
        return $"{Name} {AssignmentOperator} {Declaration}\r\n";
    }
}