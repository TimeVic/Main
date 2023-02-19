using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMembership;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Store.Project;
using TimeTracker.Web.Store.WorkspaceMemberships;
using LoadListAction = TimeTracker.Web.Store.WorkspaceMemberships.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Members.Parts;

public partial class MemberAccessForm
{
    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Parameter]
    public WorkspaceMembershipDto WorkspaceMembership { get; set; }
    
    private ProjectDto? _project;
    private UpdateRequest model = new();
    private bool _isLoading = false;

    private ICollection<MembershipAccessType> _allowedAccessLevels = new List<MembershipAccessType>()
    {
        MembershipAccessType.User,
        MembershipAccessType.Manager
    };
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(WorkspaceMembership, _projectState.Value.List);
    }
    
    private string GetProjectName(long projectId)
    {
        var project = _projectState.Value.List.First(item => item.Id == projectId);
        if (project.Client != null)
        {
            return $"{project.Name}({project.Client.Name})";
        }
        return $"{project.Name}";
    }
    
    private async Task HandleSubmit()
    {
        _isLoading = true;
        try
        {
            if (model.Access == MembershipAccessType.Manager)
            {
                model.ProjectsAccess = model.ProjectsAccess.Select(item =>
                {
                    item.HasAccess = true;
                    return item;
                }).ToList();
            }

            var membershipDto = await ApiService.WorkspaceMembershipUpdateAsync(model);
            if (membershipDto != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Member access has been changed"
                });
                DialogService.Close();
            }
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Member access saving error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }

    private void CloseModal()
    {
        DialogService.Close();
    }
}
