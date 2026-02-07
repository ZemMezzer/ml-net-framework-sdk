using MLSDK.Data.Grammar.Types;

namespace MLSDK.Data.Grammar.Objects
{
    public class GrammarString : GrammarNonDeclarationType, IEquatable<GrammarString>
    {
        private const string String = "\"\\\"\" ([A-Za-z0-9 .!?()'/_-]*) [A-Za-z0-9] \"\\\"\"";

        public GrammarString() : base("string", String) { }

        public override bool Equals(object? obj)
        {
            return obj is GrammarString;
        }

        public override int GetHashCode()
        {
            return String.GetHashCode();
        }
        public bool Equals(GrammarString? other) => true;
    }
}