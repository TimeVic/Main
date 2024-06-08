using Bogus;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.FileStorage;

namespace TimeTracker.Business.Testing.Factories.Entity.FileStorage
{
    internal class FileStorageBucketEntityFactory : IDataFactory<FileStorageBucketEntity>
    {
        private readonly Faker<FileStorageBucketEntity> _factory;

        public FileStorageBucketEntityFactory()
        {
            _factory = new Faker<FileStorageBucketEntity>()
                .RuleFor(fake => fake.Name, fake => fake.Random.String2(30))
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past());
        }

        public FileStorageBucketEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
