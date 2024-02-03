using System.Text.Json;
using PpsCommon;

// just testing how to connect to the pps-api

Console.WriteLine("starting app");

var domain = "localhost";
var port = ":10001";
var baseUrl = $"https://{domain}{port}";

var username = "user";
var password = "password123";


// UNSAFE, settings for testing only. (SSL-Disabled!)
var noSslHandler = new HttpClientHandler()
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};
var client = new HttpClient(noSslHandler) { BaseAddress = new Uri(baseUrl) };
var jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

// create the ppsClient
var ppsClient = await PpsClient.NewAsync(baseUrl, username, password, client);
var resp = await ppsClient.GetEntireTree();
Console.WriteLine(JsonSerializer.Serialize(resp, jsonOptions));

var about = await ppsClient.GetAboutServer();
Console.WriteLine(JsonSerializer.Serialize(about, jsonOptions));

var pwStrength = await ppsClient.PostPasswordStrength("123");
Console.WriteLine(JsonSerializer.Serialize(pwStrength, jsonOptions));

