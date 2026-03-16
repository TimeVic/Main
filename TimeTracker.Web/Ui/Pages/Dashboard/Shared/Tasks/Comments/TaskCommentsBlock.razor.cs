using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Ui.Pages.Dashboard.Shared.Tasks.Comments;

public partial class TaskCommentsBlock
{
    [Parameter]
    public TaskDto Task { get; set; }

    [Inject]
    public IState<AuthState> AuthState { get; set; }
    
    private IEnumerable<TaskCommentDto> _comments { get; set; } = new List<TaskCommentDto>();
    private bool _isLoading { get; set; } = false;
    private int _page { get; set; } = 1;
    private bool _isHasMore { get; set; } = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadItems(true);
    }

    private async Task LoadItems(bool isReset = false)
    {
        _isLoading = true;
        if (isReset)
        {
            _comments = new List<TaskCommentDto>();
            _page = 1;
        }
        else
        {
            _page++;
        }
        var response = await ApiService.TaskCommentsGetListAsync(Task.Id, _page);
        if (response != null)
        {
            _comments = _comments.Concat(response.Items);
            _isHasMore = response.IsHasMore;
        }
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
                        return comment;
                    }

                    return item;
                });
            }
            else
            {
                _comments = _comments.Prepend(comment);
            }
            StateHasChanged();
        });
    }

    private void OnCommentDeleted(TaskCommentDto comment)
    {
        _comments = _comments.Where(item => item.Id != comment.Id).ToList();
        StateHasChanged();
    }

    private async Task LoadMore()
    {
        await LoadItems();
    }
}
