namespace MlSDK.Data;

public readonly struct AuthData
{
    public readonly string Username;
    public readonly string Password;

    public AuthData(string username, string password)
    {
        Username = username;
        Password = password;
    }
}