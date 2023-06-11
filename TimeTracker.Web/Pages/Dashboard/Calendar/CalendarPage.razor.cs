using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Services.UI;

namespace TimeTracker.Web.Pages.Dashboard.Calendar;

public partial class CalendarPage
{
    [Inject]
    public ILogger<CalendarPage> _logger { get; set; }
    
    [Inject]
    public TooltipService _tooltipService { get; set; }
    
    [Inject] 
    private ModalDialogProviderService _modalDialogProviderService { get; set; }
    
    RadzenScheduler<TaskDto> scheduler;
    private ICollection<TaskDto> _list = new List<TaskDto>();

    private DateTime? calendarStartTime = null;
    private DateTime? calendarEndTime = null;
    
    void OnSlotRender(SchedulerSlotRenderEventArgs args)
    {
        // Highlight today in month view
        // if (args.View.Text == "Month" && args.Start.Date == DateTime.Today)
        // {
        //     args.Attributes["style"] = "background: rgba(255,220,40,.2);";
        // }
        //
        // // Highlight working hours (9-18)
        // if ((args.View.Text == "Week" || args.View.Text == "Day") && args.Start.Hour > 8 && args.Start.Hour < 19)
        // {
        //     args.Attributes["style"] = "background: rgba(255,220,40,.2);";
        // }
    }

    async Task OnSlotSelect(SchedulerSlotSelectEventArgs args)
    {
        // if (data != null)
        // {
        //     // Either call the Reload method or reassign the Data property of the Scheduler
        //     await scheduler.Reload();
        // }
    }

    private async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<TaskDto> args)
    {
        await _modalDialogProviderService.ShowEditTaskModal(args.Data);
        // Modal closed
        await LoadItems();
        await scheduler.Reload();
    }

    void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<TaskDto> args)
    {
        // Never call StateHasChanged in AppointmentRender - would lead to infinite loop

        // if (args.Data.Text == "Birthday")
        // {
        //     args.Attributes["style"] = "background: red";
        // }
    }

    private async Task OnLoadData(SchedulerLoadDataEventArgs arg)
    {
        calendarStartTime = arg.Start;
        calendarEndTime = arg.End;
        await LoadItems();
    }
    
    private async Task LoadItems()
    {
        if (!calendarStartTime.HasValue || !calendarEndTime.HasValue)
        {
            return;
        }

        try
        {
            var result = await ApiService.TasksGetForCalendarAsync(new GetForCalendarRequest()
            {
                StartTime = calendarStartTime.Value,
                EndTime = calendarEndTime.Value,
                WorkspaceId = AuthState.Value.Workspace.Id
            });
            _list = result.Items;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            await ToastService.ShowError("Tasks receiving error");
        }
    }

    private void ShowTooltip(ElementReference elementReference, TaskDto task)
    {
        var toShow = "";
        if (task.TaskList.Project.Client != null)
        {
            toShow = "task.TaskList.Project.Client.Name / ";
        }

        toShow += $"{task.TaskList.Project.Name} / {task.TaskList.Name}";
        _tooltipService.Open(elementReference, toShow, new TooltipOptions() {});
    }
}
