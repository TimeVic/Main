using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Requests.Abstractions;
using AutoMapper;
using Persistence.Transactions.Behaviors;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Orm.Dao.User;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions;

public class SearchRequestHandler : IAsyncRequestHandler<SearchRequest, SearchResponse>
{
    private readonly IUserDao _userDao;
    private readonly IMapper _mapper;
    private readonly IDbSessionProvider _sessionProvider;

    public SearchRequestHandler(
        IUserDao userDao,
        IMapper mapper,
        IDbSessionProvider sessionProvider
    )
    {
        _userDao = userDao;
        _mapper = mapper;
        _sessionProvider = sessionProvider;
    }

    public async Task<SearchResponse> ExecuteAsync(SearchRequest request)
    {
        var users = await _userDao.FindByLogin(request.Query, request.Take);
        return new SearchResponse(
            _mapper.Map<ICollection<UserDto>>(users),
            users.Count
        );
    }
}
