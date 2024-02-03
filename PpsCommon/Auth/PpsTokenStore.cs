using System.Net.Http.Headers;
using System.Text.Json;

namespace PpsCommon.Auth;

public class PpsTokenStore
{
    private const string AUTH_ENDPOINT = "/OAuth2/Token";
    
    private static readonly SemaphoreSlim AccessSemaphore;
    private readonly string _username;
    private readonly string _password;
    private PpsToken? _token;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    static PpsTokenStore()
    {
        AccessSemaphore = new SemaphoreSlim(initialCount:1 ,maxCount: 1);
    }

    public PpsTokenStore(string username, string password, HttpClient httpClient, JsonSerializerOptions jsonOptions)
    {
        _username = username;
        _password = password;
        _httpClient = httpClient;
        _jsonOptions = jsonOptions;
    }
    
    public async Task<string> Fetch()
    {
        try
        {
            await AccessSemaphore.WaitAsync(); // this will block multiple auth-calls if in parallel

            if (_token is { IsExpired: false })
            {
                return _token.AccessToken;
            }
            
            using var request = new HttpRequestMessage(HttpMethod.Post, AUTH_ENDPOINT);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", _username },
                { "password", _password },
            });
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
                
            await using var responseContentStream = await response.Content.ReadAsStreamAsync();
            var token = await JsonSerializer.DeserializeAsync<PpsToken>(responseContentStream, _jsonOptions);
                
            _token = token ?? throw new JsonException("Failed to deserialize AccessToken to JSON.");
            return _token.AccessToken;
        }
        finally
        {
            AccessSemaphore.Release(1);
        }
    }
}