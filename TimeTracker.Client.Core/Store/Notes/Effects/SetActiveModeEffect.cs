using Fluxor;

namespace TimeTracker.Client.Core.Store.Notes.Effects;

public class SetActiveModeEffect : Effect<SetNotesActiveModeAction>
{
    public override Task HandleAsync(SetNotesActiveModeAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LoadNotesTreeAction(
            IsReload: true,
            InitialNoteId: action.InitialNoteId,
            Visibility: action.Mode
        ));
        return Task.CompletedTask;
    }
}
