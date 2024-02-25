using System.Formats.Tar;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using PpsCommon;
using PpsCommon.Models.PpsModels;
using PpsCommon.PpsClientExtensions;

namespace PpsCli;

public static class SecretsFile
{
    public static async Task<string> BuildJsonString(PpsClient ppsClient, string filepath, CancellationToken workToken,
        bool useSecretName = true,
        bool writeIndented = true)
    {
        var appsettingsJsonOptions = new JsonSerializerOptions()
        {
            // Multilingual Plane (U+0000..U+FFFF).
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        
        var buf = await File.ReadAllTextAsync(filepath, workToken);
        var secretsMap = JsonSerializer.Deserialize<SecretsFileDto>(buf) ?? throw new JsonException($"Could not parse Json of file {filepath}");
        
        Dictionary<string, SecretsFileEntry> secrets = new();
        foreach(var (secretName, id) in secretsMap.PpsSecrets)
        {
            var entry = await ppsClient.GetEntry(id);
            var password = await ppsClient.GetPassword(id);
            secrets.Add(useSecretName ? secretName : id.ToString(), new SecretsFileEntry(
                Id: id,
                Name: entry.Name ?? "",
                Username: entry.Username ?? "",
                Password: password,
                Url: entry.Url ?? "null",
                Notes: entry.Notes ?? "",
                CustomUserFields: entry.CustomUserFields ?? new()
                ));
        }
        
        // this encoder will not escape unicode like ä, ü, ö. BUT WILL STILL ESCAPE: + 
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            // Multilingual Plane (U+0000..U+FFFF) also + usw..., BUT NOT html-sensitive chars < > & '
            // json encoders always block \
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = writeIndented
        };
        
        return JsonSerializer.Serialize(secrets, options);
    }
    
    public static async Task<string> ParallelBuildJsonString(PpsClient ppsClient, string filepath, CancellationToken workToken,
        bool useSecretName = true,
        bool writeIndented = true)
    {
        var appsettingsJsonOptions = new JsonSerializerOptions()
        {
            // Multilingual Plane (U+0000..U+FFFF).
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        
        var buf = await File.ReadAllTextAsync(filepath, workToken);
        var secretsMap = JsonSerializer.Deserialize<SecretsFileDto>(buf) ?? throw new JsonException($"Could not parse Json of file {filepath}");

        var parallelOpts = new ParallelOptions()
        {
            CancellationToken = workToken,
            MaxDegreeOfParallelism = 5,
        };

        System.Collections.Concurrent.ConcurrentDictionary<string, SecretsFileEntry> bagOfSecrets = new();
        await Parallel.ForEachAsync(secretsMap.PpsSecrets, parallelOpts, async (entry, innerToken) =>
        {
            var (secretName, id) = entry;
            var ppsEntryTask = ppsClient.GetEntry(id, innerToken);
            var passwordTask = ppsClient.GetPassword(id, innerToken);
            await Task.WhenAll(ppsEntryTask, passwordTask);
            var (ppsEntry, password) = Tuple.Create(ppsEntryTask.Result, passwordTask.Result);
            bagOfSecrets[useSecretName ? secretName : id.ToString()] = new SecretsFileEntry(
                Id: id,
                Name: ppsEntry.Name ?? "",
                Username: ppsEntry.Username ?? "",
                Password: password,
                Url: ppsEntry.Url ?? "null",
                Notes: ppsEntry.Notes ?? "",
                CustomUserFields: ppsEntry.CustomUserFields ?? new()
            );
        });
        
        // this encoder will not escape unicode like ä, ü, ö. BUT WILL STILL ESCAPE: + 
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            // Multilingual Plane (U+0000..U+FFFF) also + usw..., BUT NOT html-sensitive chars < > & '
            // json encoders always block \
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = writeIndented
        };
        
        return System.Text.RegularExpressions.Regex.Unescape(JsonSerializer.Serialize(bagOfSecrets, options));
    }
}
