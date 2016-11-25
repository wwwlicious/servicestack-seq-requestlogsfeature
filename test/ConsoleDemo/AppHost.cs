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
                // add additional properties to Seq log entry.
                AppendProperties =
                    (request, dto, response, duration) =>
                    new Dictionary<string, object> { { "NewCustomProperty", "42" } },
                ExcludeRequestDtoTypes = new[] { typeof(SeqRequestLogConfig) }, // add your own type exclusions
                HideRequestBodyForRequestDtoTypes = new[] { typeof(SeqRequestLogConfig) } // add your own exclusions for bodyrequest logging
            });

            /*Plugins.Add(
                new SeqRequestLogsFeature(
                    new SeqRequestLogsSettings("http://localhost:5341") // required seq server url:port

                        // everything below is optional
                        .ApiKey("seqApiKey")            // api key for seq
                        .Enabled()                      // default true
                        .EnableErrorTracking()          // default true
                        .EnableSessionTracking()        // default false
                        .EnableRequestBodyTracking()    // default false
                        .EnableResponseTracking()       // default false
                        .ClearExcludeRequestDtoTypes()  // remove default exclusions (RequestLog)
                        .ClearHideRequestBodyForRequestDtoTypes() // remove default request body exclusions (Auth, Registration)
                        .ExcludeRequestDtoTypes(typeof(SeqRequestLogConfig)) // add your own type exclusions
                        .HideRequestBodyForRequestDtoTypes(typeof(SeqRequestLogConfig)) // add your own exclusions for bodyrequest logging
                        .RequiredRoles("admin", "ops") // restrict the runtime configuration to specific roles
                        // .UseCustomLogger(new CustomLogger()) // swap out the seq logger for your own implementation
                        .AppendProperties(
                            (request, dto, response, duration) =>
                            {
                                return new Dictionary<string, object> { { "NewCustomProperty", "42" } }; // add additional properties to Seq log entry.
                            })
                        .AddLogEvent(
                            (request, dto, response, duration) =>
                                {
                                    // your custom log event
                                })));*/
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
