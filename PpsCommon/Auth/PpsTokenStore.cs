using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;

namespace PpsCommon.Auth;

public class PpsTokenStore
{
    private const string AUTH_ENDPOINT = "/OAuth2/Token";
    private const int MAX_RETRIES = 3;
    
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

            for (var retries = 0; retries < MAX_RETRIES + 1; retries++)
            {
                try
                {
                    var token = await RequestToken();
                    _token = token ?? throw new JsonException("Failed to deserialize AccessToken to JSON.");
                    return _token.AccessToken;
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"Failed to retrieve {nameof(PpsToken)} for Pps-Authorization to {_httpClient.BaseAddress} with: {ex.Message}");
                }
            }
            
            throw new AuthenticationException($"Failed to retrieve {nameof(PpsToken)} for Pps-Authorization after {MAX_RETRIES} retries.");
        }
        finally
        {
            AccessSemaphore.Release(1);
        }
    }

    private async Task<PpsToken?> RequestToken()
    {
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
        return await JsonSerializer.DeserializeAsync<PpsToken>(responseContentStream, _jsonOptions);
    }
}