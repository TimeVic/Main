using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;

public class CheckLoginRequest : IRequest<CheckLoginResponse>
{
    [Required]
    [IsLogin]
    public string Login { get; set; } = string.Empty;
}
