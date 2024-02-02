using System.Net.Http.Headers;
using System.Text.Json;
using PpsCommon;
using PpsCommon.Models.PpsModels;

// just testing how to connect to the pps-api

Console.WriteLine("starting app");

var domain = "localhost";
var port = ":10001";
var baseUrl = $"https://{domain}{port}";

var username = "user";
var password = "password123";


// UNSAFE, disabling ssl-checking for testing:
var noSslHandler = new HttpClientHandler()
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};

var client = new HttpClient(noSslHandler) { BaseAddress = new Uri(baseUrl) };

// get token from auth-endpoint
var authResponse = await PpsAuth.AuthorizeAsync(client, username, password);
var token = authResponse.AccessToken;

// get secret via uuid
var uuid = "7c1736ec-7d29-435a-8bd1-16765987377e";
var secretUrl = $"{baseUrl}/api/v6/rest/credential/{uuid}";
Console.WriteLine($"Connecting with usr:{username} to url:{baseUrl}");


using (var request = new HttpRequestMessage(HttpMethod.Get, secretUrl))
{
    request.Headers.Add("Accept", "application/json"); // might skipp that
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    request.Headers.Add("Cache-Control", "no-cache");
    var response = await client.SendAsync(request);
    // Console.WriteLine(await response.Content.ReadAsStringAsync());
}

var seriOpts = new JsonSerializerOptions()
{
    WriteIndented = true
};
var ppsClient = new PpsClient(client, authResponse, new JsonSerializerOptions(JsonSerializerDefaults.Web));
var resp = await ppsClient.GetEntireTree();
Console.WriteLine(JsonSerializer.Serialize(resp, seriOpts));

var about = await ppsClient.GetAboutServer();
Console.WriteLine(JsonSerializer.Serialize(about, seriOpts));

var pwStrength = await ppsClient.PostPasswordStrength("123");
Console.WriteLine(JsonSerializer.Serialize(pwStrength, seriOpts));

