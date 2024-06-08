using TimeTracker.Api.Shared.Constants;
using TimeTracker.Api.Shared.Dto;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.TimeEntry;
using TimeTracker.Web.Core.Exceptions;
using TimeTracker.Web.Core.Helpers;

namespace TimeTracker.Web.Services.Http
{
    public partial class ApiService
    {
        public async Task<TimeEntryDto> TimeEntryStartAsync(StartRequest model)
        {
            var response = await PostAsync<TimeEntryDto>(ApiUrl.TimeEntryStart, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task TimeEntryStopAsync(StopRequest model)
        {
            await PostAsync<TimeEntryDto>(ApiUrl.TimeEntryStop, model);
        }
        
        public async Task<TimeEntryDto> TimeEntrySetAsync(SetRequest model)
        {
            var response = await PostAsync<TimeEntryDto>(ApiUrl.TimeEntrySet, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GetListResponse> TimeEntryGetListAsync(GetListRequest model)
        {
            var response = await PostAsync<GetListResponse>(ApiUrl.TimeEntryGetList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task<GetFilteredListResponse> TimeEntryGetFilteredListAsync(GetFilteredListRequest model)
        {
            var response = await PostAsync<GetFilteredListResponse>(ApiUrl.TimeEntryGetFilteredList, model);
            if (response == null)
            {
                throw new ServerErrorException();
            }

            return response;
        }
        
        public async Task TimeEntryDeleteAsync(long id)
        {
            await PostAsync<TimeEntryDto>(ApiUrl.TimeEntryDelete, new DeleteRequest()
            {
                TimeEntryId = id
            });
        }
    }
}
