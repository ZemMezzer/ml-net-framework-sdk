using MLAgentSDK.Data;

namespace MLAgentSDK
{
    public class MlTextGenerationClient : MlTextGenerationClientBase
    {
        public History CurrentHistory { get; set; }
        public CharacterData CharacterData { get; private set; }
        
        public MlTextGenerationClient(string url, CharacterData characterData, GenerationConfig generationConfig) : base(url, generationConfig)
        {
            CharacterData = characterData;
        }

        public void SetContext(string context)
        {
            GenerationDataCache["context"] = context;
        }

        public string GetContext()
        {
            if (GenerationDataCache.TryGetValue("context", out var value))
            {
                return value.ToString();
            }
            
            return string.Empty;
        }
        
        /// <summary>
        /// Use for structured output on llama.cpp
        /// </summary>
        /// <param name="grammar">GBNF grammar</param>
        public void SetGrammar(string grammar)
        {
            GenerationDataCache["grammar_string"] = grammar;
        }

        /// <summary>
        /// Use for structured output on OpenAI API
        /// </summary>
        /// <param name="format">Json schema</param>
        public void SetResponseFormat(string format)
        {
            GenerationDataCache["response_format"] = format;
        }

        public void RemoveGrammar()
        {
            GenerationDataCache.Remove("grammar_string");
        }
        
        public async Task<GenerationResult<string>> SendGenerationRequest(string promt = "", bool useHistory = false,
            bool updateHistory = true)
        {
            if (useHistory && CurrentHistory == null)
            {
                return new GenerationResult<string>(false, string.Empty, "Null history");
            }

            var history = useHistory ? CurrentHistory : new History(string.Empty);
            return await SendGenerationRequestInternal(promt, history, updateHistory && useHistory);
        }

        public async Task<GenerationResult<string>> SendRegenerationRequest(bool updateHistory = true)
        {
            if (CurrentHistory == null || CurrentHistory.Messages.Count <= 0)
            {
                return new GenerationResult<string>(false, string.Empty, "History can't be null or empty on regenerate request, use generate request instead");
            }

            var lastMessage = CurrentHistory.GetLastMessage();
            
            if (!lastMessage.HasValue || lastMessage.Value.IsUserMessage)
            {
                return new GenerationResult<string>(false, string.Empty,
                    "Regenerate requests can only be send on model message");
            }

            var history = CurrentHistory.GetCopy();

            var promt = CurrentHistory.IsLastMessageResponseOnPromt() ? history.GetLastPromt().Content : string.Empty;
            
            history.RemoveLastCharacterMessage();
            
            if(!string.IsNullOrEmpty(promt))
                history.RemoveLastUserMessage();

            return await SendGenerationRequestInternal(promt, history, updateHistory);
        }
        
        private async Task<GenerationResult<string>> SendGenerationRequestInternal(string promt, History history, bool updateHistory)
        {
            var requestHistory = history.GetCopy();
            
            if(!string.IsNullOrEmpty(promt))
                requestHistory.AddPromt(promt);

            GenerationDataCache[MessagesKey] = requestHistory.Messages;

            var result = await SendApiRequest();

            if (!result.IsGenerationSucceed)
                return result;

            if (!updateHistory) 
                return result;
            
            requestHistory.SetModelMessage(result.Result);
            CurrentHistory = requestHistory;

            return result;
        }
    }
}

