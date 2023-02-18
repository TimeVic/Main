using Bogus;
using TimeTracker.Business.Orm.Entities;

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
                .RuleFor(fake => fake.NotificationTime, fake => fake.Date.Future().ToUniversalTime())
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past().ToUniversalTime());
        }

        public TaskEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
