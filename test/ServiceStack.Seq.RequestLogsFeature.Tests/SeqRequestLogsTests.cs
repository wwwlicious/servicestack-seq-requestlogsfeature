﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;

    using FluentValidation;
    using ServiceStack.Configuration;
    using ServiceStack.Host;
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
                    "{\"level\":\"Debug\",\"properties\":{},\"messageTemplate\":\"Servicestack SeqRequestLogsFeature\"}");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("  ")]
        [InlineData("notaurl")]
        public void InvalidUrlThrowsException(string url)
        {
            Assert.Throws<ValidationException>(() => new SeqRequestLogsSettings(url));
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
        
        [Fact]
        public void SkipLogging_IgnoresRequests()
        {
            var logger = new SeqRequestLogger(new SeqRequestLogsFeature(new AppSettings())
            {
                SkipLogging = request => request.RawUrl == "/ignore",
            });

            var basicRequest = new BasicRequest { RawUrl = "/ignore" };
            var anyRequest = new BasicRequest { RawUrl = "/any" };

            logger.Log(basicRequest, null, null, TimeSpan.Zero);
            logger.Log(anyRequest, null, null, TimeSpan.Zero);
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
