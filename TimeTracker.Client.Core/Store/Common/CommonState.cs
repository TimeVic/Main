using Fluxor;

namespace TimeTracker.Client.Core.Store.Common;

[FeatureState]
public record CommonState
{
    public bool IsInitialized;
    
    public bool IsWorkspaceInitialized;
}
