using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.GoalsTracker;

namespace TimeTracker.Api.Profiles;

public class GoalsTrackerProfile : Profile
{
    public GoalsTrackerProfile()
    {
        CreateMap<GoalsTrackerEntity, GoalsTrackerDto>();
        CreateMap<GoalsTrackerItemEntity, GoalsTrackerItemDto>();
        CreateMap<GoalsTrackerNoteEntity, GoalsTrackerNoteDto>();
    }
}
