using MLSDK.Data.Grammar.Objects;
using MLSDK.Data.Grammar.Types;
using MLSDK.Data.Grammar.Values;

namespace MLSDK.Data.Grammar;

public class GBNFGrammarBuilder : IGrammarBuilder
{
    private List<GrammarValue> _parameters = new();
    private HashSet<GrammarType> _types = new();

    public GBNFGrammarBuilder()
    {
        _types.Add(new GrammarWhiteSpace());
    }

    public string Build()
    {
        var types = string.Empty;

        foreach (var type in _types)
        {
            types += type.GenerateType();
        }

        var parameters = string.Empty;

        var isFirst = true;
        
        foreach (var parameter in _parameters)
        {
            if (!isFirst)
                parameters += "\",\" ws ";
            
            parameters += parameter.Generate();
            isFirst = false;
        }

        string result = "\"{\" " + $"{parameters}" + " \"}\" \r\n";
        result += types;

        return $"root ::= {result}";
    }

    public void AddValue(GrammarValue value)
    {
        foreach (var type in value.Types)
        {
            _types.Add(type);
        }
        
        _parameters.Add(value);
    }
}