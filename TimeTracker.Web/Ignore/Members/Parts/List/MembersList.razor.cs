using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.WorkspaceMember;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Web.Shared.Components.Form;
using TimeTracker.Client.Core.Store.Project;
using TimeTracker.Client.Core.Store.WorkspaceMembers;
using LoadListAction = TimeTracker.Client.Core.Store.WorkspaceMembers.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Members.Parts.List
{
    public partial class MembersList
    {
        [Inject] 
        private IState<WorkspaceMembersState> _state { get; set; }
    
        [Inject]
        public IState<ProjectState> _projectState { get; set; }
        
        private IEnumerable<ProjectDto> _selectedProjects = new List<ProjectDto>();
        private ICollection<MembershipAccessType> _allowedAccessLevels = new List<MembershipAccessType>()
        {
            MembershipAccessType.User,
            MembershipAccessType.Manager
        };

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Dispatcher.Dispatch(new LoadListAction(true));
        }

        private string GetProjectNames(WorkspaceMemberDto memberDto)
        {
            return string.Join(
                ", ",
                memberDto.ProjectAccesses.Select(
                    item => _projectState.Value.List.FirstOrDefault(project => project.Id == item.Project.Id)
                )
                    .Where(item => item != null)
                    .Select(item => item.Name)
                    .ToList()
            );
        }
        
        private void OnProjectsChanged(IEnumerable<ProjectDto> projects)
        {
            _selectedProjects = projects;
        }

        private async Task OnAdd()
        {
            await ModalDialogService.ShowAddWorkspaceMemberModal();
        }

        private async Task OnEdit(WorkspaceMemberDto item)
        {
            await ModalDialogService.ShowUpdateWorkspaceMemberModal(item);
        }

        private async Task OnDelete(WorkspaceMemberDto item)
        {
            var isOk = await ModalDialogService.ShowDeleteConfirmationDialog();
            if (isOk.HasValue && isOk.Value)
            {
                Dispatcher.Dispatch(new DeleteMemberAction(item));
            }
            await Task.CompletedTask;
        }
    }
}
