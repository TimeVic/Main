using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Web.Shared.Components;

public partial class TagsBadgesList
{
    [Parameter]
    public IEnumerable<TagDto> Tags { get; set; }
}
