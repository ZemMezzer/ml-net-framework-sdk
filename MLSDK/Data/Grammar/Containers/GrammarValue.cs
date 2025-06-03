using MLSDK.Data.Grammar.Types;
using Newtonsoft.Json.Linq;

namespace MLSDK.Data.Grammar.Values;

public class GrammarValue
{
    public readonly string Name;
    public IReadOnlyCollection<GrammarType> Types => _types;
    private readonly HashSet<GrammarType> _types = new();
    
    internal string RequirementChar { get; }
    
    public GrammarValue(string name, GrammarRequirement requirement, GrammarType type)
    {
        Name = name;
        
        if(type != null)
            _types.Add(type);

        RequirementChar = requirement.ToGrammarChar();
    }

    protected void AddType(GrammarType type) => _types.Add(type);

    internal virtual string GenerateGBNF()
    {
        return $"(\"{Name}\" ws \":\" ws {Types.First().Name}\",\"){RequirementChar}";
    }

    internal virtual object GenerateJsonObject()
    {
        var result = new Dictionary<string, object>()
        {
            { "type", "object" },
            { "properties", Types.First().GenerateJsonSchemaType() }
        };
        
        return result;
    }
}