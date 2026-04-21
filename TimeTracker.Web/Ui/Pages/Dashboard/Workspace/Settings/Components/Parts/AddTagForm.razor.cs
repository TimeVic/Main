using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Fluxor;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tag;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class AddTagForm
{
    [Inject]
    public IState<TagState> _state { get; set; }

    private AddRequest model = new() { Name = string.Empty };
    private EditForm _form;

    private Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return Task.CompletedTask;
        }

        Dispatcher.Dispatch(new AddAction(model));
        model = new AddRequest { Name = string.Empty };
        StateHasChanged();
        return Task.CompletedTask;
    }
}
