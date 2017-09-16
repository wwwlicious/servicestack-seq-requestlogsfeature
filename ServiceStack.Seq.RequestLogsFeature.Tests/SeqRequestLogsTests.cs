// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;

    using ServiceStack.FluentValidation;

    using Xunit;

    [Collection("AppHost")]
    public class SeqRequestLogsTests : IClassFixture<SeqRequestLogAppHost>
    {
        private readonly SeqRequestLogAppHost host;

        public SeqRequestLogsTests(SeqRequestLogAppHost host)
        {
            this.host = host;
        }

        [Fact]
        public void ConvertEmptyEventToJson()
        {
            var entry = new SeqRequestLogEntry();
            var json = entry.ToJson();
            json.Should()
                .Be(
                    "{\"Level\":\"Debug\",\"Properties\":{},\"MessageTemplate\":\"Servicestack SeqRequestLogsFeature\"}");
        }

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
        public void ValidUrlDoesNotThrowException()
        {
            new SeqRequestLogsFeature(new SeqRequestLogsSettings("http://localhost:5341"));
        }

        [Fact]
        [Trait("Category", "Local")]
        public void GenerateLogData()
        {
            var client = host.GetClient();
            var names = new[] { "bob", "server", "spoon" };

            for (var i = 0; i < 1000; i++)
            {
                var index = 0;
                if (i % 20 == 0) index = 2;
                if (i % 100 == 0) index = 1;

                var request = new Hello(names[index]);
                client.SendAsync(request);
            }
        }
    }

    [Collection("AppHost")]
    public class SeqRequestCustomLogsTests : IClassFixture<SeqRequestLogAppHost>
    {
        private readonly SeqRequestLogAppHost host;

        public SeqRequestCustomLogsTests(SeqRequestLogAppHost host)
        {
            this.host = host;
        }

        [Fact]
        [Trait("Category", "Local")]
        public void CanCallLog()
        {
            var client = host.GetClient();
            var requestDto = new Hello("Phil");

            var response = client.Send(requestDto);

            response.Greeting.Should().Be("Hello Phil");

            var customLogs = host.CustomLogs.ToArray().Single(x => ((Hello)x.RequestDto).Name == requestDto.Name);
            customLogs.RequestDto.Should().BeOfType<Hello>().Which.Name.Should().Be("Phil");
            customLogs.Response.Should().NotBeNull();
        }
    }
}