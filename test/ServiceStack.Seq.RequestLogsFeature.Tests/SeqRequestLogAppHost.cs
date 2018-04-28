using ServiceStack.Testing;

namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using FakeItEasy;
    using Funq;

    using Web;
    using static FakeItEasy.A;

    public class SeqRequestLogAppHost : AppSelfHostBase
    {
        public readonly string BaseUrl = "http://localhost:1337/";
        private const string Url = "http://8.8.8.8:1234";
        public readonly IAppSettings Settings = Fake<IAppSettings>();

        private string httpLocalhost  = "http://localhost:5341";

        public List<CustomLog> CustomLogs { get; }

        public SeqRequestLogAppHost() : base("Unit Test Self Hosted", typeof(DemoService).Assembly)
        {
            CallTo(() => Settings.GetString(ConfigKeys.SeqUrl)).Returns(Url);
            CustomLogs = new List<CustomLog>();
            Init().Start(BaseUrl);
        }

        public override void Configure(Container container)
        {
            AppSettings = Settings;

            Plugins.Add(
                new SeqRequestLogsFeature(
                    new SeqRequestLogsSettings(httpLocalhost)
                        .EnableResponseTracking()
                        .EnableRequestBodyTracking()
                        .EnableSessionTracking()
                        .EnableErrorTracking()
                        .AddLogEvent(RawLogEvent)
                        .AppendProperties(
                            (request, dto, response, duration) =>
                                {
                                    var props = new Dictionary<string, object>();
                                    props.Add("new", "value");
                                    return props;
                                })));
        }

        private void RawLogEvent(IRequest request, object requestDto, object response, TimeSpan requestDuration)
        {
            CustomLogs.Add(new CustomLog {Request = request, RequestDto = requestDto, Response = response, RequestDuration = requestDuration});
        }

        public IServiceClient GetClient()
        {
            return new JsonServiceClient(BaseUrl);
        }

        public class CustomLog
        {
            public IRequest Request { get; set; }

            public object RequestDto { get; set; }

            public object Response { get; set; }

            public TimeSpan RequestDuration { get; set; }
        }
    }
}