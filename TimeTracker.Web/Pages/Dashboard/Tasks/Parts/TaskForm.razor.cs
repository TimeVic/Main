using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class TaskForm
{
    private UpdateRequest model = new();
    private bool _isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
}
