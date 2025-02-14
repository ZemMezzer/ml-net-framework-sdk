using MLSDK.Data.Grammar.Types;

namespace MLSDK.Data.Grammar.Objects;

public class GrammarNumber : GrammarNonDeclarationType
{
    public GrammarNumber() : base("number", "(\"-\"? ([0-9] | [1-9] [0-9]*)) (\".\" [0-9]+)? ([eE] [-+]? [0-9]+)?") { }
}