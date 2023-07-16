using Bogus;
using TimeTracker.Business.Common.Constants.Task;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TaskStatus = TimeTracker.Business.Common.Constants.Task.TaskStatus;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class TaskCommentEntityFactory : IDataFactory<TaskCommentEntity>
    {
        private readonly Faker<TaskCommentEntity> _factory;

        public TaskCommentEntityFactory()
        {
            _factory = new Faker<TaskCommentEntity>()
                .RuleFor(fake => fake.Comment, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.IsArchived, fake => false)
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past().ToUniversalTime());
        }

        public TaskCommentEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
