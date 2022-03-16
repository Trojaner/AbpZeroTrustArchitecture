namespace Sample.Remoting;

public class RemotingOptions
{
    public bool UseAuthentication { get; set; } = true;
    public bool UseRemoteAuditing { get; set; } = true;
    public bool UseRemotePermissionChecks { get; set; } = true;
}