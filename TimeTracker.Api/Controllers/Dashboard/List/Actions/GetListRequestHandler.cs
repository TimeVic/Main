using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.List.Currency;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Orm.Dao;

namespace TimeTracker.Api.Controllers.Dashboard.List.Actions
{
    public class GetListRequestHandler : IAsyncRequestHandler<GetListRequest, ListResponse<CurrencyDto>>
    {
        private readonly IMapper _mapper;
        private readonly ICurrencyDao _currencyDao;

        public GetListRequestHandler(
            IMapper mapper,
            ICurrencyDao currencyDao
        )
        {
            _mapper = mapper;
            _currencyDao = currencyDao;
        }
    
        public async Task<ListResponse<CurrencyDto>> ExecuteAsync(GetListRequest request)
        {
            var listDto = await _currencyDao.GetAll();
            return _mapper.Map<ListResponse<CurrencyDto>>(listDto);
        }
    }
}
