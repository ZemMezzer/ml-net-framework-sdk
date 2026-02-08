namespace MLAgentSDK.RAG.Data
{
    [Flags]
    public enum MemoryCategory
    {
        None = 0,
        Fact = 1 << 0,
        Preference = 1 << 1,
        Plan = 1 << 2,
        Note = 1 << 3,
    }

    public static class MemoryCategoryExtensions
    {
        public static bool HasFlagFast(this MemoryCategory value, MemoryCategory flag)
        {
            return (value & flag) != 0;
        }
    }
}