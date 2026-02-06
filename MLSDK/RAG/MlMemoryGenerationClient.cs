using System;
using System.Threading.Tasks;
using MlSDK;
using MlSDK.Data;
using MLSDK.Data;
using MLSDK.Data.Grammar;
using MLSDK.Data.Grammar.Containers;
using MLSDK.Data.Grammar.Objects;
using MLSDK.Data.Grammar.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAG.Data;

namespace RAG
{
    public class MlMemoryGenerationClient : MlTextGenerationClientBase
    {
        private readonly IGrammarBuilder _grammarBuilder;

        private const string MemoryTypeKey = "memoryType";

        private const string MemoryTypeDescription =
            "Specifies how long this memory should be kept and how it is intended to be used.";

        private const string MemoryImportanceKey = "memoryImportance";
        private const string MemoryImportanceDescription = "Specifies how critical this memory is.";

        private const string MemoryCategoryKey = "memoryCategory";
        private const string MemoryCategoryDescription = "Specifies what kind of information the memory contains.";

        private const string MemoryValueKey = "memoryMessage";
        private const string MemoryValueDescription = "The actual content of the memory.";

        private const string MemoryKey = "memory";

        public MlMemoryGenerationClient(string url, GenerationConfig config, IGrammarBuilder grammarBuilder) : base(url,
            config)
        {
            _grammarBuilder = grammarBuilder;
            InitializeGrammar();
        }

        private GrammarEnum GetEnum<T>(string name) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            var resultValues = new string[values.Length];

            for (var i = 0; i < values.Length; i++)
            {
                resultValues[i] = values.GetValue(i).ToString();
            }

            return new GrammarEnum(name, resultValues);
        }

        private string GetEnumResults<T>() where T : Enum
        {
            var result = string.Empty;
            var values = Enum.GetValues(typeof(T));
            var isFirst = true;

            for (var i = 0; i < values.Length; i++)
            {
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
            var memoryTypeEnum = GetEnum<MemoryType>(MemoryTypeKey);
            var memoryImportanceEnum = GetEnum<MemoryImportance>(MemoryImportanceKey);
            var memoryCategoryEnum = GetEnum<MemoryCategory>(MemoryCategoryKey);

            var memoryObject = new GrammarObject(MemoryKey);

            var memoryType = new GrammarValue(MemoryTypeKey, GrammarRequirement.Required, memoryTypeEnum);
            var memoryImportance =
                new GrammarValue(MemoryImportanceKey, GrammarRequirement.Required, memoryImportanceEnum);
            var memoryCategory = new GrammarValue(MemoryCategoryKey, GrammarRequirement.Required, memoryCategoryEnum);
            var memoryValue = new GrammarValue(MemoryValueKey, GrammarRequirement.Required, new GrammarString());

            memoryObject.Add(memoryType);
            memoryObject.Add(memoryImportance);
            memoryObject.Add(memoryCategory);
            memoryObject.Add(memoryValue);

            _grammarBuilder.AddValue(memoryObject);
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

                if (!obj.TryGetValue(MemoryKey, out var memory))
                {
                    return new GenerationResult<MemoryBlock>(true, MemoryBlock.Empty, generationResult.ErrorMessage);
                }

                var memoryType = memory[MemoryTypeKey].ToObject<MemoryType>();
                var memoryImportance = memory[MemoryImportanceKey].ToObject<MemoryImportance>();
                var memoryCategory = memory[MemoryCategoryKey].ToObject<MemoryCategory>();

                var value = memory[MemoryValueKey].ToString();

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