using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace TimeTracker.Web.Ui.Shared.Components.Form;

public partial class CustomButton
{
    [Parameter]
    public bool IsLoading { get; set; } = false;
}
