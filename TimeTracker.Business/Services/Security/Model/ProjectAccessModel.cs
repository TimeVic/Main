using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Services.Security.Model;

public class ProjectAccessModel
{
    public ProjectEntity Project { get; set; }

    public decimal? HourlyRate { get; set; } = null;
}
