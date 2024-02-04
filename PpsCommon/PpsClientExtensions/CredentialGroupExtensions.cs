using PpsCommon.Models.PpsModels;

namespace PpsCommon.PpsClientExtensions;

public static class CredentialGroupExtensions
{
    public static Task<CredentialGroup> GetFolderRoot(this PpsClient ppsClient) 
        => ppsClient.GenericGet<CredentialGroup>("/api/v5/rest/folders");
    
    public static Task<CredentialGroup> GetFolder(this PpsClient ppsClient, Guid credentialGroup) 
        => ppsClient.GenericGet<CredentialGroup>($"/api/v5/rest/folders/{credentialGroup}");
}