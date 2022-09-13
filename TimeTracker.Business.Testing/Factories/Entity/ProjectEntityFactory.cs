using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class ProjectEntityFactory : IDataFactory<ProjectEntity>
    {
        private readonly Faker<ProjectEntity> _factory;

        public ProjectEntityFactory()
        {
            _factory = new Faker<ProjectEntity>()
                .RuleFor(fake => fake.Name, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.IsArchived, fake => false)
                .RuleFor(fake => fake.IsBillableByDefault, fake => true)
                .RuleFor(fake => fake.DefaultHourlyRate, fake => fake.Random.Decimal(1, 30))
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past());
        }

        public ProjectEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
