using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MlSDK.Data;
using MLSDK.Data;
using Newtonsoft.Json;

namespace MlSDK
{
    public class MlTextGenerationClient
    {
        private readonly string _url;
        private readonly AuthData _authData;

        private readonly Dictionary<string, string> _queryBuffer = new Dictionary<string, string>();
        private readonly HttpClient _client;

        private const string ResultKey = "result";

        private string GenerateUrl => $"{_url}/api/generate";
        private string ExecuteUrl => $"{_url}/api/execute";
        private string RegenerateUrl => $"{_url}/api/regenerate";

        private readonly string _character;
        
        public MlTextGenerationClient(string url, string character, AuthData authData)
        {
            _url = url;
            _authData = authData;
            _character = character;
            _client = new HttpClient();
        }

        public async Task<History> GetHistoryRequest()
        {
            _queryBuffer.Clear();
            
            InsertAuthData();
            InsertCommand("get_history", _character);

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
            
            InsertCharacterId(_character);
            InsertAuthData();
            InsertPromt(promt);
            InsertUseHistory(useHistory);

            return await SendGenerationRequestInternal(_queryBuffer, GenerateUrl);
        }

        public async Task<GenerationResult> SendRegenerationRequest()
        {
            var history = await GetHistoryRequest();

            if (history.Messages.Count <= 0)
            {
                string message = $"Character does not contains any history";
                Console.WriteLine(message);
                return new GenerationResult(false, message);
            }
            
            _queryBuffer.Clear();
            InsertCharacterId(_character);
            InsertAuthData();
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

        private void InsertCharacterId(string characterId)
        {
            _queryBuffer.Add("character", characterId);
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

