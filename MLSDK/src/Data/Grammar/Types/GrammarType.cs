using Llama.Grammar.Helper;

namespace MLAgentSDK.Data.Grammar.Types
{
    public class GrammarType
    {
        public enum SchemaType
        {
            Object,
            Array,
            String,
            Number,
            Boolean,
            Null,
        }
        
        public readonly string Name;
        public readonly SchemaType Type;
        public readonly bool IsRequired;
        
        public GrammarType(string name, SchemaType type, bool isRequired)
        {
            Name = name;
            Type = type;
            IsRequired = isRequired;
        }

        public virtual void AppendGrammar(SchemaBuilder.PropertiesBuilder properties)
        {
            properties.Add(Name, configure =>
            {
                configure.Type(SchemaTypeToString(Type));
            });
        }
        
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        internal static string SchemaTypeToString(SchemaType schemaType)
        {
            return schemaType.ToString().ToLower();
        }
    }
}
