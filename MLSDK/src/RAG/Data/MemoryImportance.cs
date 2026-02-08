namespace MLAgentSDK.RAG.Data
{
    [Flags]
    public enum MemoryImportance
    {
        None = 0,
        NotImportant = 1 << 0,
        LowImportant = 1 << 1,
        MediumImportant = 1 << 2,
        Important = 1 << 3,
        VeryImportant = 1 << 4
    }

    public static class MemoryImportanceExtensions
    {
        public static bool HasFlagFast(this MemoryImportance value, MemoryImportance flag)
        {
            return (value & flag) != 0;
        }
    }
}