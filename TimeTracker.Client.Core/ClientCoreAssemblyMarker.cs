using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core;

public class ClientCoreAssemblyMarker
{
    public static Type ServerFluxorStateType => typeof(AuthState);
}
