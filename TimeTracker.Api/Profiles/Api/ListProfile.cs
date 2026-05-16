using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class ListProfile : Profile
{
    public ListProfile()
    {
        CreateMap<CurrencyEntity, CurrencyDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                return new CurrencyDto
                {
                    Id = src.Id,
                    Code = src.Code,
                    Symbol = src.Symbol,
                };
            });

        CreateMap<LanguageEntity, LanguageDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                return new LanguageDto
                {
                    Id = src.Id,
                    Name = src.Name,
                    Code = src.Code,
                };
            });
    }
}
