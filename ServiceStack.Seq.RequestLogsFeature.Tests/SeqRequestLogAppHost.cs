namespace ServiceStack.Seq.RequestLogsFeature.Tests
{
    using System;
    using System.Collections.Generic;

    using Funq;

    using ServiceStack.Web;
    
    public class SeqRequestLogAppHost : AppSelfHostBase
    {
        public readonly string BaseUrl = "http://localhost:1337/";

        private string httpLocalhost  = "http://localhost:5341";

        public List<CustomLog> CustomLogs { get; }

        public SeqRequestLogAppHost() : base("DemoService", typeof(DemoService).Assembly)
        {
            CustomLogs = new List<CustomLog>();
            this.Init().Start(BaseUrl);
        }

        public override void Configure(Container container)
        {
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