using MlSDK.Data;
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
        
        public MlTextGenerationClient(string url, AuthData authData)
        {
            _url = url;
            _authData = authData;
            _client = new HttpClient();
        }

        public async Task<History> GetHistoryRequest(string characterName)
        {
            _queryBuffer.Clear();
            
            InsertAuthData();
            InsertCommand("get_history", characterName);

            var result = await SendRequestInternal(_queryBuffer, ExecuteUrl);

            if (!result.TryGetValue(ResultKey, out string? rawResult) || string.IsNullOrEmpty(rawResult))
            {
                Console.WriteLine($"Internal request error");
                return new History();
            }

            var history = JsonConvert.DeserializeObject<History>(rawResult);

            if (history == null)
            {
                Console.WriteLine($"Internal request error");
                return new History();
            }

            return history;
        }
        
        public async Task<GenerationResult> SendGenerationRequest(string promt, bool useHistory = false)
        {
            _queryBuffer.Clear();
            
            InsertAuthData();
            InsertPromt(promt);
            InsertUseHistory(useHistory);

            return await SendGenerationRequestInternal(_queryBuffer, GenerateUrl);
        }
        
        public async Task<GenerationResult> SendGenerationRequest(string promt, string characterName, bool useHistory = false)
        {
            _queryBuffer.Clear();
            
            InsertAuthData();
            InsertPromt(promt);
            InsertUseHistory(useHistory);
            InsertCharacterName(characterName);

            return await SendGenerationRequestInternal(_queryBuffer, GenerateUrl);
        }

        public async Task<GenerationResult> SendRegenerationRequest(string characterName)
        {
            var history = await GetHistoryRequest(characterName);

            if (history.Messages.Count <= 0)
            {
                string message = $"Character {characterName} does not contains any history";
                Console.WriteLine(message);
                return new GenerationResult(false, message);
            }
            
            _queryBuffer.Clear();
            InsertAuthData();
            InsertCharacterHistory(history);
            InsertCharacterName(characterName);

            return await SendGenerationRequestInternal(_queryBuffer, RegenerateUrl);
        }

        private async Task<GenerationResult> SendGenerationRequestInternal(Dictionary<string, string> query, string url)
        {
            var json = JsonConvert.SerializeObject(query);

            using (HttpContent content = new StringContent(json))
            {
                var response = await _client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                    return new GenerationResult(false, string.Empty);

                var responseResult = await response.Content.ReadAsStringAsync();
                return new GenerationResult(true, responseResult);
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

        private void InsertCharacterHistory(History history)
        {
            _queryBuffer.Add("history", JsonConvert.SerializeObject(history));
        }

        private void InsertCharacterName(string characterName)
        {
            _queryBuffer.Add("character", characterName);
        }
        
        private void InsertAuthData()
        {
            _queryBuffer.Add("username", _authData.Username);
            _queryBuffer.Add("password", _authData.Password);
        }
    }
}

