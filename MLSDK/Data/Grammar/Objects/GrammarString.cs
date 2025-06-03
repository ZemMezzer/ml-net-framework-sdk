using MLSDK.Data.Grammar.Types;

namespace MLSDK.Data.Grammar.Objects;

public class GrammarString : GrammarNonDeclarationType
{
    public GrammarString() : base("string", "\"\\\"\" ([a-zA-Z0-9.,:;'_ ])* \"\\\"\"") { }
}