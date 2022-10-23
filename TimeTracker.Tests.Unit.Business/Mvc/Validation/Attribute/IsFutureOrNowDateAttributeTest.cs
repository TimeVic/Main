using AutoMapper.Configuration;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Mvc.Validation.Attribute
{
    public class IsFutureOrNowDateAttributeTest
    {
        private readonly IsFutureOrNowDateAttribute _validationAttribute;

        public IsFutureOrNowDateAttributeTest()
        {
            _validationAttribute =  new IsFutureOrNowDateAttribute();
        }

        [Fact]
        public void IsValid1()
        {
            Assert.True(_validationAttribute.IsValid(DateTime.Now));
        }
        
        [Fact]
        public void IsValid2()
        {
            Assert.True(_validationAttribute.IsValid(DateTime.Now.AddDays(1)));
        }
        
        [Fact]
        public void IsInvalid()
        {
            Assert.False(_validationAttribute.IsValid(DateTime.Now.AddDays(-1)));
        }
        
        [Fact]
        public void IsInvalid2()
        {
            Assert.False(_validationAttribute.IsValid(DateTime.Now.StartOfDay().AddMinutes(-1)));
        }
    }
}
