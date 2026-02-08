namespace MLAgentSDK.Data.Grammar.Types
{
    public class GrammarString : GrammarType
    {
        public GrammarString(string name, bool isRequired) : base(name, SchemaType.String, isRequired) { }
    }
}