namespace TimeTracker.Business.Common.Constants.Http;

public static class AuthConstants
{
    public const string WorkspaceIdHeaderName = "X-Workspace-Id";

    /// <summary>
    /// JWT token query parameter key
    /// </summary>
    public const string ApiTokenKey = "api_token";
    
    /// <summary>
    /// Web socket api token key
    /// </summary>
    public const string WebSocketAccessApiTokenKey = "access_token";
}
