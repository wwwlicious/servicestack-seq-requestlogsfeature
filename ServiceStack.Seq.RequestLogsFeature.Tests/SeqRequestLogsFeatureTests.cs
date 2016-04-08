// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System;

    using FluentAssertions;

    using ServiceStack.FluentValidation;

    using Xunit;

    public class SeqRequestLogsFeatureTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("  ")]
        [InlineData("notaurl")]
        public void InvalidUrlThrowsException(string url)
        {
            Action action = () => new SeqRequestLogsSettings(url);
            action.ShouldThrow<ValidationException>();
        }

        [Fact]
        public void ValidUrlDoesNotThrowExceptio()
        {
            new SeqRequestLogsFeature(new SeqRequestLogsSettings("http://localhost"));
        }

        [Fact]
        public void ConfigChanges()
        {
            var request = new SeqRequestLogConfig
                             {
                                 Enabled = false,
                                 EnableRequestBodyTracking = false,
                                 EnableErrorTracking = false,
                                 EnableSessionTracking = false,
                                 EnableResponseTracking = false
                             };
            var client = new JsonServiceClient("http://myservice");
            client.Send(request);
        }
    }
}