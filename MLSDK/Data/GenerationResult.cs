using Newtonsoft.Json;

namespace MlSDK.Data;

public struct GenerationResult
{
    public bool IsGenerationSucceed { get; }
    private Dictionary<string, string> _result;

    private const string TextGenerationResultKey = "generation_result";
    private const string HistoryKey = "history";
    
    internal GenerationResult(bool isGenerationSucceed, string result)
    {
        IsGenerationSucceed = isGenerationSucceed;

        if (isGenerationSucceed)
        {
            _result = JsonConvert.DeserializeObject<Dictionary<string, string>>(result) ?? new Dictionary<string, string>();
        }
        else
        {
            _result = new Dictionary<string, string>();
        }
    }

    public bool TryGetGenerationResult(out string generationResult)
    {
        generationResult = string.Empty;

        if (!IsGenerationSucceed)
            return false;

        if (!_result.TryGetValue(TextGenerationResultKey, out generationResult))
            return false;

        return true;
    }

    public bool TryGetHistory(out History history)
    {
        history = null;

        if (!IsGenerationSucceed)
            return false;

        if (!_result.TryGetValue(HistoryKey, out string rawHistory))
            return false;

        history = JsonConvert.DeserializeObject<History>(rawHistory);

        return history != null;
    }
}