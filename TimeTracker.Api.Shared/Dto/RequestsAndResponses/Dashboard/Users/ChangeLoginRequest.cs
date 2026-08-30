using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;

public class ChangeLoginRequest : IRequest<UserDto>
{
    [Required]
    [IsLogin]
    public string Login { get; set; } = string.Empty;
}
