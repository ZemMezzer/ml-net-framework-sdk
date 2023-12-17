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
        
        public MlTextGenerationClient(string url, AuthData authData)
        {
            _url = url;
            _authData = authData;
            _client = new HttpClient();
        }

        public Task<GenerationResult> SendGenerationRequest(string promt, bool useHistory = false)
        {
            _queryBuffer.Clear();
            
            InsertAuthData();
            InsertPromt(promt);
            InsertUseHistory(useHistory);

            return SendGenerationRequestInternal(_queryBuffer);
        }

        private async Task<GenerationResult> SendGenerationRequestInternal(Dictionary<string, string> query)
        {
            var json = JsonConvert.SerializeObject(query);

            using (HttpContent content = new StringContent(json))
            {
                var response = await _client.PostAsync(_url, content);

                if (!response.IsSuccessStatusCode)
                    return new GenerationResult(false, string.Empty);

                var responseResult = await response.Content.ReadAsStringAsync();
                return new GenerationResult(true, responseResult);
            }
        }

        private void InsertPromt(string promt)
        {
            _queryBuffer.Add("promt", promt);
        }

        private void InsertUseHistory(bool useHistory)
        {
            _queryBuffer.Add("use_history", useHistory.ToString());
        }

        private void InsertAuthData()
        {
            _queryBuffer.Add("username", _authData.Username);
            _queryBuffer.Add("password", _authData.Password);
        }
    }
}

