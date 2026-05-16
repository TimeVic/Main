using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Api.Profiles.Api;

public class StorageProfile : Profile
{
    public StorageProfile()
    {
        CreateMap<StoredFileEntity, StoredFileDto>()
            .IgnoreAllAndConstructUsing((src, mapper) => new StoredFileDto
            {
                Id = src.Id,
                Type = src.Type,
                Extension = src.Extension,
                MimeType = src.MimeType,
                OriginalFileName = src.OriginalFileName,
                Title = src.Title,
                Description = src.Description,
                Url = src.Url
            });
    }
}
