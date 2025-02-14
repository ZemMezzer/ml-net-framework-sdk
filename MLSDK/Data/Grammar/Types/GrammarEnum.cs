namespace MLSDK.Data.Grammar.Types;

public class GrammarEnum : GrammarType
{
    public GrammarEnum(string name, params string[] values) : base(name, string.Empty)
    {
        var isFirst = true;
        var result = string.Empty;
        
        foreach (var v in values)
        {
            if (!isFirst)
                result += " | ";

            result += $"\"{v}\"";

            isFirst = false;
        }
        
        Declaration = result;
    }
}