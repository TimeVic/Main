using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Extensions;
using TimeTracker.Business.Orm.Mapping.Common;

namespace TimeTracker.Business.Orm.Mapping.Entities;

public class QueueMapping: BaseGuidMappings<QueueEntity>
{
    public QueueMapping()
    {
        Table("queues");
        
        Map(x => x.Status).Enum<QueueStatus>();
        Map(x => x.Channel).Enum<QueueChannel>();
        Map(x => x.Priority).Enum<QueuePriority>();
        Map(x => x.Error);
        Map(x => x.ContextType);
        Map(x => x.ContextData);
        
        Map(x => x.ProcessAt).DateTime();
        Map(x => x.CreatedAt).DateTime();
        Map(x => x.UpdatedAt).DateTime();
    }
}
