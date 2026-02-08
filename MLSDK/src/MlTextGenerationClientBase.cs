using System.Text;
using MLAgentSDK.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLAgentSDK
{
    public abstract class MlTextGenerationClientBase
    {
        protected const string MessagesKey = "messages";
        private readonly HttpClient _client;
        private readonly string _url;

        protected readonly Dictionary<string, object> GenerationDataCache = new();

        public MlTextGenerationClientBase(string url, GenerationConfig generationConfig)
        {
            _client = new HttpClient();
            _url = url;

            foreach (var parameter in generationConfig.GenerationParameters)
            {
                GenerationDataCache[parameter.Key] = parameter.Value;
            }

            GenerationDataCache["mode"] = "chat";
        }

        protected async Task<GenerationResult<string>> SendApiRequest() =>
            await SendApiRequestInternal(JsonConvert.SerializeObject(GenerationDataCache));

        private async Task<GenerationResult<string>> SendApiRequestInternal(string json)
        {
            using (HttpContent content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                try
                {
                    var response = await _client.PostAsync(_url, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = $"{response.StatusCode}: {response.ReasonPhrase}\r\n";
                        return new GenerationResult<string>(false, string.Empty, errorMessage);
                    }

                    var jsonTask = response.Content.ReadAsStringAsync();
                    var result = JObject.Parse(jsonTask.Result);

                    response.Dispose();

                    var choices = result["choices"];
                    var message = choices[0]["message"];

                    var parsedResult = message["content"].ToString();

                    var formattedResult = System.Net.WebUtility.HtmlDecode(parsedResult);

                    if (string.IsNullOrEmpty(formattedResult))
                    {
                        return new GenerationResult<string>(false, string.Empty, "Empty response");
                    }

                    return new GenerationResult<string>(true, formattedResult, string.Empty);
                }
                catch (HttpRequestException e)
                {
                    if (e.InnerException != null)
                        return new GenerationResult<string>(false, string.Empty, e.InnerException.Message);

                    return new GenerationResult<string>(false, string.Empty, e.Message);
                }
            }
        }
    }
}