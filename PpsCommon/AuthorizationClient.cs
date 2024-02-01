using System.Net.Http.Headers;
using System.Net.Http.Json;
using PpsCommon.Dtos;

namespace PpsCommon;

public class AuthorizationClient
{
    private readonly HttpClient _httpClient;
    
    public AuthorizationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthorizationResponseDto> AuthorizeAsync(string username, string password, string url)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", username },
            { "password", password },
        });
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); 

        var tokenUrl = $"{url}/OAuth2/Token";
        Console.WriteLine($"Connecting with usr:{username} to url:{tokenUrl}");

        var authResponse = await _httpClient.PostAsync(tokenUrl, content);
        authResponse.EnsureSuccessStatusCode();
        var authorizationDto = (await authResponse.Content.ReadFromJsonAsync<AuthorizationResponseDto>())!;
        Console.WriteLine(authorizationDto);
        return authorizationDto;
    }
}