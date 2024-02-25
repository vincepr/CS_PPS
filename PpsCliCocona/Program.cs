using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using PpsCommon;
using PpsCommon.PpsClientExtensions;
using Cocona;
using Cocona.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using PpsCli;


// catch (Exception ex)
// {
//     Console.Error.WriteLine($"Error: {ex.Message}. [PPS_URL PPS_USERNAME PPS_PASSWORD] must be set.");
//     Environment.Exit(1);
// }
// UNSAFE, settings for testing only. (SSL-Disabled!)
var noSslClient = new HttpClient(new HttpClientHandler()
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});
// var client = new PpsClient(new PpsClientConfiguration(config.Url, config.Username, config.Password), noSslClient);

var builder = CoconaApp.CreateBuilder();
builder.Services.AddHttpClient<PpsClient>()
    // .ConfigureHttpClient((sp, httpClient) =>
    // {
    //     // var options = sp.GetRequiredService<IOptions<SomeOptions>>().Value;
    //     // httpClient.BaseAddress = options.Url;
    //     // httpClient.Timeout = options.RequestTimeout;
    // })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigurePrimaryHttpMessageHandler(x => new HttpClientHandler()
    {
        ClientCertificateOptions = ClientCertificateOption.Manual,
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true

    })
    .AddPolicyHandler(GetRetryPolicy())
    ;

#if DEBUG
    // builder.Logging.AddDebug();
    builder.Logging.AddEventSourceLogger();
    // builder.Logging.AddJsonConsole();
    // builder.Logging.AddConsole();
    // builder.Logging.AddSimpleConsole();
#else
    builder.Logging.AddSystemdConsole();
#endif

var app = builder.Build();

app.AddCommand("legacy", async (
    CoconaAppContext ctx,
    [Argument(Description = "Path to a .json file to parse")] [FileExists] string filepath,
    OutputParams outputParams,
    PpsConfigParams configParams,
    IHttpClientFactory clientFactory
) =>
{
    (string? Url, string? Username,string? Password) config = PpsClientConfiguration.ParseEnvironmentVariables();
    // handle required url, username, password
    config.Url = configParams.Url ?? config.Url
       ?? throw new Exception("Missing ENV-Variable PPS_URL, --Url, -l");
    config.Username = configParams.Username ?? config.Username
        ?? throw new Exception("Missing ENV-Variable PPS_USERNAME, --username, -u");
    config.Password = configParams.Password ?? config.Password
        ?? throw new Exception("Missing ENV-Variable PPS_PASSWORD, --password, -p");
    var ppsConfig = new PpsClientConfiguration(config.Url, config.Username, config.Password);
    
    // - TODO: remove debug
    var watch = System.Diagnostics.Stopwatch.StartNew();
    
    var myclient = new PpsClient(ppsConfig, clientFactory.CreateClient(nameof(PpsClient)));
    
    //
    var content = await SecretsFile.ParallelBuildJsonString(myclient, filepath, ctx.CancellationToken);
    // var content = await SecretsFile.BuildJsonString(myclient, filepath, ctx.CancellationToken);
    if (outputParams.Output is not null)
    {
        WriteToFile(outputParams, content);
        Console.WriteLine($"successfully generated from file: '{filepath}' to: {outputParams.Output}");
    }
    else
    {
        Console.WriteLine(content);
    }
    
    // TODO - remove debug
    watch.Stop();
    Console.WriteLine("time in ms: "+ watch.ElapsedMilliseconds);
});

// app.AddCommand("server-info", async (OutputParams outputParams)
//         => HandleJsonRequest(outputParams, await client.AboutServer()))
//     .WithDescription("Returns info about the pps-server's configuration");
//
// app.AddCommand("folder-root", async (OutputParams outputParams)
//         => HandleJsonRequest(outputParams, await client.GetFolderRoot()))
//     .WithDescription("Gets the whole tree the user can see. Passwords are not included in this view");
//
// app.AddCommand("folder", async (OutputParams outputParams, [Argument] string groupId)
//         => HandleJsonRequest(outputParams, await client.GetFolderRoot()))
//     .WithDescription("Gets everything inside provided folder");
//
// app.AddCommand("pw-strength", async (OutputParams outputParams, [Argument] string testPassword)
//         => HandleJsonRequest(outputParams, await client.PostPasswordStrength(testPassword)))
//     .WithDescription("For input password, returns a numerical value about it's strength");
//
// app.AddCommand("offline-package-available", async (OutputParams outputParams)
//         => HandleStringRequest(outputParams, (await client.IsOfflineAvailable()).ToString()))
//     .WithDescription("Returns bool if user has ability to get offline access to credentials");
//
// app.AddCommand("offline-package", async (OutputParams outputParams)
//         => HandleJsonRequest(outputParams, await client.GetFolderRoot()))
//     .WithDescription("Gets the whole offline package. Attached-Files included");
//
// app.AddCommand("search", async (OutputParams outputParams, [Argument] string searchTerm)
//         => HandleJsonRequest(outputParams, await client.Search(searchTerm)))
//     .WithDescription("Returns Entries or Folders that match the search term");
//
// app.AddCommand("credential", async (OutputParams outputParams, [Argument] Guid id)
//         => HandleJsonRequest(outputParams, await client.GetCredential(id)))
//     .WithDescription("Returns the Credential for provided guid-id");
//
// app.AddCommand("entry", async (OutputParams outputParams, [Argument] Guid id)
//         => HandleJsonRequest(outputParams, await client.GetEntry(id)))
//     .WithDescription("Returns the Entry for provided guid-id");
//
// app.AddCommand("password", async (OutputParams outputParams, [Argument] Guid id)
//         => HandleStringRequest(outputParams, await client.GetPassword(id)))
//     .WithDescription("Returns the Password for of the Entry of provided guid-id");
//
// app.AddCommand("client-configuration", async (OutputParams outputParams, [Argument] string clientName)
//         => HandleJsonRequest(outputParams, await client.ClientConfiguration(clientName)))
//     .WithDescription("Returns the server-enforced client configuration");

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

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(retryCount:6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
            retryAttempt)));
}

public record OutputParams(
    [Option('o', Description = "Write the output into a file instead of to std-out.")]
    string? Output,
    [Option(Description = "When writing to a file append instead of overwrite.")]
    bool Append
) : ICommandParameterSet;

public record PpsConfigParams(
    [Option('l', Description = "Alternative to PPS_URL Environment-Variable.")]
    string? Url,
    [Option('u', Description = "Alternative to PPS_USERNAME Environment-Variable.")]
    string? Username,
    [Option('p', Description = "Alternative to PPS_PASSWORD Environment-Variable.")]
    string? Password
) : ICommandParameterSet;

public class FileExists : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        => value is string path && File.Exists(path) //|| Directory.Exists(path)
            ? ValidationResult.Success
            : new ValidationResult($"The path '{value}' is not found.");
}