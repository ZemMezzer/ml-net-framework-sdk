using Llama.Grammar.Helper;
using Llama.Grammar.Service;

namespace MLAgentSDK.Data.Grammar
{
    public class GBNFGrammarBuilder : GrammarBuilder
    {
        protected override string BuildInternal(SchemaBuilder root)
        {
            return new GbnfGrammar().ConvertJsonSchemaToGbnf(root.ToJson());
        }
    }
}

