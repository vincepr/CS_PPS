using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PpsCommon.Auth;
using PpsCommon.Dtos;
using PpsCommon.Models.PpsModels;

namespace PpsCommon;

public class PpsClient
{
    private readonly HttpClient _httpClient;
    private readonly PpsTokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOpts;

    public static async Task<PpsClient> NewAsync(string baseUrl, string username, string password, HttpClient httpClient, JsonSerializerOptions jsonOptions)
    {
        httpClient.BaseAddress = new Uri(baseUrl);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        
        var tokenStore = new PpsTokenStore(username, password, httpClient, jsonOptions);
        await tokenStore.Fetch();
        
        var ppsClient = new PpsClient(httpClient, tokenStore, jsonOptions);
        return ppsClient;
    }

    public static async Task<PpsClient> NewAsync(string baseUrl, string username, string password)
        => await PpsClient.NewAsync(baseUrl, username, password, new HttpClient());

    public static async Task<PpsClient> NewAsync(string baseUrl, string username, string password, HttpClient httpClient)
        => await PpsClient.NewAsync(baseUrl, username, password, httpClient, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private PpsClient(HttpClient httpClient, PpsTokenStore tokenStore, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
        _jsonOpts = jsonOptions;
    }
   
    public Task<CredentialGroup> GetEntireTree() => GenericGet<CredentialGroup>("/api/v5/rest/folders/");
    public Task<AboutServer> GetAboutServer() => GenericGet<AboutServer>("/api/v5/rest/about/");
    public Task<PasswordStrength> PostPasswordStrength(string testPassword) 
        => GenericPost<PasswordStrengthRequestDto, PasswordStrength>(new PasswordStrengthRequestDto(testPassword),
            "/api/v5/rest/passwordstrength");

    public async Task<TResponse> GenericGet<TResponse>(string relativeUri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenStore.Fetch());
        
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
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenStore.Fetch());
        
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var responseContentStream = await response.Content.ReadAsStreamAsync();
        
        TResponse? model = await JsonSerializer.DeserializeAsync<TResponse>(responseContentStream, _jsonOpts);
        return model ?? throw new JsonException($"Failed to deserialize Json, for {typeof(TResponse)}");
    }    
}