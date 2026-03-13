using Bogus;
using TimeTracker.Business.Orm.Entities.GoalsTracker;

namespace TimeTracker.Business.Testing.Factories.Entity.GoalsTracker
{
    internal class GoalsTrackerEntityFactory : IDataFactory<GoalsTrackerEntity>
    {
        private readonly Faker<GoalsTrackerEntity> _factory;

        public GoalsTrackerEntityFactory()
        {
            _factory = new Faker<GoalsTrackerEntity>()
                .RuleFor(fake => fake.Year, fake => DateTime.UtcNow.Year)
                .RuleFor(fake => fake.Month, fake => DateTime.UtcNow.Month)
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past());
        }

        public GoalsTrackerEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
