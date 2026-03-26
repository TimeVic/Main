using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.Comments;
using TimeTracker.Business.Extensions;
using TimeTracker.Web.Services.UI;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Comments;

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
    
    public IEnumerable<long> WatcherIds { get; set; } = new List<long>();
    
    private AddRequest model = new();
    private bool _isLoading = false;
    private bool _isEditMode = false;
    private EditForm? _form;
    private bool _isValid = false;
    private bool _isNewComment => Comment.Id == Guid.Empty;

    private string _userName => _isNewComment ? AuthState.Value.User.Name : Comment.User.Name;

    private bool _canEdit => Comment.User?.Id == AuthState.Value.User.Id;

    public MarkupString CommentHtml => (MarkupString) _markdownService.ToHtml(model.Comment);

    private int _commentLinesCount
    {
        get
        {
            var lines = $"{model.Comment}".CountLines();
            return lines > 3 ? lines : 3;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        RunAfterRendered(() => _form?.EditContext!.NotifyValidationStateChanged());
        
        await base.OnInitializedAsync();
        model.Fill(Comment);
    }
    
    private void Submit()
    {
        if (!_form!.EditContext!.Validate())
        {
            return;
        }
        
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
                ToastService.ShowError(e.Message);
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
        // _form?.ResetValidation();
    }


    private void OnClickEdit()
    {
        _isEditMode = true;
    }

    private void OnFocusOnEditField()
    {
        if (!_isEditMode)
        {
            _isEditMode = true;    
        }
    }

    private async Task OnClickDelete()
    {
        // var isOk = await _dialogService.ShowDeleteConfirmationDialog(
        //     "Are you sure you want to remove this comment?"
        // );
        // if (!isOk.HasValue || !isOk.Value)
        // {
        //     return;
        // }
        // await InvokeAsync(async () =>
        // {
        //     _isLoading = true;
        //     try
        //     {
        //         await ApiService.TaskCommentDeleteAsync(Comment.Id);
        //         await OnDeleted.InvokeAsync(Comment);
        //         ResetForm();
        //     }
        //     catch (Exception e)
        //     {
        //         ToastService.ShowError(e.Message);
        //     }
        //     finally
        //     {
        //         _isLoading = false;
        //     }
        //     StateHasChanged();    
        // });
    }
}
