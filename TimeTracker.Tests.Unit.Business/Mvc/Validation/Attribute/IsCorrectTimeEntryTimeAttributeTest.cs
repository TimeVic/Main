using AutoMapper.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Mvc.Validation.Attribute
{
    public class IsCorrectTimeEntryTimeAttributeTest
    {
        private readonly IsCorrectTimeEntryTimeAttribute _validationAttribute;

        public IsCorrectTimeEntryTimeAttributeTest()
        {
            _validationAttribute =  new IsCorrectTimeEntryTimeAttribute();
        }

        [Fact]
        public void IsValid1()
        {
            Assert.True(_validationAttribute.IsValid(TimeSpan.FromHours(1)));
        }
        
        [Fact]
        public void IsValid2()
        {
            Assert.True(_validationAttribute.IsValid(GlobalConstants.EndOfDay - TimeSpan.FromSeconds(1)));
        }
        
        [Fact]
        public void IsInvalid()
        {
            Assert.False(_validationAttribute.IsValid(TimeSpan.FromDays(1)));
        }
        
        [Fact]
        public void IsInvalid2()
        {
            Assert.False(_validationAttribute.IsValid(TimeSpan.FromDays(2)));
        }
    }
}
