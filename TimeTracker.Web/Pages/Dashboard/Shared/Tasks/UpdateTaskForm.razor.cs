﻿using Fluxor;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Tasks;
using TimeTracker.Web.Services.Security;
using TimeTracker.Web.Store.WorkspaceMemberships;
using SetListItemAction = TimeTracker.Web.Store.Tasks.SetListItemAction;

namespace TimeTracker.Web.Pages.Dashboard.Shared.Tasks;

public partial class UpdateTaskForm
{
    [Parameter]
    public TaskDto Task { get; set; }
    
    [Inject]
    public IState<TimeTracker.Web.Store.TasksList.TasksListState> _tasksListState { get; set; }

    [Inject] 
    private ISecurityManager _securityManager { get; set; }

    [Inject]
    private IState<WorkspaceMembershipsState> _workspaceMembershipsState { get; set; }

    private RadzenTemplateForm<UpdateRequest> _form;

    private UpdateRequest model = new();
    private bool _isLoading = false;
    
    private readonly int _descriptionTextAreaRowsMin = 10;
    private readonly int _descriptionTextAreaRowsMax = 20;
    private int _descriptionTextAreaRows = 6;

    private string _tabLabelAttachments
    {
        get
        {
            var label = "Attachments";
            if (Task.Attachments.Any())
            {
                label += $"({Task.Attachments.Count})";
            }

            return label;
        }
    }

    private ICollection<long> _allowedUserIds
    {
        get
        {
            return _securityManager.GetMembersWhichHaveAccessToProject(Task.TaskList.Project)
                .Select(item => item.Id)
                .ToList();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(Task);
        ResizeDescriptionField(model.Description);
    }

    private void HandleSubmit(UpdateRequest request)
    {
        InvokeAsync(async () =>
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
        });
    }

    private Task SubmitForm()
    {
        if (_form.IsValid)
        {
            InvokeAsync(async () => await _form.Submit.InvokeAsync());
        }
        return System.Threading.Tasks.Task.CompletedTask;
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

    private async Task OnChangedAssigned(WorkspaceMembershipDto membership)
    {
        model.UserId = membership.User.Id;
        await SubmitForm();
    }

    private async Task OnChangeNotificationTime(DateTime? notificationTime)
    {
        model.NotificationTime = notificationTime;
        await SubmitForm();
    }

    private void OnRenderNotificationTime(DateRenderEventArgs renderEvent)
    {
        renderEvent.Disabled = renderEvent.Disabled || renderEvent.Date < DateTime.Now;
    }

    private void OnFileUploaded(StoredFileDto uploadedFile)
    {
        Task.Attachments.Add(uploadedFile);
        Dispatcher.Dispatch(new SetListItemAction(Task));
    }

    private async Task OnTagsChanged(ICollection<long> selectedTagIds)
    {
        model.TagIds = selectedTagIds;
        await SubmitForm();
    }

    private void AttachmentsListUpdated(ICollection<StoredFileDto> attachments)
    {
        Task.Attachments = attachments;
        Dispatcher.Dispatch(new SetListItemAction(Task));
    }
}
