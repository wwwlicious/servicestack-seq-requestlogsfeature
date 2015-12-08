namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;

    using ServiceStack.Admin;
    using ServiceStack.Web;

    public class SeqRequestLogsFeature : IPlugin
    {
        private readonly string _apiKey;
        private readonly bool _initialEnabled;
        public SeqRequestLogsFeature(string seqUrl, string apiKey = null, bool enabled = true, bool enableErrorTracking = true, bool enabledRequestBodyTracking = false, bool enableSessionTracking = false, bool enableResponseTracking = false)
        {
            this.SeqUrl = seqUrl;
            _apiKey = apiKey;
            _initialEnabled = enabled;
            this.EnableErrorTracking = enableErrorTracking;
            this.EnableRequestBodyTracking = enabledRequestBodyTracking;
            this.EnableSessionTracking = enableSessionTracking;
            this.EnableResponseTracking = enableResponseTracking;
            this.ExcludeRequestDtoTypes = new[] { typeof(RequestLogs) };
            this.HideRequestBodyForRequestDtoTypes = new[] {
                typeof(Authenticate), typeof(Register)
            };
        }
        /// <summary>
        /// Seq server url
        /// </summary>
        public string SeqUrl { get; set; }

        /// <summary>
        /// Turn On/Off Session Tracking
        /// </summary>
        public bool EnableSessionTracking { get; set; }

        /// <summary>
        /// Turn On/Off Logging of Raw Request Body, default is Off
        /// </summary>
        public bool EnableRequestBodyTracking { get; set; }

        /// <summary>
        /// Turn On/Off Tracking of Responses
        /// </summary>
        public bool EnableResponseTracking { get; set; }

        /// <summary>
        /// Turn On/Off Tracking of Exceptions
        /// </summary>
        public bool EnableErrorTracking { get; set; }

        /// <summary>
        /// Change the RequestLogger provider. Default is SeqRequestLogger
        /// </summary>
        public IRequestLogger RequestLogger { get; set; }

        /// <summary>
        /// Don't log requests of these types. By default RequestLog's are excluded
        /// </summary>
        public Type[] ExcludeRequestDtoTypes { get; set; }

        /// <summary>
        /// Don't log request bodys for services with sensitive information.
        /// By default Auth and Registration requests are hidden.
        /// </summary>
        public Type[] HideRequestBodyForRequestDtoTypes { get; set; }

        public void Register(IAppHost appHost)
        {
            var requestLogger = RequestLogger ?? new SeqRequestLogger(SeqUrl, _apiKey, _initialEnabled);
            requestLogger.EnableSessionTracking = EnableSessionTracking;
            requestLogger.EnableResponseTracking = EnableResponseTracking;
            requestLogger.EnableRequestBodyTracking = EnableRequestBodyTracking;
            requestLogger.EnableErrorTracking = EnableErrorTracking;
            requestLogger.ExcludeRequestDtoTypes = ExcludeRequestDtoTypes;
            requestLogger.HideRequestBodyForRequestDtoTypes = HideRequestBodyForRequestDtoTypes;

            appHost.Register(requestLogger);
            appHost.RegisterService(typeof(SeqRequestLogConfigService));
            if (EnableRequestBodyTracking)
            {
                appHost.PreRequestFilters.Insert(0, (httpReq, httpRes) =>
                {
                    httpReq.UseBufferedStream = EnableRequestBodyTracking;
                });
            }

            appHost.GetPlugin<MetadataFeature>()
                .AddDebugLink(SeqUrl, "Seq Request Logs")
                .AddPluginLink("/SeqRequestLogConfig", "Seq IRequestLogger Configuration");
        }
    }
}
