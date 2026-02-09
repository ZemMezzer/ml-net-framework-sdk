using MLAgentSDK.Data;
using MLAgentSDK.Data.Grammar;
using MLAgentSDK.Data.Grammar.Types;
using MLAgentSDK.RAG.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLAgentSDK.RAG
{
    public class MlMemoryGenerationClient : MlTextGenerationClientBase
    {
        private readonly GrammarBuilder _grammarBuilder;

        private const string MemoryTypeKey = "memoryType";

        private const string MemoryTypeDescription =
            "Specifies how long this memory should be kept and how it is intended to be used.";

        private const string MemoryImportanceKey = "memoryImportance";
        private const string MemoryImportanceDescription = "Specifies how critical this memory is.";

        private const string MemoryCategoryKey = "memoryCategory";
        private const string MemoryCategoryDescription = "Specifies what kind of information the memory contains.";

        private const string MemoryValueKey = "memoryMessage";
        private const string MemoryValueDescription = "The actual content of the memory.";

        public MlMemoryGenerationClient(string url, GenerationConfig config, GrammarBuilder grammarBuilder) : base(url,
            config)
        {
            _grammarBuilder = grammarBuilder;
            InitializeGrammar();
        }

        private GrammarEnum GetEnum<T>(string name, bool isRequired) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            var resultValues = new List<object>();

            for (var i = 0; i < values.Length; i++)
            {
                var value = values.GetValue(i).ToString();
                if(value.ToLower() == "none")
                    continue;
                
                resultValues.Add(value);
            }

            return new GrammarEnum(name, isRequired, resultValues.ToArray());
        }

        private string GetEnumResults<T>() where T : Enum
        {
            var result = string.Empty;
            var values = Enum.GetValues(typeof(T));
            var isFirst = true;

            for (var i = 0; i < values.Length; i++)
            {
                var value = values.GetValue(i).ToString();
                if(value.ToLower() == "none")
                    continue;
                
                if (!isFirst)
                {
                    result += ", ";
                }

                result += values.GetValue(i).ToString();
                isFirst = false;
            }

            return result;
        }

        private void InitializeGrammar()
        {
            var memoryType = GetEnum<MemoryType>(MemoryTypeKey, true);
            var memoryImportance = GetEnum<MemoryImportance>(MemoryImportanceKey, true);
            var memoryCategory = GetEnum<MemoryCategory>(MemoryCategoryKey, true);
            var memoryValue = new GrammarString(MemoryValueKey, true);

            _grammarBuilder.AddType(memoryType);
            _grammarBuilder.AddType(memoryImportance);
            _grammarBuilder.AddType(memoryCategory);
            _grammarBuilder.AddType(memoryValue);
        }

        private string AppendProperty<T>(string key, string description, string source) where T : Enum
        {
            source += $"Description: {description}\n {key}: |{GetEnumResults<T>()}|";
            return source;
        }

        private string AppendMetaData(string source)
        {
            source = AppendProperty<MemoryType>(MemoryTypeKey, MemoryTypeDescription, source);
            source = AppendProperty<MemoryImportance>(MemoryImportanceKey, MemoryImportanceDescription, source);
            source = AppendProperty<MemoryCategory>(MemoryCategoryKey, MemoryCategoryDescription, source);
            source += $"{MemoryValueKey} is: {MemoryValueDescription}";

            return source;
        }

        public async Task<GenerationResult<MemoryBlock>> SendMemoryBlockGenerationRequest(string generationMessage,
            string userMessage, string aiMessage = null)
        {
            var history = new History();

            generationMessage = AppendMetaData(generationMessage);
            history.AddSystemMessage(generationMessage);

            if (!string.IsNullOrEmpty(aiMessage))
            {
                history.SetModelMessage(aiMessage);
            }

            history.AddPromt(userMessage);

            GenerationDataCache[MessagesKey] = history.Messages;
            GenerationDataCache["grammar_string"] = _grammarBuilder.Build();

            var generationResult = await SendApiRequest();

            if (!generationResult.IsGenerationSucceed)
            {
                return new GenerationResult<MemoryBlock>(false, null, generationResult.ErrorMessage);
            }

            try
            {
                var obj = JsonConvert.DeserializeObject<JObject>(generationResult.Result);

                if (!obj.TryGetValue(MemoryTypeKey, out var memoryTypeRaw) ||
                    !obj.TryGetValue(MemoryImportanceKey, out var memoryImportanceRaw) ||
                    !obj.TryGetValue(MemoryCategoryKey, out var memoryCategoryRaw) ||
                    !obj.TryGetValue(MemoryValueKey, out var memoryValueRaw))
                {
                    return new GenerationResult<MemoryBlock>(true, MemoryBlock.Empty, generationResult.ErrorMessage);
                }

                var memoryType = memoryTypeRaw.ToObject<MemoryType>();
                var memoryImportance = memoryImportanceRaw.ToObject<MemoryImportance>();
                var memoryCategory = memoryCategoryRaw.ToObject<MemoryCategory>();
                var value = memoryValueRaw.ToString();

                var result = new MemoryBlock()
                {
                    Id = Guid.NewGuid().ToString(),
                    Category = memoryCategory,
                    Importance = memoryImportance,
                    Type = memoryType,
                    Value = value,
                    CreatedAt = DateTime.Now
                };

                return new GenerationResult<MemoryBlock>(true, result, generationResult.ErrorMessage);
            }
            catch
            {
                return new GenerationResult<MemoryBlock>(false, null, "Unable to process memory");
            }
        }
    }
}