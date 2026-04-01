using Api.Requests.Abstractions;

namespace TimeTracker.Business.Common.Dto
{
    public class ListResponse<T>: List<T>, IResponse
    {
    
    }
}
