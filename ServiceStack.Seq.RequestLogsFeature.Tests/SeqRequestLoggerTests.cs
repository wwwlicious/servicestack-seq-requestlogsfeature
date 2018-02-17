// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/. 
namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System;
    using FakeItEasy;
    using ServiceStack.Configuration;
    using ServiceStack.Host;
    using ServiceStack.Testing;
    using Xunit;

    public class SeqRequestLoggerTests
    {
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
}