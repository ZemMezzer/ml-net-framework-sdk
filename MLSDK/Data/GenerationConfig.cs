namespace MLSDK.Data
{
    public readonly struct GenerationConfig
    {
        public struct Parameter
        {
            public readonly string Key;
            public readonly object Value;

            public Parameter(string key, object value)
            {
                Key = key;
                Value = value;
            }
        }
    
        private readonly Parameter[] _generationParameters;

        public ReadOnlySpan<Parameter> GenerationParameters => _generationParameters.AsSpan();

        public GenerationConfig(Span<Parameter> parameters)
        { 
            _generationParameters = new Parameter[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                _generationParameters[i] = parameters[i];
            }
        }
    }
}