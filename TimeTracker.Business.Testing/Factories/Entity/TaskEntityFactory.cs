using Bogus;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Entities;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class TaskEntityFactory : IDataFactory<TaskEntity>
    {
        private readonly Faker<TaskEntity> _factory;

        public TaskEntityFactory()
        {
            _factory = new Faker<TaskEntity>()
                .RuleFor(fake => fake.Title, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.ExternalTaskId, fake => fake.Random.String2(30))
                .RuleFor(fake => fake.Description, fake => fake.Lorem.Sentence())
                .RuleFor(fake => fake.IsArchived, fake => false)
                .RuleFor(fake => fake.Status, fake => TaskStatus.Backlog)
                .RuleFor(fake => fake.Priority, fake => TaskPriority.Medium)
                .RuleFor(fake => fake.StartTime, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.EndTime, fake => fake.Date.Future().ToUniversalTime())
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past().ToUniversalTime());
        }

        public TaskEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
