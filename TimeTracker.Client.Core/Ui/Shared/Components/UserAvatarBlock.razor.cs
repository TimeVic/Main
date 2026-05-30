using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Client.Core.Services.UI;

namespace TimeTracker.Client.Core.Ui.Shared.Components;

public partial class UserAvatarBlock
{
    [Parameter]
    public UserDto User { get; set; } = null!;

    [Parameter]
    public string Class { get; set; } = string.Empty;

    [Parameter]
    public string SizeClass { get; set; } = "h-9 w-9";

    [Parameter]
    public StorageImageSize ImageSize { get; set; } = StorageImageSize.Xs_128;

    [Inject]
    private UrlService UrlService { get; set; } = null!;

    private string Initials => string.IsNullOrWhiteSpace(User?.Initials) ? "?" : User.Initials;

    private string TitleText => User?.Name ?? string.Empty;

    private string AltText => TitleText;

    private string AvatarKey => User?.Avatar?.Id.ToString() ?? $"avatar-empty-{User?.Id}";

    private string? AvatarUrl => User?.Avatar == null
        ? null
        : UrlService.GetStorageImageUrl(User.Avatar, ImageSize);

    private string AvatarClass => string.Join(
        " ",
        new[]
        {
            "inline-flex shrink-0 items-center justify-center rounded-full bg-slate-900 object-cover text-xs font-semibold uppercase text-white",
            SizeClass,
            Class
        }.Where(item => !string.IsNullOrWhiteSpace(item))
    );
}
