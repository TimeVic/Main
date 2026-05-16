using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Api.Services.Users;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions;

public class GetCurrentRequestHandler : IAsyncRequestHandler<GetCurrentRequest, UserDto>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDtoBuilder _userDtoBuilder;

    public GetCurrentRequestHandler(
        IApiRequestService apiRequestService,
        IUserDtoBuilder userDtoBuilder
    )
    {
        _apiRequestService = apiRequestService;
        _userDtoBuilder = userDtoBuilder;
    }

    public async Task<UserDto> ExecuteAsync(GetCurrentRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        return await _userDtoBuilder.BuildAsync(user);
    }
}
