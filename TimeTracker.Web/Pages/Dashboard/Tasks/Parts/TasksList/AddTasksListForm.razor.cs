﻿using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks.List;
using TimeTracker.Web.Constants;
using TimeTracker.Web.Store.Project;
using LoadListAction = TimeTracker.Web.Store.TasksList.LoadListAction;

namespace TimeTracker.Web.Pages.Dashboard.Tasks.Parts.TasksList;

public partial class AddTasksListForm
{
    [Inject]
    public IState<ProjectState> ProjectState { get; set; }
    
    private AddRequest model = new();
    private bool _isLoading = false;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
    
    private async Task HandleSubmit()
    {
        _isLoading = true;
        try
        {
            var membershipDto = await ApiService.TaskListAddAsync(model);
            if (membershipDto != null)
            {
                Dispatcher.Dispatch(new LoadListAction(true));
                NotificationService.Notify(new NotificationMessage()
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Task list has been added"
                });
                DialogService.CloseSide();

                var navigateToProjectsClient = ProjectState.Value.List.FirstOrDefault(
                    item => item.Id == model.ProjectId
                );
                NavigationManager.NavigateTo(
                    string.Format(SiteUrl.Dashboard_Tasks, navigateToProjectsClient?.Client?.Id.ToString() ?? "")    
                );
            }
        }
        catch (Exception)
        {
            NotificationService.Notify(new NotificationMessage()
            {
                Severity = NotificationSeverity.Error,
                Summary = "Task list adding error"
            });
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();
    }
    
    private void CloseModal()
    {
        DialogService.CloseSide();
    }
}