namespace MLSDK.Data.Grammar.Types;

public class GrammarEnum : GrammarType
{
    private readonly string[] _values;
    
    public GrammarEnum(string name, params string[] values) : base(name, string.Empty)
    {
        _values = values;
        
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

    internal override object GenerateJsonSchemaType()
    {
        var anyOf = new List<Dictionary<string, object>>();
        
        var result = new Dictionary<string, object>()
        {
            {
                Name, new Dictionary<string, object>()
                {
                    {"type", "object"},
                    {"anyOf", anyOf}
                }
            }
        };

        foreach (var value in _values)
        {
            anyOf.Add(new Dictionary<string, object>()
            {
                {"properties", new Dictionary<string, object>()
                {
                    {"const", value}
                }},
                
                {"additionalProperties", false}
            });
        }

        return result;
    }
}