using Microsoft.AspNetCore.Components;
using MudBlazor;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;

namespace TimeTracker.Web.Pages.Dashboard.GoalsTracker.Parts;

public partial class ChangePositionModal
{
    [Parameter]
    public GoalsTrackerDto Tracker { get; set; }
    
    [CascadingParameter] 
    MudDialogInstance MudDialog { get; set; }

    private bool _isLoading = false;

    private void Submit()
    {
        
    }

    private void OnCloseModal()
    {
        MudDialog.Close();
    }
}
