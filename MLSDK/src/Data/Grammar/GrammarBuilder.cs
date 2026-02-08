using Llama.Grammar.Helper;
using MLAgentSDK.Data.Grammar.Types;

namespace MLAgentSDK.Data.Grammar
{
    public abstract class GrammarBuilder
    {
        private readonly HashSet<GrammarType> _types = new();
        private readonly List<string> _requiredTypes = new();
        
        public string Build()
        {
            var builder = new SchemaBuilder();
            builder.Type(GrammarType.SchemaTypeToString(GrammarType.SchemaType.Object));
            builder.Properties(properties =>
            {
                foreach (var type in _types)
                {
                    type.AppendGrammar(properties);
                
                    if(type.IsRequired)
                        _requiredTypes.Add(type.Name);
                }
            });

            builder.Required(_requiredTypes.ToArray());

            return BuildInternal(builder);
        }

        public void AddType(GrammarType type)
        {
            _types.Add(type);
        }
        
        protected abstract string BuildInternal(SchemaBuilder root);
    }
}