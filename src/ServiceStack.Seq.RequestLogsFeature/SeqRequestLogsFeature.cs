// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Admin;
    using Configuration;
    using FluentValidation;
    using Logging;
    using Validators;
    using Web;

    public class SeqRequestLogsFeature : IPlugin
    {
        private readonly ILog log = LogManager.GetLogger(typeof(SeqRequestLogsFeature));
        private readonly IAppSettings appSettings;
        private readonly ConfigValidator configValidator = new ConfigValidator();

        public SeqRequestLogsFeature()
        {
            appSettings = appSettings ?? ServiceStackHost.Instance.AppSettings;

            if (log.IsDebugEnabled)
                log.Debug($"Using {appSettings.GetType().Name} appSettings for appSettings provider");
        }

        public SeqRequestLogsFeature(IAppSettings settings)
        {
            appSettings = settings.ThrowIfNull(nameof(settings));
            
            if (log.IsDebugEnabled)
                log.Debug($"Using {appSettings.GetType().Name} appSettings for appSettings provider");
        }

        [Obsolete("Use parameterless ctor instead and set public properties")]
        public SeqRequestLogsFeature(SeqRequestLogsSettings settings) : this()
        {
            // NOTE - this is to prevent breaking backwards compat.
            settings.ThrowIfNull(nameof(settings));
            settings.PopulateProperties(this);
        }

        /// <summary>
        /// Excludes requests for specific DTO types from logging, ignores RequestLog requests by default
        /// </summary>
        public IEnumerable<Type> ExcludeRequestDtoTypes { get; set; } = new List<Type>(new[] { typeof(RequestLogs) });

        /// <summary>
        /// Exclude request body for specific DTO types from logging, ignores authentication and registration dtos by default
        /// </summary>
        public IEnumerable<Type> HideRequestBodyForRequestDtoTypes { get; set; } =
            new List<Type>(new[] { typeof(Authenticate), typeof(Register) });

        /// <summary>
        /// Restrict access to the runtime log settings 
        /// </summary>
        public List<string> RequiredRoles
        {
            get => appSettings.GetList(ConfigKeys.RequiredRoles)?.ToList();
            set => appSettings.Set(ConfigKeys.RequiredRoles, string.Join(",", value));
        }

        /// <summary>
        /// Sets a seq server url
        /// </summary>
        public string SeqUrl
        {
            get => appSettings.GetString(ConfigKeys.SeqUrl);
            set
            {
                appSettings.Set(ConfigKeys.SeqUrl, value);
                configValidator.ValidateAndThrow(this);
            }
        }

        /// <summary>
        /// Sets a seq api key
        /// </summary>
        public string ApiKey
        {
            get => appSettings.GetString(ConfigKeys.ApiKey);
            set => appSettings.Set(ConfigKeys.ApiKey, value);
        }

        /// <summary>
        /// Turn the logging on and off, defaults to true
        /// </summary>
        public bool Enabled
        {
            get => appSettings.Get(ConfigKeys.Enabled, true);
            set => appSettings.Set(ConfigKeys.Enabled, value);
        }

        /// <summary>
        /// Log errors, defaults to true
        /// </summary>
        public bool EnableErrorTracking
        {
            get => appSettings.Get(ConfigKeys.EnableErrorTracking, true);
            set => appSettings.Set(ConfigKeys.EnableErrorTracking, value);
        }

        /// <summary>
        /// Log request bodies, defaults to false
        /// </summary>
        public bool EnableRequestBodyTracking
        {
            get => appSettings.Get(ConfigKeys.EnableRequestBodyTracking, false);
            set => appSettings.Set(ConfigKeys.EnableRequestBodyTracking, value);
        }

        /// <summary>
        /// Log session details, defaults to false
        /// </summary>
        public bool EnableSessionTracking
        {
            get => appSettings.Get(ConfigKeys.EnableSessionTracking, false);
            set => appSettings.Set(ConfigKeys.EnableSessionTracking, value);
        }

        /// <summary>
        /// Log responses, defaults to false
        /// </summary>
        public bool EnableResponseTracking
        {
            get => appSettings.Get(ConfigKeys.EnableResponseTracking, false);
            set => appSettings.Set(ConfigKeys.EnableResponseTracking, value);
        }
        
        /// <summary>
        /// Low level request filter for logging, return true to skip logging the request
        /// </summary>
        public Func<IRequest, bool> SkipLogging { get; set; }

        /// <summary>
        /// Append custom properties to all log entries
        /// </summary>
        public PropertyAppender AppendProperties { get; set; }

        /// <summary>
        /// Lowest level access to customised logging, executes before any other logging settings
        /// </summary>
        public RawLogEvent RawEventLogger { get; set; }

        private IRequestLogger logger;

        /// <summary>
        /// Sets the seq logger by default, override with a custom implemetation of <see cref="IRequestLogger"/>
        /// </summary>
        public IRequestLogger Logger
        {
            get { return logger = logger ?? new SeqRequestLogger(this); }
            set => logger = value;
        } 

        /// <summary>
        /// Low level delegate for appending custom properties to a log entry
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestDto"></param>
        /// <param name="response"></param>
        /// <param name="soapDuration"></param>
        public delegate Dictionary<string, object> PropertyAppender(
            IRequest request,
            object requestDto,
            object response,
            TimeSpan soapDuration);

        /// <summary>
        /// Low level delegate for customised logging
        /// </summary>
        /// <param name="request"></param>
        /// <param name="requestDto"></param>
        /// <param name="response"></param>
        /// <param name="requestDuration"></param>
        public delegate void RawLogEvent(
            IRequest request,
            object requestDto,
            object response,
            TimeSpan requestDuration);

        /// <summary>
        /// Registers the plugin with the apphost
        /// </summary>
        /// <param name="appHost"></param>
        public void Register(IAppHost appHost)
        {
            configValidator.ValidateAndThrow(this);

            ConfigureRequestLogger(appHost);

            var atRestPath = "/SeqRequestLogConfig";

            appHost.RegisterService(typeof(SeqRequestLogConfigService), atRestPath);

            if (EnableRequestBodyTracking)
            {
                appHost.PreRequestFilters.Insert(0, (httpReq, httpRes) =>
                {
                    httpReq.UseBufferedStream = true;
                });
            }

            appHost.GetPlugin<MetadataFeature>()
                .AddDebugLink(SeqUrl, "Seq Request Logs")
                .AddPluginLink(atRestPath.TrimStart('/'), "Seq IRequestLogger Configuration");
        }

        private void ConfigureRequestLogger(IAppHost appHost)
        {
            var requestLogger = Logger;
            requestLogger.EnableSessionTracking = EnableSessionTracking;
            requestLogger.EnableResponseTracking = EnableResponseTracking;
            requestLogger.EnableRequestBodyTracking = EnableRequestBodyTracking;
            requestLogger.EnableErrorTracking = EnableErrorTracking;
            requestLogger.ExcludeRequestDtoTypes = ExcludeRequestDtoTypes.ToArray();
            requestLogger.HideRequestBodyForRequestDtoTypes = HideRequestBodyForRequestDtoTypes.ToArray();
            requestLogger.SkipLogging = SkipLogging;
            appHost.Register(requestLogger);
        }
    }
}
