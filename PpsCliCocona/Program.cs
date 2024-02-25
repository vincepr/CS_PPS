using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using PpsCommon;
using PpsCommon.PpsClientExtensions;
using Cocona;
using Cocona.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Polly;
using Polly.Extensions.Http;
using PpsCli;

class Program
{
    static void Main(string[] args)
    {
        var builder = CoconaApp.CreateBuilder(args, options =>
        {
            options.TreatPublicMethodsAsCommands = true;
            options.EnableShellCompletionSupport = true;
        });
        
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
        Console.Error.WriteLine("Todo: Still using no ssl-http-client");

#if DEBUG
        builder.Logging.AddDebug();
#else
        builder.Logging.AddSystemdConsole();
#endif

        var app = builder.Build();
        app.AddCommands<CliRootCommands>();

     


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
    }
    
    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(retryCount:6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                retryAttempt)));
    }
}

public class CliRootCommands
{
    [Command(Description = "Provide appsettings.json to generated the appsettings.secret.json")]
    public async Task Legacy(
        [Argument(Description = "Path to a .json file to parse")] [FileExists]
        string filepath, 
        OutputParams outputParams, 
        PpsConfigParams configParams,
        [FromService]CoconaAppContext ctx,
        [FromService]IHttpClientFactory clientFactory
    )
    {
        var myclient = SetupClient(configParams, clientFactory);

        // - TODO: remove debug
        var watch = System.Diagnostics.Stopwatch.StartNew();
        
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
    }
    
    [Command(Description = "Returns info about the pps-server's configuration")]
    public async Task ServerInfo(
        PpsConfigParams configParams,
        OutputParams outputParams,
        [FromService]CoconaAppContext ctx,
        [FromService]IHttpClientFactory factory)
    {
        var client = SetupClient(configParams, factory);
        HandleJsonRequest(outputParams, await client.AboutServer(ctx.CancellationToken));
    }
    
    // helper methods:
    private static void HandleJsonRequest<T>(OutputParams outputParams, T obj, bool writeIndented = true)
        => HandleStringRequest(outputParams, JsonSerializer.Serialize(
            obj, new JsonSerializerOptions { WriteIndented = writeIndented }));

    private static void HandleStringRequest(OutputParams outputParams, string output)
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

    private static void WriteToFile(OutputParams outputParams, string text)
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

    private static PpsClient SetupClient(PpsConfigParams? ppsConfigParams, IHttpClientFactory httpClientFactory)
    {
        (string? Url, string? Username,string? Password) config = PpsClientConfiguration.ParseEnvironmentVariables();
        // handle required url, username, password
        config.Url = ppsConfigParams?.Url ?? config.Url
            ?? throw new Exception("Missing ENV-Variable PPS_URL, --Url, -l");
        config.Username = ppsConfigParams?.Username ?? config.Username
            ?? throw new Exception("Missing ENV-Variable PPS_USERNAME, --username, -u");
        config.Password = ppsConfigParams?.Password ?? config.Password
            ?? throw new Exception("Missing ENV-Variable PPS_PASSWORD, --password, -p");
        var ppsConfig = new PpsClientConfiguration(config.Url, config.Username, config.Password);

        var ppsClient = new PpsClient(ppsConfig, httpClientFactory.CreateClient(nameof(PpsClient)));
        return ppsClient;
    }
}

// helper structs for Cocona:
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