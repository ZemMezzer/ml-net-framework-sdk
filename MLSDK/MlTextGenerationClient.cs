using System.Text;
using MlSDK.Data;
using MLSDK.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MlSDK
{
    public class MlTextGenerationClient
    {
        private readonly string _url;
        private readonly HttpClient _client;
        public History CurrentHistory { get; set; }
        public CharacterData CharacterData { get; private set; }

        private Dictionary<string, object> _generationDataCache = new();

        private const string MessagesKey = "messages";
        
        public MlTextGenerationClient(string url, CharacterData characterData, GenerationConfig generationConfig)
        {
            _url = url;
            CharacterData = characterData;
            _client = new HttpClient();

            foreach (var parameter in generationConfig.GenerationParameters)
            {
                _generationDataCache[parameter.Key] = parameter.Value;
            }

            _generationDataCache["mode"] = "chat";
            _generationDataCache["context"] = characterData.Context;
        }

        public async Task<GenerationResult> SendGenerationRequest(string promt = "", bool useHistory = false,
            bool updateHistory = true)
        {
            if (useHistory && CurrentHistory == null)
            {
                return new GenerationResult(false, string.Empty, "Null history");
            }

            var history = useHistory ? CurrentHistory : new History(string.Empty, CharacterData.Context);
            return await SendGenerationRequestInternal(promt, history, updateHistory && useHistory);
        }

        public async Task<GenerationResult> SendRegenerationRequest(bool updateHistory = true)
        {
            if (CurrentHistory == null || CurrentHistory.Messages.Count <= 0)
            {
                return new GenerationResult(false, string.Empty, "History can't be null or empty on regenerate request, use generate request instead");
            }

            var lastMessage = CurrentHistory.GetLastMessage();
            
            if (!lastMessage.HasValue || lastMessage.Value.IsUserMessage)
            {
                return new GenerationResult(false, string.Empty,
                    "Regenerate requests can only be send on model message");
            }

            var history = CurrentHistory.GetCopy();

            var promt = CurrentHistory.IsLastMessageResponseOnPromt() ? history.GetLastPromt().Content : string.Empty;
            
            history.RemoveLastCharacterMessage();
            
            if(!string.IsNullOrEmpty(promt))
                history.RemoveLastUserMessage();

            return await SendGenerationRequestInternal(promt, history, updateHistory);
        }
        
        private async Task<GenerationResult> SendGenerationRequestInternal(string promt, History history, bool updateHistory)
        {
            var requestHistory = history.GetCopy();
            
            if(!string.IsNullOrEmpty(promt))
                requestHistory.AddPromt(promt);

            _generationDataCache[MessagesKey] = requestHistory.Messages;

            var result = await SendApiRequest(JsonConvert.SerializeObject(_generationDataCache));

            if (!result.IsGenerationSucceed)
                return result;

            if (!updateHistory) 
                return result;
            
            requestHistory.SetModelMessage(result.ResultMessage);
            CurrentHistory = requestHistory;

            return result;
        }

        private async Task<GenerationResult> SendApiRequest(string json)
        {
            using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var response = await _client.PostAsync(_url, content);

                if (!response.IsSuccessStatusCode)
                {
                    return new GenerationResult(false, string.Empty, response.ReasonPhrase);
                }

                var jsonTask = response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(jsonTask.Result);

                response.Dispose();

                var choices = result["choices"];
                var message = choices[0]["message"];

                var parsedResult = message["content"].ToString();

                var formattedResult = System.Net.WebUtility.HtmlDecode(parsedResult);

                if (string.IsNullOrEmpty(formattedResult))
                {
                    return new GenerationResult(false, string.Empty, "Empty response");
                }

                return new GenerationResult(true, formattedResult, string.Empty);
            }
        }
    }
}

