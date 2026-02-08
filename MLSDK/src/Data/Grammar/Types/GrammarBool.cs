namespace MLAgentSDK.Data.Grammar.Types;

public class GrammarBool : GrammarType
{
    public GrammarBool(string name, bool isRequired) : base(name, SchemaType.Boolean, isRequired) { }
}