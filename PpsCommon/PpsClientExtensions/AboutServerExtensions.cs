using PpsCommon.Models.PpsModels;

namespace PpsCommon.PpsClientExtensions;

public static class AboutServerExtensions
{
    public static Task<AboutServer> AboutServer(this PpsClient ppsClient) 
        => ppsClient.GenericGet<AboutServer>("/api/v5/rest/about/");
}