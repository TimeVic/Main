using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
namespace TimeTracker.Client.Core.Services.Http
{
    public partial class ApiService
    {
        public async Task<TimeEntryDto?> TimeEntryStartAsync(StartRequest model)
        {
            return await PostAsync<TimeEntryDto>(ApiUrl.TimeEntryStart, model);
        }

        public async Task<TimeEntryDto?> TimeEntryStopAsync(StopRequest model)
        {
            return await PostAsync<TimeEntryDto>(ApiUrl.TimeEntryStop, model);
        }

        public async Task<GetActiveResponse?> TimeEntryGetActiveAsync()
        {
            return await PostAsync<GetActiveResponse>(ApiUrl.TimeEntryGetActive, new GetActiveRequest());
        }
        
        public async Task<TimeEntryDto?> TimeEntrySetAsync(SetRequest model)
        {
            return await PostAsync<TimeEntryDto>(ApiUrl.TimeEntrySet, model);
        }
        
        public async Task<GetListResponse?> TimeEntryGetListAsync(GetListRequest model)
        {
            return await PostAsync<GetListResponse>(ApiUrl.TimeEntryGetList, model);
        }
        
        public async Task<GetFilteredListResponse?> TimeEntryGetFilteredListAsync(GetFilteredListRequest model)
        {
            return await PostAsync<GetFilteredListResponse>(ApiUrl.TimeEntryGetFilteredList, model);
        }
        
        public async Task TimeEntryDeleteAsync(Guid id)
        {
            await PostAsync<TimeEntryDto>(ApiUrl.TimeEntryDelete, new DeleteRequest()
            {
                TimeEntryId = id
            });
        }
    }
}
