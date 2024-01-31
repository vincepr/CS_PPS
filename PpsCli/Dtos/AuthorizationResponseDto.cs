using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PpsCli.Dtos;

public class AuthorizationResponseDto(string accessToken,
    string tokenType,
    int expiresIn)
{
    [JsonPropertyName("access_token")] 
    public string AccessToken { get; init; } = accessToken ?? throw new ArgumentNullException();

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = tokenType ?? throw new ArgumentNullException();

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; } = expiresIn;
}