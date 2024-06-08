using AutoMapper.Configuration;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.FileStorage.Mvc.Attribute;
using TimeTracker.Business.Helpers;

namespace TimeTracker.Tests.Unit.Business.Mvc.Validation.Attribute
{
    public class IsStorageBucketNameAttributeTest
    {
        private readonly IsStorageBucketNameAttribute _validationAttribute;

        public IsStorageBucketNameAttributeTest()
        {
            _validationAttribute =  new IsStorageBucketNameAttribute();
        }

        [Theory]
        [InlineData("aaaa")]
        [InlineData("abc-abc")]
        [InlineData("a-c")]
        [InlineData("abcdeabcdeabcdeabcdeabcdeab-cde")]
        [InlineData("abcd-eabcde-abcdeabc-deabcdeab-cde")]
        public void IsValid(string bucketName)
        {
            var isValid = _validationAttribute.IsValid(bucketName);
            Assert.True(isValid);
        }
        
        [Theory]
        [InlineData("akjs-")]
        [InlineData("-akjs-")]
        [InlineData("Abc")]
        [InlineData("abc@Abc")]
        [InlineData("-abc")]
        [InlineData("abcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcde")]
        public void IsInvalid(string bucketName)
        {
            var isValid = _validationAttribute.IsValid(bucketName);
            Assert.False(isValid);
        }
    }
}
