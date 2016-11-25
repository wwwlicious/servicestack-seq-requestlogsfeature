// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Collections.Generic;
    using Admin;
    using Web;

    public class SeqRequestLogsFeature : IPlugin
    {
        public SeqRequestLogsFeature()
        {
        }

        [Obsolete("Use parameterless ctor instead and set public properties")]
        public SeqRequestLogsFeature(SeqRequestLogsSettings settings)
        {
            // NOTE - this is to prevent breaking backwards compat.
            settings.PopulateProperties(this);
        }

        public List<Type> ExcludeRequestDtoTypes { get; set; } = new List<Type>(new[] { typeof(RequestLogs) });

        public List<Type> HideRequestBodyForRequestDtoTypes { get; set; } =
            new List<Type>(new[] { typeof(Authenticate), typeof(Register) });

        public List<string> RequiredRoles { get; set; } = new List<string>();

        public string SeqUrl { get; set; }

        public string ApiKey { get; set; }

        public bool Enabled { get; set; } = true;

        public bool EnableErrorTracking { get; set; } = true;

        public bool EnableRequestBodyTracking { get; set; }

        public bool EnableSessionTracking { get; set; }

        public bool EnableResponseTracking { get; set; }

        public PropertyAppender AppendProperties { get; set; }

        public RawLogEvent RawEventLogger { get; set; }

        private IRequestLogger logger;
        public IRequestLogger Logger
        {
            get { return logger = logger ?? new SeqRequestLogger(this); }
            set { logger = value; }
        } 

        public delegate Dictionary<string, object> PropertyAppender(
            IRequest request,
            object requestDto,
            object response,
            TimeSpan SoapDuration);

        public delegate void RawLogEvent(
            IRequest request,
            object requestDto,
            object response,
            TimeSpan requestDuration);

        public void Register(IAppHost appHost)
        {
            var requestLogger = Logger;
            requestLogger.EnableSessionTracking = EnableSessionTracking;
            requestLogger.EnableResponseTracking = EnableResponseTracking;
            requestLogger.EnableRequestBodyTracking = EnableRequestBodyTracking;
            requestLogger.EnableErrorTracking = EnableErrorTracking;
            requestLogger.ExcludeRequestDtoTypes = ExcludeRequestDtoTypes.ToArray();
            requestLogger.HideRequestBodyForRequestDtoTypes = HideRequestBodyForRequestDtoTypes.ToArray();

            appHost.Register(requestLogger);
            appHost.RegisterService(typeof(SeqRequestLogConfigService));
            if (EnableRequestBodyTracking)
            {
                appHost.PreRequestFilters.Insert(0, (httpReq, httpRes) =>
                {
                    httpReq.UseBufferedStream = true;
                });
            }

            appHost.GetPlugin<MetadataFeature>()
                .AddDebugLink(SeqUrl, "Seq Request Logs")
                .AddPluginLink("/SeqRequestLogConfig", "Seq IRequestLogger Configuration");
        }
    }
}
