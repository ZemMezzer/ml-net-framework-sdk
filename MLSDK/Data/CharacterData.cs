namespace MLSDK.Data;

[Serializable]
public struct CharacterData
{
    public string Name;

    public CharacterData(string name)
    {
        Name = name;
    }
}