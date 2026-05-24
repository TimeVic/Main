using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Client.Core.Store.Project;
using LoadListAction = TimeTracker.Client.Core.Store.WorkspaceMembers.LoadListAction;

namespace TimeTracker.Client.Web.Pages.Dashboard.Members.Parts;

public partial class MemberAccessModal
{
    [CascadingParameter] 
    MudDialogInstance MudDialog { get; set; }
    
    [Inject]
    public IState<ProjectState> _projectState { get; set; }
    
    [Parameter]
    public WorkspaceMemberDto WorkspaceMember { get; set; }
    
    private ProjectDto? _project;
    private UpdateRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private FluentEditForm _form;

    private ICollection<MembershipAccessType> _allowedAccessLevels = new List<MembershipAccessType>()
    {
        MembershipAccessType.User,
        MembershipAccessType.Manager
    };
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(WorkspaceMember, _projectState.Value.List);
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
    
    private async Task Submit()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }
        
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

            var memberDto = await ApiService.WorkspaceMemberUpdateAsync(model);
            if (memberDto != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                await ToastService.ShowInfo("Member access has been changed");
                OnCloseModal();
            }
        }
        catch (Exception)
        {
            await ToastService.ShowError("Member access saving error");
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
