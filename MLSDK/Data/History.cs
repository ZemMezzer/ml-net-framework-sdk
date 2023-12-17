using Newtonsoft.Json;

namespace MlSDK.Data;

[Serializable]
public class History
{
    [JsonProperty("visible")] private List<List<string>> _visibleMessages;
    [JsonIgnore] public List<List<string>> Messages => _visibleMessages;
}