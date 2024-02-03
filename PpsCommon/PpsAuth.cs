using System.Net.Http.Headers;
using System.Net.Http.Json;
using PpsCommon.Dtos;

namespace PpsCommon;

public static class PpsAuth
{
    /// <summary>
    /// Call to retrieve a valid Token for Authorization.
    /// </summary>
    /// <param name="httpClient">HttpClient used for connection.</param>
    /// <param name="username">Username used to fetch credentials.</param>
    /// <param name="password">Password used to fetch credentials.</param>
    /// <param name="baseUrl">BaseUrl to the pps-server. Without /OAuth2/Token.</param>
    /// <returns></returns>
    public static async Task<PpsToken> AuthorizeAsync(
        HttpClient httpClient, string username, string password)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", username },
            { "password", password },
        });
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); 

        var tokenUrl = new Uri(httpClient.BaseAddress!, $"/OAuth2/Token" );
        Console.WriteLine($"Connecting with usr:{username} to url:{tokenUrl}");

        var authResponse = await httpClient.PostAsync(tokenUrl, content);
        authResponse.EnsureSuccessStatusCode();
        var authorizationDto = (await authResponse.Content.ReadFromJsonAsync<PpsToken>())!;
        Console.WriteLine("received Token of Type: " + authorizationDto.TokenType);
        return authorizationDto;
    }
}