using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class TaskListEntityFactory : IDataFactory<TaskListEntity>
    {
        private readonly Faker<TaskListEntity> _factory;

        public TaskListEntityFactory()
        {
            _factory = new Faker<TaskListEntity>()
                .RuleFor(fake => fake.Name, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past());
        }

        public TaskListEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
