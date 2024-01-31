using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Channels;
using PpsCli.Dtos;

// just testing how to connect to the pps-api

Console.WriteLine("starting app");

var domain = "localhost";
var port = "10001";

var username = "user";
var password = "password123";


// UNSAFE, disabling ssl-checking for testing:
var noSslHandler = new HttpClientHandler();
noSslHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
noSslHandler.ServerCertificateCustomValidationCallback = (_msg, _cert, _chain, _policy) => true;

var client = new HttpClient(noSslHandler);

// get token from auth-endpoint
var content = new FormUrlEncodedContent(new Dictionary<string, string>
{
    { "grant_type", "password" },
    { "username", username },
    { "password", password },
});
content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); 

var tokenUrl = $"https://{domain}:{port}/OAuth2/Token";
Console.WriteLine($"Connecting with usr:{username} to url:{tokenUrl}");

var authResponse = await client.PostAsync(tokenUrl, content);
authResponse.EnsureSuccessStatusCode();
var authorizationDto = await authResponse.Content.ReadFromJsonAsync<AuthorizationResponseDto>();
Console.WriteLine(authorizationDto);

// get secret via uuid
var token = authorizationDto.AccessToken;
var uuid = "https://mini:10001/WebClient/Main?itemId=7c1736ec-7d29-435a-8bd1-16765987377e";
var secretUrl = $"https://{domain}:{port}/api/v6/rest/credential/{uuid}";
Console.WriteLine($"Connecting with usr:{username} to url:{tokenUrl}");

WebHeaderCollection headers = new WebHeaderCollection();
headers.Add("Accept", "application/json");
headers.Add("Authorization", $"Bearer {token}");
headers.Add("Cache-Control", "no-cache");

using (var request = new HttpRequestMessage(HttpMethod.Get, secretUrl))
{
    request.Headers.Add("Accept", "application/json"); // might skipp that
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    request.Headers.Add("Cache-Control", "no-cache");
    var response = await client.SendAsync(request);
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}