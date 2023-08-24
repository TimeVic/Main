using Fluxor;

namespace TimeTracker.Web.Store.Common;

[FeatureState]
public record CommonState
{
    public bool IsInitialized;
    
    public bool IsWorkspaceInitialized;
}
