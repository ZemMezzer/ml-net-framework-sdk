using MLSDK.Data.Grammar.Values;

namespace MLSDK.Data.Grammar.Containers;

public class GrammarObject : GrammarValue
{
    private readonly List<GrammarValue> _values = new();
    
    public GrammarObject(string name) : base(name, GrammarRequirement.Required, null) {}

    public void Add(GrammarValue value)
    {
        _values.Add(value);

        foreach (var type in value.Types)
        {
            AddType(type);
        }
    }
    
    internal override string GenerateGBNF()
    {
        var values = string.Empty;

        var isFirst = true;
        
        foreach (var value in _values)
        {
            if (!isFirst)
                values += "\",\" ws ";
            
            values += value.GenerateGBNF();
            isFirst = false;
        }

        var result = $" \"{Name}\" ws \":\" ws " + "\"{\" " + $"{values}" + " \"}\" ";

        return result;
    }
}