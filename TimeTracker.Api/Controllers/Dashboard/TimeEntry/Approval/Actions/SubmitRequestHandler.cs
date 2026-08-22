using Api.Requests.Abstractions;
using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry.Approval;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Services.Entity;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Dashboard.TimeEntry.Approval.Actions;

public class SubmitRequestHandler : IAsyncRequestHandler<SubmitRequest, TimeEntryDto>
{
    private readonly IMapper _mapper;
    private readonly IApiRequestService _apiRequestService;
    private readonly ITimeEntryDao _timeEntryDao;
    private readonly ITimeEntryApprovalService _approvalService;

    public SubmitRequestHandler(
        IMapper mapper,
        IApiRequestService apiRequestService,
        ITimeEntryDao timeEntryDao,
        ITimeEntryApprovalService approvalService
    )
    {
        _mapper = mapper;
        _apiRequestService = apiRequestService;
        _timeEntryDao = timeEntryDao;
        _approvalService = approvalService;
    }

    public async Task<TimeEntryDto> ExecuteAsync(SubmitRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        var timeEntry = await _timeEntryDao.GetByIdAsync(request.TimeEntryId);
        RecordNotFoundException.ThrowIfNull(timeEntry, "Time entry not found");

        var updatedEntry = await _approvalService.SubmitAsync(user, timeEntry);
        return _mapper.Map<TimeEntryDto>(updatedEntry);
    }
}
