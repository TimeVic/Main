using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class IntegrationClickUpEntityFactory : IDataFactory<WorkspaceSettingsClickUpEntity>
    {
        private readonly Faker<WorkspaceSettingsClickUpEntity> _factory;

        public IntegrationClickUpEntityFactory()
        {
            _factory = new Faker<WorkspaceSettingsClickUpEntity>()
                .RuleFor(fake => fake.SecurityKey, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.TeamId, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past());
        }

        public WorkspaceSettingsClickUpEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
