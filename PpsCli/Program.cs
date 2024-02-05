using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using PpsCommon;
using PpsCommon.PpsClientExtensions;
using Cocona;
using Microsoft.Extensions.Logging;
using PpsCli;

PpsClientConfiguration? config = null;
try
{
    config = PpsClientConfiguration.ParseEnvironmentVariables();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}. [PPS_URL PPS_USERNAME PPS_PASSWORD] must be set.");
    Environment.Exit(1);
}

// UNSAFE, settings for testing only. (SSL-Disabled!)
var noSslClient = new HttpClient(new HttpClientHandler()
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});
var client = await PpsClient.NewAsync(config.Url, config.Username, config.Password, noSslClient);

var builder = CoconaApp.CreateBuilder();

#if DEBUG
    builder.Logging.AddDebug();
#else
    builder.Logging.AddSystemdConsole();
#endif

var app = builder.Build();

app.AddCommand("legacy", async (
    CoconaAppContext ctx,
    [Argument(Description = "Path to a .json file to parse")] [FileExists] string filepath,
    OutputParams outputParams
) =>
{
    var content = await SecretsFile.BuildJsonString(client, filepath, ctx.CancellationToken);
    if (outputParams.Output is not null)
    {
        WriteToFile(outputParams, content);
        Console.WriteLine($"successfully generated from file: '{filepath}' to: {outputParams.Output}");
    }
    else
    {
        Console.WriteLine(content);
    }
});

app.AddCommand("server-info", async (OutputParams outputParams)
        => HandleJsonRequest(outputParams, await client.AboutServer()))
    .WithDescription("Returns info about the pps-server's configuration");

app.AddCommand("folder-root", async (OutputParams outputParams)
        => HandleJsonRequest(outputParams, await client.GetFolderRoot()))
    .WithDescription("Gets the whole tree the user can see. Passwords are not included in this view");

app.AddCommand("folder", async (OutputParams outputParams, [Argument] string groupId)
        => HandleJsonRequest(outputParams, await client.GetFolderRoot()))
    .WithDescription("Gets everything inside provided folder");

app.AddCommand("pw-strength", async (OutputParams outputParams, [Argument] string testPassword)
        => HandleJsonRequest(outputParams, await client.PostPasswordStrength(testPassword)))
    .WithDescription("For input password, returns a numerical value about it's strength");

app.AddCommand("offline-package-available", async (OutputParams outputParams)
        => HandleStringRequest(outputParams, (await client.IsOfflineAvailable()).ToString()))
    .WithDescription("Returns bool if user has ability to get offline access to credentials");

app.AddCommand("offline-package", async (OutputParams outputParams)
        => HandleJsonRequest(outputParams, await client.GetFolderRoot()))
    .WithDescription("Gets the whole offline package. Attached-Files included");

app.AddCommand("search", async (OutputParams outputParams, [Argument] string searchTerm)
        => HandleJsonRequest(outputParams, await client.Search(searchTerm)))
    .WithDescription("Returns Entries or Folders that match the search term");

app.AddCommand("credential", async (OutputParams outputParams, [Argument] Guid id)
        => HandleJsonRequest(outputParams, await client.GetCredential(id)))
    .WithDescription("Returns the Credential for provided guid-id");

app.AddCommand("entry", async (OutputParams outputParams, [Argument] Guid id)
        => HandleJsonRequest(outputParams, await client.GetEntry(id)))
    .WithDescription("Returns the Entry for provided guid-id");

app.AddCommand("password", async (OutputParams outputParams, [Argument] Guid id)
        => HandleStringRequest(outputParams, await client.GetPassword(id)))
    .WithDescription("Returns the Password for of the Entry of provided guid-id");

app.AddCommand("client-configuration", async (OutputParams outputParams, [Argument] string clientName)
        => HandleJsonRequest(outputParams, await client.ClientConfiguration(clientName)))
    .WithDescription("Returns the server-enforced client configuration");

app.Run();









static void HandleJsonRequest<T>(OutputParams outputParams, T obj, bool writeIndented = true)
    => HandleStringRequest(outputParams, JsonSerializer.Serialize(
        obj, new JsonSerializerOptions { WriteIndented = writeIndented }));

static void HandleStringRequest(OutputParams outputParams, string output)
{
    if (outputParams.Output is null)
    {
        Console.WriteLine(output);
    }
    else
    {
        WriteToFile(outputParams, output);
        Console.WriteLine($"successfully wrote file: {Path.GetFullPath(outputParams.Output)}");
    }
}

static void WriteToFile(OutputParams outputParams, string text)
{
    var directoryPath = Path.GetDirectoryName(outputParams.Output);
    if (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
    {
        Directory.CreateDirectory(directoryPath);
    }

    string fullPath = Path.GetFullPath(outputParams.Output!);
    if (outputParams.Append && File.Exists(fullPath))
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
    [Option('o', Description = "Write the output into a file instead of to std-out.")]
    string? Output,
    [Option(Description = "When writing to a file append instead of overwrite.")]
    bool Append
) : ICommandParameterSet;

public class FileExists : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        => value is string path && File.Exists(path) //|| Directory.Exists(path)
            ? ValidationResult.Success
            : new ValidationResult($"The path '{value}' is not found.");
}