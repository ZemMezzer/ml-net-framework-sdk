namespace MLAgentSDK.Data
{
    public struct GenerationResult<T>
    {
        public readonly bool IsGenerationSucceed;
        public readonly T Result;
        public readonly string ErrorMessage;

        public GenerationResult(bool isGenerationSucceed, T result, string errorMessage)
        {
            IsGenerationSucceed = isGenerationSucceed;
            Result = result;
            ErrorMessage = errorMessage;
        }
    }
}