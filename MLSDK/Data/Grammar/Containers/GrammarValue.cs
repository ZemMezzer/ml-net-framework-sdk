using MLSDK.Data.Grammar.Types;

namespace MLSDK.Data.Grammar.Values;

public class GrammarValue
{
    protected readonly string Name;
    public IReadOnlyCollection<GrammarType> Types => _types;
    private readonly HashSet<GrammarType> _types = new();
    
    public GrammarValue(string name, GrammarType type)
    {
        Name = name;
        
        if(type != null)
            _types.Add(type);
    }

    protected void AddType(GrammarType type) => _types.Add(type);

    internal virtual string Generate()
    {
        return $"\"{Name}\" ws \":\" ws {Types.First().Name}";
    }
}