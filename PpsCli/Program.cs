using System.ComponentModel.DataAnnotations;using System.Text.Json;
using PpsCommon;
using PpsCommon.PpsClientExtensions;
using Cocona;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PpsCommon.Models.PpsModels;


var domain = "localhost";
var port = ":10001";


PpsClientConfiguration ppsConfig;
try
{
    ppsConfig = PpsClientConfiguration.ParseEnvironmentVariables();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}. [PPS_BASEURL PPS_USERNAME PPS_PASSWORD] must be set.");
    Environment.Exit(1);
}

var baseUrl = Environment.GetEnvironmentVariable("PPS_BASEURL") ?? throw new Exception("need baseUrl");
// $"https://{domain}{port}";
var username = Environment.GetEnvironmentVariable("PPS_USERNAME");
// user
var password = Environment.GetEnvironmentVariable("PPS_PASSWORD");
// password123

// UNSAFE, settings for testing only. (SSL-Disabled!)
var noSslClient = new HttpClient(new HttpClientHandler()
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});
var client = await PpsClient.NewAsync(baseUrl, username, password, noSslClient);

var builder = CoconaApp.CreateBuilder();

#if DEBUG
    builder.Logging.AddDebug();
#else
    builder.Logging.AddSystemdConsole();
#endif

var app = builder.Build();

app.AddCommand("legacy", async (
    CoconaAppContext ctx,
    [Argument(Description = "Path to a .json file or folder with the files.")] string filepath,
    [Option('f', Description = "create simply legacy files instead.")] bool legacy
    
) =>
{
    Console.WriteLine($"running on file: '{filepath}' legacymode:{legacy}");

    var logger = app.Services.GetService<ILogger<Program>>()!;
    logger.LogInformation("logging");
    while (!ctx.CancellationToken.IsCancellationRequested)
    {
        await Task.Delay(100, cancellationToken: ctx.CancellationToken);
    }
});

app.AddCommand("server-info" , async (OutputParams outputParams) 
    => HandleJsonRequest(outputParams, await client.AboutServer()))
    .WithDescription("Returns info about the pps-server's configuration");

app.AddCommand("root", async (OutputParams outputParams) 
    => HandleJsonRequest(outputParams, await client.GetRoot()))
    .WithDescription("Gets the whole tree the user can see. Passwords are not included in this view");

app.AddCommand("pw-strength", async (OutputParams outputParams, [Argument] string testPassword)
    => HandleJsonRequest(outputParams, await client.PostPasswordStrength(testPassword)))
    .WithDescription("For input password, returns a numerical value about it's strength");

app.Run();

static void HandleJsonRequest<T>(OutputParams outputParams, T obj, bool writeIndented = true) 
    => HandleCommonParameters(outputParams, JsonSerializer.Serialize(
        obj, new JsonSerializerOptions { WriteIndented = writeIndented }));

static void HandleCommonParameters(OutputParams outputParams, string output)
{
    if (outputParams.output is null)
    {
        Console.WriteLine(output);
    }
    else
    {
        WriteToFile(outputParams, output);
        Console.WriteLine($"successfully wrote file: {Path.GetFullPath(outputParams.output)}");
    }
}

static void WriteToFile(OutputParams outputParams, string text)
{
        var directoryPath = Path.GetDirectoryName(outputParams.output);
        if(!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fullPath = Path.GetFullPath(outputParams.output!);
        if (outputParams.append && File.Exists(fullPath))
        {
            text = $"{Environment.NewLine}{text}";
            File.AppendAllText(fullPath, text);
        }
        else
        { 
            File.WriteAllText(fullPath, text);
        }
}

public record OutputParams(
    [Option('o', Description = "Write the output into a file instead of to std-out.")] string? output,
    [Option(Description = "When writing to a file append instead of overwrite.")] bool append
    ) : ICommandParameterSet;

public class FolderExists : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) 
        => value is string path && Directory.Exists(path) 
            ? ValidationResult.Success! 
            : new ValidationResult($"The path '{value}' is not found.");
}

// Console.WriteLine("starting app");
//

// // create the ppsClient
// var ppsClient = await PpsClient.NewAsync(baseUrl, username, password, noSslClient);
// var jsonOptions = new JsonSerializerOptions() { WriteIndented = true };
//
// var root = await ppsClient.GetEntireTree();
// Console.WriteLine(JsonSerializer.Serialize(root, jsonOptions));
//
// var about = await ppsClient.AboutServer();
// Console.WriteLine(JsonSerializer.Serialize(about, jsonOptions));
//
// var pwStrength = await ppsClient.PostPasswordStrength("password123");
// Console.WriteLine(JsonSerializer.Serialize(pwStrength, jsonOptions));
//