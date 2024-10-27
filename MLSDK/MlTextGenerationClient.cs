using MlSDK.Data;
using MLSDK.Data;
using Newtonsoft.Json;

namespace MlSDK
{
    public class MlTextGenerationClient
    {
        private readonly string _url;
        private readonly AuthData _authData;

        private readonly Dictionary<string, string> _queryBuffer = new();
        private readonly HttpClient _client;

        private const string ResultKey = "result";

        private string GenerateUrl => $"{_url}/api/generate";
        private string ExecuteUrl => $"{_url}/api/execute";
        private string RegenerateUrl => $"{_url}/api/regenerate";
        
        public History CurrentHistory { get; private set; }
        public CharacterData CharacterData { get; private set; }
        
        public MlTextGenerationClient(string url, CharacterData characterData, AuthData authData)
        {
            _url = url;
            _authData = authData;
            CharacterData = characterData;
            _client = new HttpClient();
        }

        public void SetHistory(History history) => CurrentHistory = history;
        
        public async Task<GenerationResult> SendGenerationRequest(string promt, bool useHistory = false)
        {
            if (useHistory && CurrentHistory == null)
            {
                Console.WriteLine($"You don't set any history");
            }
            
            _queryBuffer.Clear();
            
            InsertCharacter(CharacterData);
            InsertAuthData();
            InsertPromt(promt);
            InsertUseHistory(useHistory && CurrentHistory != null);
            
            if(useHistory && CurrentHistory!=null)
                InsertCharacterHistory(CurrentHistory);

            return await SendGenerationRequestInternal(_queryBuffer, GenerateUrl);
        }

        public async Task<GenerationResult> SendRegenerationRequest()
        {
            if (CurrentHistory == null || CurrentHistory.Messages.Count <= 0)
            {
                string message = $"Character does not contains any history";
                Console.WriteLine(message);
                return new GenerationResult(false, message);
            }
            
            _queryBuffer.Clear();
            InsertCharacter(CharacterData);
            InsertAuthData();
            InsertCharacterHistory(CurrentHistory);
            InsertUseHistory(true);

            return await SendGenerationRequestInternal(_queryBuffer, RegenerateUrl);
        }

        private async Task<GenerationResult> SendGenerationRequestInternal(Dictionary<string, string> query, string url)
        {
            var json = JsonConvert.SerializeObject(query);

            try
            {
                using (HttpContent content = new StringContent(json))
                {
                    var response = await _client.PostAsync(url, content);

                    if (!response.IsSuccessStatusCode)
                        return new GenerationResult(false, $"{response.StatusCode} :{response.ReasonPhrase}");

                    var responseResult = await response.Content.ReadAsStringAsync();
                    return new GenerationResult(true, responseResult);
                }
            }
            catch (Exception e)
            {
                return new GenerationResult(false, e.Message);
            }
        }

        private async Task<Dictionary<string, string?>> SendRequestInternal(Dictionary<string, string> query, string url)
        {
            var json = JsonConvert.SerializeObject(query);

            using (HttpContent content = new StringContent(json))
            {
                var response = await _client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                    return new Dictionary<string, string?>();

                var responseResult = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseResult) ??
                       new Dictionary<string, string?>();
            }
        }

        private void InsertCommand(string command, string parameter)
        {
            _queryBuffer.Add("command", command);
            _queryBuffer.Add("parameter", parameter);
        }
        
        private void InsertPromt(string promt)
        {
            _queryBuffer.Add("promt", promt);
        }

        private void InsertUseHistory(bool useHistory)
        {
            _queryBuffer.Add("use_history", useHistory.ToString());
        }

        private void InsertCharacter(CharacterData characterData)
        {
            _queryBuffer.Add("character_data", JsonConvert.SerializeObject(characterData));
        }
        
        private void InsertCharacterHistory(History history)
        {
            _queryBuffer.Add("history", JsonConvert.SerializeObject(history));
        }
        
        private void InsertAuthData()
        {
            _queryBuffer.Add("username", _authData.Username);
            _queryBuffer.Add("password", _authData.Password);
        }
    }
}

