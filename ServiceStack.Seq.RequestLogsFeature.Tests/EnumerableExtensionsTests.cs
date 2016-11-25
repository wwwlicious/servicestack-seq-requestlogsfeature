namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System.Collections.Generic;
    using FluentAssertions;
    using Xunit;

    public class EnumerableExtensionsTests
    {
        [Fact]
        public void IsNullOrEmpty_True_IfNull()
        {
            List<string> testList = null;
            testList.IsNullOrEmpty().Should().BeTrue();
        }

        [Fact]
        public void IsNullOrEmpty_True_IfEmpty()
        {
            new List<int>().IsNullOrEmpty().Should().BeTrue();
        }

        [Fact]
        public void IsNullOrEmpty_False_HasValue()
        {
            var testList = new List<string> { "foo", "bar" };
            testList.IsNullOrEmpty().Should().BeFalse();
        }
    }
}
