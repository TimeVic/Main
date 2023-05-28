using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks.Comments;

public partial class TaskCommentsBlock
{
    [Parameter]
    public TaskDto Task { get; set; }

    [Inject]
    public IState<AuthState> AuthState { get; set; }
    

    private ICollection<TaskCommentDto> _comments { get; set; } = new List<TaskCommentDto>();
    private bool _isLoading { get; set; } = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadItems(1);
    }

    private async Task LoadItems(int page)
    {
        _isLoading = true;
        if (page == 1)
        {
            _comments = new List<TaskCommentDto>();
        }
        var response = await ApiService.TaskCommentsGetListAsync(Task.Id, page);
        _comments = _comments.Concat(response.Items).ToList();
        _isLoading = false;
    }

    private void OnCommentSaved(TaskCommentDto comment)
    {
        InvokeAsync(() =>
        {
            if (_comments.Any(item => item.Id == comment.Id))
            {
                _comments = _comments.Select(item =>
                {
                    if (item.Id == comment.Id)
                    {
                        Debug.Log(comment);
                        return comment;
                    }

                    return item;
                }).ToList();
            }
            else
            {
                _comments = _comments.Prepend(comment).ToList();
            }
            StateHasChanged();
        });
    }

    private void OnCommentDeleted(TaskCommentDto comment)
    {
        _comments = _comments.Where(item => item.Id != comment.Id).ToList();
        StateHasChanged();
    }
}
