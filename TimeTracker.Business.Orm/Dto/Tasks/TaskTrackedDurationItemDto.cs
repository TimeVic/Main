namespace TimeTracker.Business.Orm.Dto.Tasks;

public class TaskTrackedDurationItemDto
{
    public Guid TaskId { get; set; }

    public double TrackedSeconds { get; set; }
}
