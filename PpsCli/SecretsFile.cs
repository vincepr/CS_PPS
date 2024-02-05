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
            Console.WriteLine(password);
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
}
