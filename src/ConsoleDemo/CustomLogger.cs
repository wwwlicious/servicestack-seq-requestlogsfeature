namespace ConsoleDemo
{
    using System;
    using System.Collections.Generic;
    using ServiceStack;
    using ServiceStack.Web;

    public class CustomLogger : IRequestLogger
    {
        public bool EnableSessionTracking { get; set; }

        public bool EnableRequestBodyTracking { get; set; }

        public bool EnableResponseTracking { get; set; }

        public bool EnableErrorTracking { get; set; }

        public bool LimitToServiceRequests { get; set; }

        public string[] RequiredRoles { get; set; }

        public Func<IRequest, bool> SkipLogging { get; set; }

        public Type[] ExcludeRequestDtoTypes { get; set; }

        public Type[] HideRequestBodyForRequestDtoTypes { get; set; }

        public void Log(IRequest request, object requestDto, object response, TimeSpan elapsed)
        {
            throw new NotImplementedException();
        }

        List<RequestLogEntry> IRequestLogger.GetLatestLogs(int? take)
        {
            throw new NotImplementedException();
        }
    }
}