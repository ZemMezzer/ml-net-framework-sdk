using Llama.Grammar.Helper;

namespace MLAgentSDK.Data.Grammar.Types
{
    public class GrammarEnum : GrammarType
    {
        private readonly object[] _values;
        
        public GrammarEnum(string name, bool isRequired, params object[] values) : base(name, SchemaType.Object, isRequired)
        {
            _values = values;
        }

        public override void AppendGrammar(SchemaBuilder.PropertiesBuilder properties)
        {
            properties.Add(Name, configure =>
            {
                configure.Type(SchemaTypeToString(SchemaType.String)).Enum(_values);
            });

        }
    }
}