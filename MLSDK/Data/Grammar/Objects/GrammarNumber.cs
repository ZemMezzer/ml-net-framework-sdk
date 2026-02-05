using MLSDK.Data.Grammar.Types;

namespace MLSDK.Data.Grammar.Objects;

public class GrammarNumber : GrammarNonDeclarationType, IEquatable<GrammarNumber>
{
    private const string Number = "(\"-\"? ([0-9] | [1-9] [0-9]*)) (\".\" [0-9]+)? ([eE] [-+]? [0-9]+)?";
    
    public GrammarNumber() : base("number", Number) { }
    public bool Equals(GrammarNumber? other) => true;

    public override bool Equals(object? obj) => obj is GrammarNumber;

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}