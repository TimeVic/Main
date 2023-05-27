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
    

    private IEnumerable<TaskCommentDto> _comments { get; set; } = new List<TaskCommentDto>();
    private bool _isLoading { get; set; } = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadItems(1);
    }

    private async Task LoadItems(int page)
    {
        _isLoading = true;
        var response = await ApiService.TaskCommentsGetListAsync(Task.Id, page);
        _comments = response.Items;
        _isLoading = false;
    }

    private void OnCommentSaved(TaskCommentDto comment)
    {
        var isExists = false;
        _comments = _comments.Select(item =>
        {
            if (item.Id == comment.Id)
            {
                isExists = true;
                return comment;
            }

            return item;
        });
        if (!isExists)
        {
            _comments = _comments.Prepend(comment);
        }
    }
}
