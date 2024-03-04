using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PpsCommon.Auth;

namespace PpsCommon;

public class PpsClient
{
    private readonly HttpClient _httpClient;
    private readonly PpsTokenStore _tokenStore;
    private readonly JsonSerializerOptions _jsonOpts;

    private PpsClient(PpsClientConfiguration config, IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonOptions)
    {
        var httpClient = httpClientFactory.CreateClient(nameof(PpsClient));
        httpClient.BaseAddress = new Uri(config.Url);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        var tokenStore = new PpsTokenStore(config.Username, config.Password, httpClient, jsonOptions);

        _httpClient = httpClient;
        _tokenStore = tokenStore;
        _jsonOpts = jsonOptions;
    }

    public PpsClient(PpsClientConfiguration config, IHttpClientFactory httpClientFactory)
        : this(config, httpClientFactory, new JsonSerializerOptions(JsonSerializerDefaults.Web))
    {
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
        Console.WriteLine("response = \n" + (await response.Content.ReadAsStringAsync(workToken)).Substring(0, 3) +
                          "xxx");
#endif
        var stringResponse = (await response.Content.ReadAsStringAsync(workToken));

        // Trim leading and trailing '"'
        if (stringResponse.StartsWith('"') && stringResponse.EndsWith('"'))
            stringResponse = stringResponse.Substring(1, stringResponse.Length - 2);
        return stringResponse;
    }
}