using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Store.Tasks;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts;

public partial class UpdateTaskForm
{
    [Parameter]
    public TaskDto Task { get; set; }
    
    [Inject]
    public IState<TimeTracker.Web.Store.TasksList.TasksListState> TasksListState { get; set; }

    private RadzenTemplateForm<UpdateRequest> _form;

    private UpdateRequest model = new();
    private bool _isLoading = false;
    
    private readonly int _descriptionTextAreaRowsMin = 10;
    private readonly int _descriptionTextAreaRowsMax = 20;
    private int _descriptionTextAreaRows = 6;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(Task);
        ResizeDescriptionField(model.Description);
    }

    private async Task HandleSubmit(UpdateRequest request)
    {
        _isLoading = true;
        try
        {
            var responseDto = await ApiService.TasksUpdateAsync(model);
            if (responseDto != null)
            {
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
            }
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Task adding error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }

    private async Task SubmitForm()
    {
        if (_form.IsValid)
        {
            await _form.Submit.InvokeAsync();
        }
    }

    private void ResizeDescriptionTextArea(ChangeEventArgs elementEvent)
    {
        var description = (string)(elementEvent.Value ?? "");
        ResizeDescriptionField(description);
    }

    private void ResizeDescriptionField(string? description)
    {
        description ??= "";
        _descriptionTextAreaRows = Math.Max(description.Split('\n').Length, description.Split('\r').Length);
        _descriptionTextAreaRows = Math.Max(_descriptionTextAreaRows, _descriptionTextAreaRowsMin);
        _descriptionTextAreaRows = Math.Min(_descriptionTextAreaRows, _descriptionTextAreaRowsMax);
    }
}
