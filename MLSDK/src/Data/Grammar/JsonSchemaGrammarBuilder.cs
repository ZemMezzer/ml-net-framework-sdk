using Llama.Grammar.Helper;

namespace MLAgentSDK.Data.Grammar
{
    public class JsonSchemaGrammarBuilder : GrammarBuilder
    {
        protected override string BuildInternal(SchemaBuilder root)
        {
            return root.ToJson();
        }
    }
}