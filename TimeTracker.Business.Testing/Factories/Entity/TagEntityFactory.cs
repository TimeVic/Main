using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class TagEntityFactory : IDataFactory<TagEntity>
    {
        private readonly Faker<TagEntity> _factory;

        public TagEntityFactory()
        {
            _factory = new Faker<TagEntity>()
                .RuleFor(fake => fake.Name, fake => fake.Random.String2(30))
                .RuleFor(fake => fake.CreatedAt, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdatedAt, fake => fake.Date.Past());
        }

        public TagEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
