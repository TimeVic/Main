using Fluxor;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class SetActiveModeEffect : Effect<SetNotesActiveModeAction>
{
    public override Task HandleAsync(SetNotesActiveModeAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoadNotesTreeAction(IsReload: true, Visibility: action.Mode));
        return Task.CompletedTask;
    }
}
