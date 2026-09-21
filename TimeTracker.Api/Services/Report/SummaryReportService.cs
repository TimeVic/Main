using AutoMapper;
using TimeTracker.Api.Shared.Dto.Model.Report;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Report;
using TimeTracker.Business.Common.Constants.Reports;
using TimeTracker.Business.Orm.Dao.Report;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.Services.Report;

public class SummaryReportService : ISummaryReportService
{
    private readonly IMapper _mapper;
    private readonly ISummaryReportDao _summaryReportDao;

    public SummaryReportService(
        IMapper mapper,
        ISummaryReportDao summaryReportDao
    )
    {
        _mapper = mapper;
        _summaryReportDao = summaryReportDao;
    }

    public async Task<SummaryReportResponse> GetReportAsync(
        UserEntity currentUser,
        Guid workspaceId,
        DateTime startTime,
        DateTime endTime,
        SummaryReportType type
    )
    {
        var byDaysReportItems = await _summaryReportDao.GetReportByDayAsync(
            workspaceId,
            currentUser.Id,
            startTime,
            endTime
        );
        var response = new SummaryReportResponse
        {
            ByDays = _mapper.Map<ICollection<SummaryByDaysReportItemDto>>(byDaysReportItems)
        };
        if (type == SummaryReportType.GroupByDay)
        {
            var groupedItems = await _summaryReportDao.GetReportByDayAsync(
                workspaceId,
                currentUser.Id,
                startTime,
                endTime
            );
            response.GroupedByDay = _mapper.Map<ICollection<SummaryByDaysReportItemDto>>(groupedItems);
        }
        else if (type == SummaryReportType.GroupByClient)
        {
            var groupedItems = await _summaryReportDao.GetReportByClientAsync(
                workspaceId,
                currentUser.Id,
                startTime,
                endTime
            );
            response.GroupedByClient = _mapper.Map<ICollection<SummaryByClientsReportItemDto>>(groupedItems);
        }
        else if (type == SummaryReportType.GroupByProject)
        {
            var groupedItems = await _summaryReportDao.GetReportByProjectAsync(
                workspaceId,
                currentUser.Id,
                startTime,
                endTime
            );
            response.GroupedByProject = _mapper.Map<ICollection<SummaryByProjectsReportItemDto>>(groupedItems);
        }
        else if (type == SummaryReportType.GroupByMonth)
        {
            var groupedItems = await _summaryReportDao.GetReportByMonthAsync(
                workspaceId,
                currentUser.Id,
                startTime,
                endTime
            );
            response.GroupedByMonth = _mapper.Map<ICollection<SummaryByMonthsReportItemDto>>(groupedItems);
        }
        else if (type == SummaryReportType.GroupByWeek)
        {
            var groupedItems = await _summaryReportDao.GetReportByWeekAsync(
                workspaceId,
                currentUser.Id,
                startTime,
                endTime
            );
            response.GroupedByWeek = _mapper.Map<ICollection<SummaryByWeeksReportItemDto>>(groupedItems);
        }
        return response;
    }
}
