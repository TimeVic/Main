using Bogus;
using TimeTracker.Business.Orm.Entities.GoalsTracker;

namespace TimeTracker.Business.Testing.Factories.Entity.GoalsTracker
{
    internal class GoalsTrackerItemEntityFactory : IDataFactory<GoalsTrackerItemEntity>
    {
        private readonly Faker<GoalsTrackerItemEntity> _factory;

        public GoalsTrackerItemEntityFactory()
        {
            _factory = new Faker<GoalsTrackerItemEntity>()
                .RuleFor(fake => fake.Name, fake => fake.Name.JobTitle())
                .RuleFor(fake => fake.NumberOfTimes, fake => fake.Random.Number(50))
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past());
        }

        public GoalsTrackerItemEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
