using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Project;
using TimeTracker.Web.Store.Project;

namespace TimeTracker.Web.Pages.Dashboard.Project.Parts.List;

public partial class UpdateProjectModal
{
    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public ProjectDto Project { get; set; }
    
    private UpdateRequest model = new();
    private bool _isLoading = false;
    private bool _isValid = false;
    private MudForm _form;

    private long _projectId
    {
        get => model.ProjectId;
        set => model.ProjectId = value;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        model.Fill(Project);
    }

    private async Task Submit()
    {
        _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }

        _isLoading = true;
        try
        {
            var responseDto = await ApiService.ProjectUpdateAsync(model);
            if (responseDto != null)
            {
                Dispatcher.Dispatch(new SetListItemAction(responseDto));
                await ToastService.ShowInfo("Project was updated");
                OnCloseModal();
            }
        }
        catch (Exception e)
        {
            await ToastService.ShowError(e.Message);
        }
        finally
        {
            _isLoading = false;
        }
        StateHasChanged();    
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
