using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.GoalsTracker;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.GoalsTracker;

namespace TimeTracker.Api.Profiles;

public class GoalsTrackerProfile : Profile
{
    public GoalsTrackerProfile()
    {
        CreateMap<GoalsTrackerEntity, GoalsTrackerDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {

                var items = mapper.Mapper.Map<List<GoalsTrackerItemDto>>(src.ActiveItems.ToArray());
                var notes = mapper.Mapper.Map<List<GoalsTrackerNoteDto>>(src.Notes.ToArray());
                return new GoalsTrackerDto
                {
                    Id = src.Id,
                    Year = src.Year,
                    Month = src.Month,
                    Items = items,
                    Notes = notes,
                };
            });
        CreateMap<GoalsTrackerItemEntity, GoalsTrackerItemDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                var markers = mapper.Mapper.Map<List<GoalsTrackerCompletionMarkerDto>>(src.CompletionMarkers.ToArray());
                return new GoalsTrackerItemDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    NumberOfTimes = src.NumberOfTimes,
                    Position = src.Position,
                    CompletionMarkers = markers 
                };
            });
        CreateMap<GoalsTrackerCompletionMarkerEntity, GoalsTrackerCompletionMarkerDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new GoalsTrackerCompletionMarkerDto
            {
                Id = src.Id,
                DayOfMonth = src.DayOfMonth,
                IsChecked = src.IsChecked
            });
        CreateMap<GoalsTrackerNoteEntity, GoalsTrackerNoteDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new GoalsTrackerNoteDto
            {
                Id = src.Id,
                Text = src.Text
            });
    }
}
