namespace MLSDK.Data.Grammar.Types
{
    public class GrammarType
    {
        protected const string AssignmentOperator = "::=";
    
        public readonly string Name;
        public string Declaration { get; protected set; }

        public GrammarType(string name, string value)
        {
            Name = name;
            Declaration = $"{value}";
        }

        /// <summary>
        /// Generates GBNF style grammar
        /// </summary>
        /// <returns></returns>
        internal virtual string GenerateGBNFType()
        {
            var declaration = $"{Name}Dec {AssignmentOperator} {Declaration}";
            var type = $"{Name} {AssignmentOperator}  \"\\\"\" {Name}Dec \"\\\"\"";
        
            return $"{type}\r\n{declaration}\r\n";
        }

        internal virtual object GenerateJsonSchemaType()
        {
            var result = new Dictionary<string, object>()
            {
                {Name, new{type = "string"}}
            };

            return result;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
