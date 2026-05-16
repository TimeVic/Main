using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;

public class UpdateSettingsRequest : IRequest<UserDto>
{
    [StringLength(100)]
    public string? UserName { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string LanguageCode { get; set; } = string.Empty;
}
