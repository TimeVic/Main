using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Project;
using LoadListAction = TimeTracker.Client.Core.Store.WorkspaceMembers.LoadListAction;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class UpdateMemberModal
{
    private sealed class ProjectAccessGroup
    {
        public required string ClientName { get; init; }

        public required IReadOnlyCollection<MemberProjectAccessRequest> ProjectsAccess { get; init; }
    }

    [Parameter]
    public required WorkspaceMemberDto Member { get; set; }

    [Parameter]
    public required bool IsOpened { get; set; }

    [Parameter]
    public EventCallback<bool> IsOpenedChanged { get; set; }

    [Inject]
    private IState<ProjectState> ProjectState { get; set; } = default!;

    private readonly ICollection<MembershipAccessType> _allowedAccessLevels = new List<MembershipAccessType>
    {
        MembershipAccessType.User,
        MembershipAccessType.Manager
    };

    private UpdateRequest _model = new();
    private EditForm _form = default!;
    private bool _isLoading;

    protected override void OnParametersSet()
    {
        _model = new UpdateRequest();
        _model.Fill(Member, ProjectState.Value.List);
        base.OnParametersSet();
    }

    private string GetProjectName(Guid projectId)
    {
        var project = GetProject(projectId);
        if (project == null) return projectId.ToString();

        // Project titles omit client names because the access list is grouped by client.
        return project.Name;
    }

    private IReadOnlyCollection<ProjectAccessGroup> GetProjectAccessGroups()
    {
        return _model.ProjectsAccess
            .Select(item => new
            {
                ProjectAccess = item,
                Project = GetProject(item.ProjectId)
            })
            .GroupBy(item => item.Project?.Client?.Id)
            .Select(group => new ProjectAccessGroup
            {
                ClientName = GetClientGroupName(group.First().Project),
                ProjectsAccess = group.Select(item => item.ProjectAccess).ToList()
            })
            .ToList();
    }

    private ProjectDto? GetProject(Guid projectId)
    {
        return ProjectState.Value.List.FirstOrDefault(p => p.Id == projectId);
    }

    private string GetClientGroupName(ProjectDto? project)
    {
        return project?.Client?.Name ?? DashboardLocalizer["NoClient"].Value;
    }

    private async Task Submit()
    {
        if (!_form.EditContext!.Validate())
        {
            return;
        }

        if (_model.Access == MembershipAccessType.Manager)
        {
            foreach (var item in _model.ProjectsAccess)
            {
                item.HasAccess = true;
            }
        }

        _isLoading = true;
        try
        {
            var result = await ApiService.WorkspaceMemberUpdateAsync(_model);
            if (result != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                ToastService.ShowInfo(DashboardLocalizer["UpdateMemberModal_MemberAccessUpdated"].Value);
                await OnCloseModal();
            }
        }
        catch (Exception)
        {
            ToastService.ShowError(DashboardLocalizer["UpdateMemberModal_MemberUpdateError"].Value);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }

    private async Task OnCloseModal()
    {
        await IsOpenedChanged.InvokeAsync(false);
        IsOpened = false;
    }
}






