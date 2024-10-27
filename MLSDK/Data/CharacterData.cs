namespace MLSDK.Data;

[Serializable]
public struct CharacterData
{
    public string Name;
    public string Context;

    public CharacterData(string name, string context)
    {
        Name = name;
        Context = context;
    }
}