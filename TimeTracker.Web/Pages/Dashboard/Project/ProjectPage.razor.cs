using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Services.Http;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Pages.Dashboard.Project;

public partial class ProjectPage
{
    [Parameter]
    public long ProjectId { get; set; }

    [Inject]
    private IState<ProjectState> _state { get; set; }

    [Inject]
    private NavigationManager _navigationManager { get; set; }
    
    [Inject]
    private IApiService _apiService { get; set; }
    
    private ProjectDto? _project;
    private UpdateRequest model = new();
    private bool _isLoading = false;
    
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (!_state.Value.List.Any())
        {
            _navigationManager.NavigateTo(SiteUrl.Dashboard_Projects);
            return;
        }

        _project = _state.Value.List.FirstOrDefault(item => item.Id == ProjectId);
        if (_project == null)
        {
            _navigationManager.NavigateTo(SiteUrl.Dashboard_Projects);
            return;
        }
        model.Fill(_project);
    }
    
    private void OnChangeClient(long clientId)
    {
        model.ClientId = clientId == 0 ? null : clientId;
    }
    
    private async Task HandleSubmit()
    {
        _isLoading = true;
        try
        {
            var project = await _apiService.ProjectUpdateAsync(model);
            if (project != null)
            {
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Project is sent"
                });
            }
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Project saving error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
    
    private void OnClickBackButton()
    {
        _navigationManager.NavigateTo(SiteUrl.Dashboard_Projects);
    }
}
