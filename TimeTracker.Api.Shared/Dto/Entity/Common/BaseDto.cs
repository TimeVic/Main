using Api.Requests.Abstractions;

namespace TimeTracker.Api.Shared.Dto.Entity.Common;

public abstract class BaseDto: IResponse
{
    public Guid Id { get; set; }
}
