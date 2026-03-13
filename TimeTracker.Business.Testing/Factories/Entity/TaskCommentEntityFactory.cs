using Bogus;
using TimeTracker.Business.Orm.Entities.Tasks;

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
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past().ToUniversalTime());
        }

        public TaskCommentEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
