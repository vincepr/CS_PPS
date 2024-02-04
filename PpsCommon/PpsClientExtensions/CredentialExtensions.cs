using PpsCommon.Models.PpsModels;

namespace PpsCommon.PpsClientExtensions;

public static class CredentialExtensions
{
    public static Task<Credential> GetCredential(this PpsClient ppsClient, Guid id) 
        => ppsClient.GenericGet<Credential>($"/api/v5/rest/credential/{id}");
    
    public static Task<Credential> GetEntry(this PpsClient ppsClient, Guid id) 
        => ppsClient.GenericGet<Credential>($"/api/v5/rest/entries/{id}");

    public static Task<string> GetPassword(this PpsClient ppsClient, Guid id)
        => ppsClient.GenericString($"/api/v5/rest/entries/{id}/password");
}