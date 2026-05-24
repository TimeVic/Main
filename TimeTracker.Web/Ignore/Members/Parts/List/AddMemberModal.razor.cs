using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Web.Pages.Dashboard.Members.Parts.List;

public partial class AddMemberModal
{
    [CascadingParameter] 
    FluentDialog MudDialog { get; set; }

    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private MudForm _form;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }

        Dispatcher.Dispatch(new AddNewMemberAction(model.Email));
        OnCloseModal();
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
