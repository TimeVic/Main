using Bogus;
using TimeTracker.Business.Orm.Entities;

namespace TimeTracker.Business.Testing.Factories.Entity
{
    internal class PaymentEntityFactory : IDataFactory<PaymentEntity>
    {
        private readonly Faker<PaymentEntity> _factory;

        public PaymentEntityFactory()
        {
            _factory = new Faker<PaymentEntity>()
                .RuleFor(fake => fake.PaymentTime, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.Amount, fake => fake.Random.Decimal(1, 200))
                .RuleFor(fake => fake.Description, fake => fake.Lorem.Sentence(3))
                .RuleFor(fake => fake.CreateTime, fake => fake.Date.Past().ToUniversalTime())
                .RuleFor(fake => fake.UpdateTime, fake => fake.Date.Past().ToUniversalTime());
        }

        public PaymentEntity Generate()
        {
            return _factory.Generate();
        }
    }
}
