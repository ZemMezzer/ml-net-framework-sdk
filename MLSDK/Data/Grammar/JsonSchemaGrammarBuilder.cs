using MLSDK.Data.Grammar.Values;
using Newtonsoft.Json;

namespace MLSDK.Data.Grammar;

public class JsonSchemaGrammarBuilder : IGrammarBuilder
{
    private readonly List<GrammarValue> _grammarValues = new();
    private readonly Dictionary<string, object> _result = new();

    private readonly Dictionary<string, object> _schemaBuffer = new();
    private readonly List<string> _requiredBuffer = new();
    
    public string Build()
    {
        _result.Clear();
        _schemaBuffer.Clear();
        _requiredBuffer.Clear();
        
        _result["type"] = "json_schema";
        _result["json_schema"] = new Dictionary<string, object>()
        {
            {"name", "response"},
            {"strict", true},
            {"schema", new {type = "object", properties = _schemaBuffer, required = _requiredBuffer} }
        };

        foreach (var value in _grammarValues)
        {
            _schemaBuffer.Add(value.Name, value.GenerateJsonObject());
            _requiredBuffer.Add(value.Name);
        }
        
        return JsonConvert.SerializeObject(_result);
    }

    public void AddValue(GrammarValue value)
    {
        _grammarValues.Add(value);
    }
}