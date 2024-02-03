using System.Text.Json;
using PpsCommon;
using PpsCommon.PpsClientExtensions;

// just testing how to connect to the pps-api

Console.WriteLine("starting app");

var domain = "localhost";
var port = ":10001";
var baseUrl = $"https://{domain}{port}";

var username = "user";
var password = "password123";


// UNSAFE, settings for testing only. (SSL-Disabled!)
var noSslClient = new HttpClient(new HttpClientHandler()
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});

// create the ppsClient
var ppsClient = await PpsClient.NewAsync(baseUrl, username, password, noSslClient);
var jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

var root = await ppsClient.GetEntireTree();
Console.WriteLine(JsonSerializer.Serialize(root, jsonOptions));

// var about = await ppsClient.AboutServer();
// Console.WriteLine(JsonSerializer.Serialize(about, jsonOptions));
//
// var pwStrength = await ppsClient.PostPasswordStrength("password123");
// Console.WriteLine(JsonSerializer.Serialize(pwStrength, jsonOptions));
//
