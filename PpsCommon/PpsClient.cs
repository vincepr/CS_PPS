using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PpsCommon.Dtos;
using PpsCommon.Models.PpsModels;

namespace PpsCommon;

public class PpsClient
{
    private readonly HttpClient _httpClient;
    private readonly string _token; // TODO: build actual token-store: https://josef.codes/dealing-with-access-tokens-in-dotnet/
    private readonly JsonSerializerOptions _jsonOpts;

    public PpsClient(HttpClient httpClient, PpsToken credentials, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient;
        _token = credentials.AccessToken;
        _jsonOpts = jsonOptions;
        
        // setup http client
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }
   
    public Task<CredentialGroup> GetEntireTree() => GenericGet<CredentialGroup>("/api/v5/rest/folders/");
    public Task<AboutServer> GetAboutServer() => GenericGet<AboutServer>("/api/v5/rest/about/");
    public Task<PasswordStrength> PostPasswordStrength(string testPassword) 
        => GenericPost<PasswordStrengthRequestDto, PasswordStrength>(new PasswordStrengthRequestDto(testPassword),
            "/api/v5/rest/passwordstrength");


    public async Task<TResponse> GenericGet<TResponse>(string relativeUri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var responseContentStream = await response.Content.ReadAsStreamAsync();
        
        TResponse? model = await JsonSerializer.DeserializeAsync<TResponse>(responseContentStream, _jsonOpts);
        return model ?? throw new JsonException($"Failed to deserialize Json, for {typeof(TResponse)}");
    }

    public async Task<TResponse> GenericPost<TBody, TResponse>(TBody body, string relativeUri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, relativeUri);
        request.Content = new StringContent(JsonSerializer.Serialize(body, _jsonOpts), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var responseContentStream = await response.Content.ReadAsStreamAsync();
        
        TResponse? model = await JsonSerializer.DeserializeAsync<TResponse>(responseContentStream, _jsonOpts);
        return model ?? throw new JsonException($"Failed to deserialize Json, for {typeof(TResponse)}");
    }    

}