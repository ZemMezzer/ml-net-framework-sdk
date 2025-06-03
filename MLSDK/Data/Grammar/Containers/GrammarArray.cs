using MLSDK.Data.Grammar.Types;
using MLSDK.Data.Grammar.Values;

namespace MLSDK.Data.Grammar.Containers;

public class GrammarArray : GrammarValue
{
    private readonly List<GrammarValue> _parameters = new();
    
    public GrammarArray(string name) : base(name, GrammarRequirement.Required, null) {}

    public void Add(GrammarValue value)
    {
        foreach (var type in value.Types)
        {
            AddType(type);
        }
        
        _parameters.Add(value);
    }
    
    internal override string GenerateGBNF()
    {
        var isFirst = true;

        var result = "\"{\"";
        
        foreach (var parameter in _parameters)
        {
            if (!isFirst)
                result += "\",\" ws ";

            result += parameter.GenerateGBNF();
            
            isFirst = false;
        }

        result += "\"}\"";
        
        return $"\"{Name}\" ws \":\" ws \"[\" {result} \"]\"*";
    }
}