using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Channels;
using PpsCli.Dtos;
using PpsCommon;

// just testing how to connect to the pps-api

Console.WriteLine("starting app");

var domain = "localhost";
var port = ":10001";
var baseUrl = $"https://{domain}{port}";

var username = "user";
var password = "password123";


// UNSAFE, disabling ssl-checking for testing:
var noSslHandler = new HttpClientHandler();
noSslHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
noSslHandler.ServerCertificateCustomValidationCallback = (_msg, _cert, _chain, _policy) => true;

var client = new HttpClient(noSslHandler);

// get token from auth-endpoint
var authResponse = await new AuthorizationClient(client).AuthorizeAsync(username, password, baseUrl);
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
    Console.WriteLine(await response.Content.ReadAsStringAsync());
}