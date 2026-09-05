using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Clients.Parts;

public partial class UpdateProjectModal
{
    [CascadingParameter]
    public AppModalInstance? ModalInstance { get; set; }

    [Parameter]
    public required ProjectDto Project { get; set; }
    
    [Parameter]
    public virtual EventCallback<ProjectDto?> OnAdded { get; set; }
    
    [Inject]
    public virtual IState<ProjectState> _state { get; set; } = default!;
    
    private UpdateRequest model = new() { Name = string.Empty };
    private EditForm _form = default!;

    protected override async Task OnInitializedAsync()
    {
        model.Fill(Project);
        await base.OnInitializedAsync();
    }

    protected override void OnParametersSet()
    {
        if (Project != null && model.ProjectId != Project.Id)
        {
            model.Fill(Project);
        }
        base.OnParametersSet();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateAction(model));
        if (ModalInstance != null)
        {
            await ModalInstance.Close(AppModalResult.Ok());
        }
        StateHasChanged();    
    }

    private void OnClientChanged(ClientDto? client)
    {
        model.ClientId = client?.Id ?? Guid.Empty;
    }

    private void OnChangeDefaultHourlyRate(decimal? hourlyRate)
    {
        model.DefaultHourlyRate = hourlyRate;
    }
}
