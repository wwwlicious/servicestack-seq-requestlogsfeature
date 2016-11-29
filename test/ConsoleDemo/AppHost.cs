// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ConsoleDemo
{
    using System;
    using System.Collections.Generic;

    using Funq;

    using ServiceStack;
    using ServiceStack.Request.Correlation;
    using ServiceStack.Seq.RequestLogsFeature;
    using ServiceStack.Web;

    public class AppHost : AppSelfHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost() : base("ConsoleDemo", typeof(DemoService).Assembly)
        {
        }

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            // Config examples
            Plugins.Add(new RequestCorrelationFeature());
            Plugins.Add(new SeqRequestLogsFeature
            {
                ApiKey = "Overwrite-app-setting",
                // add additional properties to Seq log entry.
                AppendProperties =
                    (request, dto, response, duration) =>
                    new Dictionary<string, object> { { "NewCustomProperty", "42" } },
                ExcludeRequestDtoTypes = new[] { typeof(SeqRequestLogConfig) }, // add your own type exclusions
                HideRequestBodyForRequestDtoTypes = new[] { typeof(SeqRequestLogConfig) } // add your own exclusions for bodyrequest logging
            });
        }
    }

    public class CustomLogger : IRequestLogger
    {
        public bool EnableSessionTracking { get; set; }

        public bool EnableRequestBodyTracking { get; set; }

        public bool EnableResponseTracking { get; set; }

        public bool EnableErrorTracking { get; set; }

        public string[] RequiredRoles { get; set; }

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
