using TimeTracker.Api.Shared.Dto.Entity.Common;

namespace TimeTracker.Api.Shared.Dto.Entity.List;

public class LanguageDto : BaseDto
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}
