using System;

namespace RAG.Data
{
    [Serializable]
    public class MemoryBlock
    {
        public MemoryImportance Importance;
        public MemoryType Type;
        public MemoryCategory Category;
        
        public string Id;
        public string Value;
        public DateTime CreatedAt;

        public static MemoryBlock Empty => new MemoryBlock() { Importance = MemoryImportance.NotImportant };
    }
}
