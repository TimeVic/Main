using Bogus;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class TimeEntryEntityFactory : IDataFactory<TimeEntryEntity>
    {
        private readonly Faker<TimeEntryEntity> _factory;

        public TimeEntryEntityFactory()
        {
            _factory = new Faker<TimeEntryEntity>()
                .RuleFor(fake => fake.TaskId, fake => fake.Random.String2(6))
                .RuleFor(fake => fake.Description, fake => fake.Lorem.Lines(2))
                .RuleFor(fake => fake.HourlyRate, fake => fake.Random.Decimal(0, 3))
                .RuleFor(fake => fake.IsBillable, fake => true)
                .RuleFor(fake => fake.StartTime, fake => DateTime.UtcNow.AddHours(-2).TimeOfDay)
                .RuleFor(fake => fake.EndTime, fake => DateTime.UtcNow.TimeOfDay)
                .RuleFor(fake => fake.Date, fake => DateTime.UtcNow.StartOfDay())
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past());
        }

        public TimeEntryEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
