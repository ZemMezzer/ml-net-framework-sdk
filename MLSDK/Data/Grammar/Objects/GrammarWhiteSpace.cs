using MLSDK.Data.Grammar.Types;

namespace MLSDK.Data.Grammar.Objects;

internal class GrammarWhiteSpace : GrammarNonDeclarationType
{
    private const string WhiteSpace = "\" \"";
    public GrammarWhiteSpace() : base("ws", WhiteSpace) {}
}