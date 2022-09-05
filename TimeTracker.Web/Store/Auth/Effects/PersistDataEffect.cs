using Fluxor;
using TimeTracker.Web.Store.Common.Actions;
using TimeTracker.Web.Store.Common.Effects;

namespace TimeTracker.Web.Store.Auth.Effects;

public class LogoutEffect: AEffectPersistData<LogoutAction>
{
    public override Task HandleAsync(LogoutAction pageAction, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new PersistDataAction());
        return Task.CompletedTask;
    }
}
