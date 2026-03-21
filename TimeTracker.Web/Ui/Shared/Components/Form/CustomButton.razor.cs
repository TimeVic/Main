using Microsoft.AspNetCore.Components;

namespace TimeTracker.Web.Ui.Shared.Components.Form;

public partial class CustomButton
{
    [Parameter]
    public bool IsLoading { get; set; } = false;
}
