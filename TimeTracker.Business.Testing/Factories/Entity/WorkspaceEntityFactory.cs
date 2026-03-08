using Bogus;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class WorkspaceEntityFactory : IDataFactory<WorkspaceEntity>
    {
        private readonly Faker<WorkspaceEntity> _factory;

        public WorkspaceEntityFactory()
        {
            _factory = new Faker<WorkspaceEntity>()
                .RuleFor(fake => fake.Name, fake => fake.Random.String2(100))
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past());
        }

        public WorkspaceEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
