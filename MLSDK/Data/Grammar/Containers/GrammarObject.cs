using MLSDK.Data.Grammar.Values;

namespace MLSDK.Data.Grammar.Containers;

public class GrammarObject : GrammarValue
{
    private readonly List<GrammarValue> _values = new();
    
    public GrammarObject(string name) : base(name, null) {}

    public void Add(GrammarValue value)
    {
        _values.Add(value);

        foreach (var type in value.Types)
        {
            AddType(type);
        }
    }
    
    internal override string Generate()
    {
        var values = string.Empty;

        var isFirst = true;
        
        foreach (var value in _values)
        {
            if (!isFirst)
                values += "\",\" ws ";
            
            values += value.Generate();
            isFirst = false;
        }

        var result = "\"{\" " + $"{values}" + " \"}\" \r\n";

        return result;
    }
}