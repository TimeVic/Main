using Api.Requests.Abstractions;
using TimeTracker.Api.Services.Users;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions;

public class UpdateSettingsRequestHandler : IAsyncRequestHandler<UpdateSettingsRequest, UserDto>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IUserDao _userDao;
    private readonly ILanguageDao _languageDao;
    private readonly IUserDtoBuilder _userDtoBuilder;

    public UpdateSettingsRequestHandler(
        IApiRequestService apiRequestService,
        IUserDao userDao,
        ILanguageDao languageDao,
        IUserDtoBuilder userDtoBuilder
    )
    {
        _apiRequestService = apiRequestService;
        _userDao = userDao;
        _languageDao = languageDao;
        _userDtoBuilder = userDtoBuilder;
    }

    public async Task<UserDto> ExecuteAsync(UpdateSettingsRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var language = await _languageDao.GetByCodeAsync(request.LanguageCode);
        RecordNotFoundException.ThrowIfNull(language);

        user = await _userDao.UpdateSettingsAsync(user, request.UserName, language);
        return await _userDtoBuilder.BuildAsync(user);
    }
}
