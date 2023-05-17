using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.Dashboard;
using TimeTracker.Web.Store.Tasks;
using TimeTracker.Web.Store.WorkspaceMemberships;
using SetListItemAction = TimeTracker.Web.Store.Tasks.SetListItemAction;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

public partial class UpdateTaskForm
{
    [Parameter]
    public TaskDto Task { get; set; }
    
    [Inject]
    public IState<Store.TasksList.TasksListState> _tasksListState { get; set; }

    [Inject] 
    private ISecurityManager _securityManager { get; set; }

    [Inject]
    private IState<WorkspaceMembershipsState> _workspaceMembershipsState { get; set; }

    private RadzenTemplateForm<UpdateRequest> _form;

    private UpdateRequest model = new();
    private bool _isLoading = false;

    private string _tabLabelAttachments
    {
        get
        {
            var label = "Attachments";
            if (Task.Attachments.Any())
            {
                label += $"({Task.Attachments.Count})";
            }

            return label;
        }
    }

    private ICollection<long> _allowedUserIds
    {
        get
        {
            return _securityManager.GetMembersWhichHaveAccessToProject(Task.TaskList.Project)
                .Select(item => item.Id)
                .ToList();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(Task);
    }

    private void HandleSubmit(UpdateRequest request)
    {
        InvokeAsync(async () =>
        {
            _isLoading = true;
            try
            {
                Dispatcher.Dispatch(new UpdateListItemAction(model));
            }
            catch (Exception)
            {
                await ToastService.ShowError("Task adding error");
            }
            finally
            {
                _isLoading = false;
            }
            StateHasChanged();
        });
    }

    private Task SubmitForm()
    {
        if (_form.IsValid)
        {
            InvokeAsync(async () => await _form.Submit.InvokeAsync());
        }
        return System.Threading.Tasks.Task.CompletedTask;
    }

    private async Task OnChangedAssigned(WorkspaceMembershipDto membership)
    {
        model.UserId = membership.User.Id;
        await SubmitForm();
    }

    private async Task OnChangeStartTime(DateTime? time)
    {
        model.StartTime = time;
        await SubmitForm();
    }

    private async Task OnChangeEndTime(DateTime? time)
    {
        model.EndTime = time;
        await SubmitForm();
    }
    
    private void OnFileUploaded(StoredFileDto uploadedFile)
    {
        Task.Attachments.Add(uploadedFile);
        Dispatcher.Dispatch(new SetAttachmentsAction(Task.Id, Task.Attachments));
    }

    private async Task OnTagsChanged(ICollection<long> selectedTagIds)
    {
        model.TagIds = selectedTagIds;
        await SubmitForm();
    }

    private void AttachmentsListUpdated(ICollection<StoredFileDto> attachments)
    {
        Task.Attachments = attachments;
        Dispatcher.Dispatch(new SetAttachmentsAction(Task.Id, Task.Attachments));
    }
}
