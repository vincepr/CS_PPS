using PpsCommon.Models.PpsModels;

namespace PpsCommon.PpsClientExtensions;

public static class CredentialExtensions
{
    public static Task<Credential> GetCredential(this PpsClient ppsClient, Guid id, CancellationToken token = default) 
        => ppsClient.GenericGet<Credential>($"/api/v5/rest/credential/{id}", token);
    
    public static Task<Credential> GetEntry(this PpsClient ppsClient, Guid id, CancellationToken token = default) 
        => ppsClient.GenericGet<Credential>($"/api/v5/rest/entries/{id}", token);

    public static Task<string> GetPassword(this PpsClient ppsClient, Guid id, CancellationToken token = default)
        => ppsClient.GenericString($"/api/v5/rest/entries/{id}/password", token);
}