using TimeTracker.Business.Orm.Core;

namespace TimeTracker.Business.Orm.Entities.GoalsTracker
{
    public class GoalsTrackerItemEntity: AEntity
    {
        public virtual required string Name { get; set; }
        public virtual int NumberOfTimes { get; set; }
        public virtual int Position { get; set; } = 0;
        public virtual bool IsArchived { get; set; } = false;
        public virtual required GoalsTrackerEntity Tracker { get; set; }
        
        public virtual ICollection<GoalsTrackerCompletionMarkerEntity> CompletionMarkers { get; set; } = new List<GoalsTrackerCompletionMarkerEntity>();
        
        // public virtual void SetClient(ClientEntity? client)
        // {
        //     if (Client?.Id == client?.Id)
        //     {
        //         return;
        //     }
        //
        //     Client = client;
        //     client?.Projects.Add(this);
        // }
    }
}
