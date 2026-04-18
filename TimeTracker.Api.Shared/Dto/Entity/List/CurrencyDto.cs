using TimeTracker.Api.Shared.Dto.Entity.Common;

namespace TimeTracker.Api.Shared.Dto.Entity.List;

public class CurrencyDto: BaseDto
{   
    public string Code { get; set; } = string.Empty;
    
    public string Symbol { get; set; } = string.Empty;
}
