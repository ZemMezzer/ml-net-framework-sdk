namespace MLSDK.Data.Grammar.Types;

public class GrammarType
{
    protected const string AssignmentOperator = "::=";
    
    public readonly string Name;
    public string Declaration { get; protected set; }

    public GrammarType(string name, string value)
    {
        Name = name;
        Declaration = $"{value}";
    }

    /// <summary>
    /// Generates GBNF style grammar
    /// </summary>
    /// <returns></returns>
    internal virtual string GenerateType()
    {
        string declaration = $"{Name}Dec {AssignmentOperator} {Declaration}";
        string type = $"{Name} {AssignmentOperator}  \"\\\"\" {Name}Dec \"\\\"\"";
        return $"{type}\r\n{declaration}\r\n";
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}