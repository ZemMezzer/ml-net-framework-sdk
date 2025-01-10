namespace MlSDK.Data
{
    public struct GenerationResult
    {
        public readonly bool IsGenerationSucceed;
        public readonly string ResultMessage;
        public readonly string ErrorMessage;

        public GenerationResult(bool isGenerationSucceed, string resultMessage, string errorMessage)
        {
            IsGenerationSucceed = isGenerationSucceed;
            ResultMessage = resultMessage;
            ErrorMessage = errorMessage;
        }
    }
}