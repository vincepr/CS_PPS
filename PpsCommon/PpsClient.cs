using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PpsCommon.Auth;

namespace PpsCommon;

public class PpsClient
{
    private readonly HttpClient _httpClient;
    private readonly PpsTokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOpts;

    public static async Task<PpsClient> NewAsync(string baseUrl, string username, string password,
        HttpClient httpClient, JsonSerializerOptions jsonOptions)
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
        => await NewAsync(baseUrl, username, password, new HttpClient());

    public static async Task<PpsClient> NewAsync(string baseUrl, string username, string password,
        HttpClient httpClient)
        => await NewAsync(baseUrl, username, password, httpClient,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private PpsClient(HttpClient httpClient, PpsTokenStore tokenStore, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
        _jsonOpts = jsonOptions;
    }

    public async Task<TResponse> GenericGet<TResponse>(string relativeUri, CancellationToken workToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenStore.Fetch());

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, workToken);
        response.EnsureSuccessStatusCode();
        
#if DEBUG
        Console.WriteLine("response = \n" + await response.Content.ReadAsStringAsync(workToken));
#endif
        await using var responseContentStream = await response.Content.ReadAsStreamAsync(workToken);

        TResponse? model =
            await JsonSerializer.DeserializeAsync<TResponse>(responseContentStream, _jsonOpts, workToken);
        return model ?? throw new JsonException($"Failed to deserialize Json, for {typeof(TResponse)}");
    }

    public async Task<TResponse> GenericPost<TBody, TResponse>(TBody body, string relativeUri,
        CancellationToken workToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, relativeUri);
        request.Content =
            new StringContent(JsonSerializer.Serialize(body, _jsonOpts), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenStore.Fetch());

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, workToken);
        response.EnsureSuccessStatusCode();
        
#if DEBUG
        Console.WriteLine("response = \n" + await response.Content.ReadAsStringAsync(workToken));
#endif
        await using var responseStream = await response.Content.ReadAsStreamAsync(workToken);

        TResponse? model = await JsonSerializer.DeserializeAsync<TResponse>(responseStream, _jsonOpts, workToken);
        return model ?? throw new JsonException($"Failed to deserialize Json, for {typeof(TResponse)}");
    }

    public async Task<bool> GenericBool(string relativeUri, CancellationToken workToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenStore.Fetch());

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, workToken);
        
#if DEBUG
        Console.WriteLine("response = \n" + await response.Content.ReadAsStringAsync(workToken));
#endif
        if (!response.IsSuccessStatusCode) return false;
        var body = (await response.Content.ReadAsStringAsync(workToken)).Trim().ToLower();
        if (body == "true") return true;
        return false;
    }

    public async Task<string> GenericString(string relativeUri, CancellationToken workToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokenStore.Fetch());
        
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, workToken);
        response.EnsureSuccessStatusCode();
        
#if DEBUG
        Console.WriteLine("response = \n" + await response.Content.ReadAsStringAsync(workToken));
#endif
        return await response.Content.ReadAsStringAsync(workToken);
    }
}