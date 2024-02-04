using System.Formats.Tar;
using System.Text.Json;
using PpsCommon;
using PpsCommon.Models.PpsModels;
using PpsCommon.PpsClientExtensions;

namespace PpsCli;

public static class SecretsFile
{
    public static async Task<string> Build(PpsClient ppsClient, string filepath, CancellationToken workToken,
        bool writeIndented = true)
    {
        var buf = await File.ReadAllTextAsync(filepath, workToken);
        var secretsMap = JsonSerializer.Deserialize<SecretsFileDto>(buf) ?? throw new JsonException($"Could not parse Json of file {filepath}");
        List<SecretsFileEntry> secrets = new();
        foreach(var (secretName, id) in secretsMap.PpsSecrets)
        {
            var entry = await ppsClient.GetEntry(id);
            var password = await ppsClient.GetPassword(id);
            Console.WriteLine(password);
            secrets.Add(new SecretsFileEntry(
                SecretName: secretName,
                Id: id,
                Name: entry.Name ?? "",
                Username: entry.Username ?? "",
                Password: password,
                Url: entry.Url ?? "null",
                Notes: entry.Notes ?? "",
                CustomUserFields: entry.CustomUserFields ?? new()
                ));
        }

        return JsonSerializer.Serialize(secrets, new JsonSerializerOptions { WriteIndented = writeIndented });
    }
}
