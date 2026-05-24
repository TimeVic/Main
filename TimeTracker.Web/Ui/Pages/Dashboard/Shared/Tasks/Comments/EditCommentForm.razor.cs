using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Services.UI;
using TimeTracker.Client.Core.Store.WorkspaceMembers;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Comments;

public partial class EditCommentForm: IDisposable
{
    [Parameter]
    public TaskCommentDto Comment { get; set; } = null!;
    
    [Parameter]
    public EventCallback<TaskCommentDto> OnSaved { get; set; }
    
    [Parameter]
    public EventCallback<TaskCommentDto> OnDeleted { get; set; }
    
    [Parameter]
    public ProjectDto? Project { get; set; }
    
    [Parameter]
    public Guid TaskId { get; set; }
    
    [Inject]
    public MarkdownService MarkdownService { get; set; } = null!;

    [Inject]
    private IState<WorkspaceMembersState> WorkspaceMembersState { get; set; } = null!;
    
    [Inject]
    private ISecurityManager SecurityManager { get; set; } = null!;
    
    private AddRequest _model = new();
    private bool _isLoading;
    private bool _isEditMode;
    private bool _isDeleteConfirmationOpen;
    private EditForm? _form;
    private EditContext _editContext = null!;
    private Guid? _loadedCommentId;
    private bool _isNewComment => Comment.Id == Guid.Empty;
    private bool _canEdit => Comment.User?.Id == AuthState.Value.User?.Id;
    private bool IsEditorVisible => _isNewComment || _isEditMode;
    private bool IsCommentEmpty => string.IsNullOrWhiteSpace(_model.Comment);
    private string AuthorName => Comment.User?.Name ?? DashboardLocalizer["User"].Value;
    private string SubmitButtonText => _isNewComment
        ? DashboardLocalizer["TaskComment_AddComment"].Value
        : DashboardLocalizer["Save"].Value;
    private string ContainerClass => _isNewComment
        ? "pb-1"
        : "border-t border-slate-200 py-4 first:border-t-0 first:pt-0 last:pb-0";
    private bool _isSubscribersSelectAvailable =>
        Project != null &&
        SecurityManager.GetMembersWhichHaveAccessToProject(Project)
            .Any(member => member.Access != MembershipAccessType.Owner);
    private MarkupString CommentHtml => (MarkupString) MarkdownService.ToHtml(Comment.Comment);
    private string FormattedCreatedAt => Comment.CreatedAt == default
        ? string.Empty
        : Comment.CreatedAt.ToLocalTime().TimeAgo(DateTimeKind.Local);

    protected override void OnInitialized()
    {
        base.OnInitialized();
        WorkspaceMembersState.StateChanged += OnWorkspaceMembersStateChanged;
    }

    protected override void OnParametersSet()
    {
        if (_loadedCommentId == Comment.Id && _isEditMode)
        {
            return;
        }

        FillFormFromComment();
    }

    public void Dispose()
    {
        WorkspaceMembersState.StateChanged -= OnWorkspaceMembersStateChanged;
    }

    private void OnWorkspaceMembersStateChanged(object? sender, EventArgs args)
    {
        InvokeAsync(StateHasChanged);
    }

    private void FillFormFromComment()
    {
        _model = new AddRequest
        {
            TaskId = TaskId != Guid.Empty ? TaskId : Comment.Task?.Id ?? Guid.Empty,
            Comment = Comment.Comment ?? string.Empty,
            WatcherIds = Comment.Watchers?.Select(item => item.Id).ToList() ?? new List<Guid>()
        };
        _editContext = new EditContext(_model);
        _loadedCommentId = Comment.Id;
    }
    
    private Task OnCommentChanged(string comment)
    {
        _model.Comment = comment;
        _editContext.NotifyFieldChanged(new FieldIdentifier(_model, nameof(_model.Comment)));
        return Task.CompletedTask;
    }

    private async Task Submit()
    {
        if (_form?.EditContext == null || !_form.EditContext.Validate() || IsCommentEmpty)
        {
            return;
        }
        
        _isLoading = true;
        try
        {
            TaskCommentDto? responseDto;
            if (_isNewComment)
            {
                responseDto = await ApiService.TaskCommentAddAsync(_model);
            }
            else
            {
                responseDto = await ApiService.TaskCommentUpdateAsync(new UpdateRequest(Comment.Id, _model));
            }

            if (responseDto != null)
            {
                await OnSaved.InvokeAsync(responseDto);
            }

            ResetForm();
        }
        catch (Exception e)
        {
            ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnClickEdit()
    {
        _isEditMode = true;
    }

    private void ResetForm()
    {
        _isEditMode = false;
        FillFormFromComment();
    }

    private void OpenDeleteConfirmation()
    {
        _isDeleteConfirmationOpen = true;
    }

    private void CloseDeleteConfirmation()
    {
        _isDeleteConfirmationOpen = false;
    }

    private async Task OnConfirmDelete()
    {
        _isDeleteConfirmationOpen = false;
        _isLoading = true;
        try
        {
            await ApiService.TaskCommentDeleteAsync(Comment.Id);
            await OnDeleted.InvokeAsync(Comment);
        }
        catch (Exception e)
        {
            ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
