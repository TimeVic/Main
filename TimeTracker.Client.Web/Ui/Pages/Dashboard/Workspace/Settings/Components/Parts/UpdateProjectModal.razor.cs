using Fluxor;
using LumexUI;
using Markdig.Extensions.TaskLists;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Client.Core.Store.Project;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class UpdateProjectModal
{
    [Parameter]
    public required ProjectDto Project { get; set; }
    
    [Parameter]
    public required bool IsOpened { get; set; } = false;
    
    [Parameter]
    public virtual EventCallback<bool> IsOpenedChanged { get; set; }
    
    [Parameter]
    public virtual EventCallback<ProjectDto?> OnAdded { get; set; }
    
    [Inject]
    public virtual IState<ProjectState> _state { get; set; }
    
    private UpdateRequest model = new() { Name = string.Empty };
    private EditForm _form;
    private LumexModal modal;

    protected override async Task OnInitializedAsync()
    {
        model.Fill(Project);
        await base.OnInitializedAsync();
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        Dispatcher.Dispatch(new UpdateAction(model));
        await OnCloseModal();
        StateHasChanged();    
    }
    
    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
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
