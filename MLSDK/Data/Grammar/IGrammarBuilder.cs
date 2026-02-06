using MLSDK.Data.Grammar.Containers;

namespace MLSDK.Data.Grammar
{
    public interface IGrammarBuilder
    {
        public string Build();
        public void AddValue(GrammarValue value);
    }
}