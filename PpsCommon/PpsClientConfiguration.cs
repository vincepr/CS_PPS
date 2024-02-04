namespace PpsCommon;

public record PpsClientConfiguration(string BaseUrl, string Username, string Password)
{
    public static PpsClientConfiguration ParseEnvironmentVariables()
    {
        var baseUrl = Environment.GetEnvironmentVariable("PPS_BASEURL") 
                      ?? throw new Exception("Missing ENV-Variable PPS_BASEURL");
        var username = Environment.GetEnvironmentVariable("PPS_USERNAME")
                      ?? throw new Exception("Missing ENV-Variable PPS_USERNAME");
        var password = Environment.GetEnvironmentVariable("PPS_PASSWORD")
                      ?? throw new Exception("Missing ENV-Variable PPS_PASSWORD");
        return new PpsClientConfiguration(baseUrl, username, password);
    }
};