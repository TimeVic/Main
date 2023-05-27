using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;
using TimeTracker.Web.Store.Tag;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks.Comments;

public partial class EditCommentForm
{
    [Parameter]
    public TaskCommentDto Comment { get; set; }
    
    [Parameter]
    public EventCallback<TaskCommentDto> OnSaved { get; set; }
    
    [Parameter]
    public EventCallback<TaskCommentDto> OnDeleted { get; set; }
    
    [Inject]
    public IState<AuthState> AuthState { get; set; }
    
    [Inject]
    public MarkdownService _markdownService { get; set; }
    
    private RadzenTemplateForm<AddRequest> _form;
    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isEditMode = false;
    private bool _isNewComment
    {
        get => Comment.Id == 0;
    }
    
    private string _userName
    {
        get => _isNewComment ? AuthState.Value.User.Name : Comment.User.Name;
    }
    
    private bool _canEdit
    {
        get => Comment.User?.Id == AuthState.Value.User.Id;
    }
    
    public MarkupString CommentHtml => (MarkupString) _markdownService.ToHtml(model.Comment);
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(Comment);
    }
    
    private void HandleSubmit(AddRequest request)
    {
        InvokeAsync(async () =>
        {
            _isLoading = true;
            try
            {
                TaskCommentDto? responseDto = null;
                if (_isNewComment)
                {
                    responseDto = await ApiService.TaskCommentAddAsync(model);
                }
                else
                {
                    var updateModel = new UpdateRequest(Comment.Id, model);
                    responseDto = await ApiService.TaskCommentUpdateAsync(updateModel);
                }
                await InvokeAsync(async () => await OnSaved.InvokeAsync(responseDto));
                ResetForm();
            }
            catch (Exception e)
            {
                await ToastService.ShowError(e.Message);
            }
            finally
            {
                _isLoading = false;
            }
            StateHasChanged();    
        });
    }

    private void OnClickEditComment()
    {
        _isEditMode = true;
    }

    private void ResetForm()
    {
        _isEditMode = false;
        model.Fill(Comment);
    }


    private void OnClickEdit()
    {
        _isEditMode = true;
    }

    private void OnFocusOnEditField()
    {
        _isEditMode = true;
    }

    private async Task OnClickDelete()
    {
        var isOk = await DialogService.Confirm(
            "Are you sure you want to remove this comment?",
            "Delete confirmation",
            new ConfirmOptions()
            {
                OkButtonText = "Delete",
                CancelButtonText = "Cancel"
            }
        );
        if (!isOk.HasValue || !isOk.Value)
        {
            return;
        }
        await InvokeAsync(async () =>
        {
            _isLoading = true;
            try
            {
                await ApiService.TaskCommentDeleteAsync(Comment.Id);
                await OnDeleted.InvokeAsync(Comment);
                ResetForm();
            }
            catch (Exception e)
            {
                await ToastService.ShowError(e.Message);
            }
            finally
            {
                _isLoading = false;
            }
            StateHasChanged();    
        });
    }
}
