using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Store.Project;
using LoadListAction = TimeTracker.Web.Store.WorkspaceMembers.LoadListAction;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Workspace.Settings.Components.Parts;

public partial class UpdateMemberModal
{
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
        var project = ProjectState.Value.List.FirstOrDefault(p => p.Id == projectId);
        if (project == null) return projectId.ToString();
        return project.Client != null ? $"{project.Name} ({project.Client.Name})" : project.Name;
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
                ToastService.ShowInfo("Member access has been updated");
                await OnCloseModal();
            }
        }
        catch (Exception)
        {
            ToastService.ShowError("Member update error");
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








