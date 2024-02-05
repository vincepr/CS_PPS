namespace PpsCli;

public record SecretsFileEntry(
    Guid Id,
    string Name,
    string Username,
    string Password,
    string Url,
    string Notes,
    Dictionary<string, string> CustomUserFields
    );