using System.Text.Json.Serialization;

namespace PpsCommon.Auth;

public class PpsToken(string accessToken,
    string tokenType,
    int expiresIn)
{
    [JsonPropertyName("access_token")] 
    public string AccessToken { get; init; } = accessToken ?? throw new ArgumentNullException();

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = tokenType ?? throw new ArgumentNullException();

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; } = expiresIn;
    
    /// <summary>
    /// Converted DateTime from int(in seconds) we received.
    /// </summary>
    private DateTime ExpiresInDate { get; } = DateTime.UtcNow.AddSeconds(expiresIn);
    
    /// <summary>
    /// Calculates if token is expired. Expires Tokens 5 Minute early to make sure nothing overlaps. 
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresInDate.AddMinutes(-5);

}