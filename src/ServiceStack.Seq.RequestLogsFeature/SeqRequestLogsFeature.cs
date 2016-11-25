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
    using Validators;
    using Web;

    public class SeqRequestLogsFeature : IPlugin
    {
        private IAppSettings appSettings;
        private readonly ConfigValidator configValidator = new ConfigValidator();
        private readonly SeqRequestLogsSettings featureSettings;

        public SeqRequestLogsFeature()
        {
        }

        [Obsolete("Use parameterless ctor instead and set public properties")]
        public SeqRequestLogsFeature(SeqRequestLogsSettings settings)
        {
            // NOTE - this is to prevent breaking backwards compat.
            featureSettings = settings;
        }

        public IEnumerable<Type> ExcludeRequestDtoTypes { get; set; } = new List<Type>(new[] { typeof(RequestLogs) });

        public IEnumerable<Type> HideRequestBodyForRequestDtoTypes { get; set; } =
            new List<Type>(new[] { typeof(Authenticate), typeof(Register) });

        public List<string> RequiredRoles
        {
            get { return appSettings.GetList(ConfigKeys.RequiredRoles)?.ToList(); }
            set { appSettings.Set(ConfigKeys.RequiredRoles, value); }
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
            get { return appSettings.Get<bool>(ConfigKeys.EnableRequestBodyTracking); }
            set { appSettings.Set(ConfigKeys.EnableRequestBodyTracking, value); }
        }

        public bool EnableSessionTracking
        {
            get { return appSettings.Get<bool>(ConfigKeys.EnableSessionTracking); }
            set { appSettings.Set(ConfigKeys.EnableSessionTracking, value); }
        }

        public bool EnableResponseTracking
        {
            get { return appSettings.Get<bool>(ConfigKeys.EnableResponseTracking); }
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
            appSettings = appHost.AppSettings ?? new AppSettings();

            // If there is a feature settings object, use it to populate Plugin settings
            featureSettings?.PopulateProperties(this);

            configValidator.ValidateAndThrow(this);

            ConfigureRequestLogger(appHost);
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
