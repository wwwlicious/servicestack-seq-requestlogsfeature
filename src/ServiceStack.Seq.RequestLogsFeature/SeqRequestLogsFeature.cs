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
            appSettings = AppHostBase.Instance.AppSettings;

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

        public IEnumerable<Type> ExcludeRequestDtoTypes { get; set; } = new List<Type>(new[] { typeof(RequestLogs) });

        public IEnumerable<Type> HideRequestBodyForRequestDtoTypes { get; set; } =
            new List<Type>(new[] { typeof(Authenticate), typeof(Register) });

        public List<string> RequiredRoles
        {
            get { return appSettings.GetList(ConfigKeys.RequiredRoles)?.ToList(); }
            set { appSettings.Set(ConfigKeys.RequiredRoles, string.Join(",", value)); }
        }

        public string SeqUrl
        {
            get { return appSettings.GetString(ConfigKeys.SeqUrl); }
            set
            {
                appSettings.Set(ConfigKeys.SeqUrl, value);
                configValidator.ValidateAndThrow(this);
            }
        }

        public string ApiKey
        {
            get { return appSettings.GetString(ConfigKeys.ApiKey); }
            set { appSettings.Set(ConfigKeys.ApiKey, value); }
        }

        public bool Enabled
        {
            get { return appSettings.Get(ConfigKeys.Enabled, true); }
            set { appSettings.Set(ConfigKeys.Enabled, value); }
        }

        public bool EnableErrorTracking
        {
            get { return appSettings.Get(ConfigKeys.EnableErrorTracking, true); }
            set { appSettings.Set(ConfigKeys.EnableErrorTracking, value); }
        }

        public bool EnableRequestBodyTracking
        {
            get { return appSettings.Get(ConfigKeys.EnableRequestBodyTracking, false); }
            set { appSettings.Set(ConfigKeys.EnableRequestBodyTracking, value); }
        }

        public bool EnableSessionTracking
        {
            get { return appSettings.Get(ConfigKeys.EnableSessionTracking, false); }
            set { appSettings.Set(ConfigKeys.EnableSessionTracking, value); }
        }

        public bool EnableResponseTracking
        {
            get { return appSettings.Get(ConfigKeys.EnableResponseTracking, false); }
            set { appSettings.Set(ConfigKeys.EnableResponseTracking, value); }
        }

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
            TimeSpan soapDuration);

        public delegate void RawLogEvent(
            IRequest request,
            object requestDto,
            object response,
            TimeSpan requestDuration);

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

            appHost.Register(requestLogger);
        }
    }
}
