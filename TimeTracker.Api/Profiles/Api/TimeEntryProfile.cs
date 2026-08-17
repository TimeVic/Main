using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.Task;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class TimeEntryProfile : Profile
{
    public TimeEntryProfile()
    {
        CreateMap<TimeEntryEntity, TimeEntryDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                var project = mapper.Mapper.Map<ProjectDto>(src.Project);
                var user = mapper.Mapper.Map<UserDto>(src.User);
                var task = mapper.Mapper.Map<TaskDto>(src.Task);
                return new TimeEntryDto
                {
                    Id = src.Id,
                    Description = src.Description,
                    HourlyRate = src.HourlyRate,
                    IsBillable = src.IsBillable,
                    IsAutostopped = src.IsAutostopped,
                    IsSynced = src.IsSynced,
                    CreatedAt = src.CreatedAt,
                    Project = project,
                    User = user,
                    Task = task,
                    StartTime = src.StartTime,
                    EndTime = src.EndTime,
                    TimeZone = src.TimeZone
                };
            });
    }
}
