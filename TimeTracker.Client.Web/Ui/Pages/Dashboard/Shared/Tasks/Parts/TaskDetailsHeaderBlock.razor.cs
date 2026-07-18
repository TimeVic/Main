using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Client.Core.Store.Tasks;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Shared.Tasks.Parts;

public partial class TaskDetailsHeaderBlock
{
    [Parameter]
    public required TaskDto Task { get; set; }

    [Parameter]
    public bool IsFullPage { get; set; }

    [Parameter]
    public EventCallback<string> TitleChanged { get; set; }

    private string TaskUrl => string.Format(SiteUrl.Dashboard_Task, Task.Id);

    private async Task SaveTitleAsync(string title)
    {
        var request = new UpdateRequest();
        request.Fill(Task);
        request.Title = title;

        var response = await ApiService.TasksUpdateAsync(request)
            ?? throw new InvalidOperationException(DashboardLocalizer["TaskDetail_TitleSaveFailed"]);

        Task.Title = response.Title;
        Dispatcher.Dispatch(new SetListItemAction(response));
        Dispatcher.Dispatch(new SetOverdueTasksListItemAction(response));
        await TitleChanged.InvokeAsync(response.Title);
    }
}
