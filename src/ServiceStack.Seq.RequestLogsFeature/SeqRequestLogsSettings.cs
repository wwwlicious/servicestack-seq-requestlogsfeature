// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Collections.Generic;

    using FluentValidation;
    using Web;

    [Obsolete("Use public properties of SeqRequestLogsFeature instead of explicit Settings object")]
    public class SeqRequestLogsSettings
    {
        private readonly Validator validator = new Validator();

        private readonly List<Type> excludeRequestDtoTypes = new List<Type>();

        private readonly List<Type> hideRequestBodyForRequestDtoTypes = new List<Type>();

        private readonly List<string> requiredRoles = new List<string>();

        private string url;

        private string apiKey;

        private bool? enabled;

        private bool? enableErrorTracking;

        private bool? enableRequestBodyTracking;

        private bool? enableSessionTracking;

        private bool? enableResponseTracking;

        private IRequestLogger logger;

        private SeqRequestLogsFeature.RawLogEvent rawLogEvent;

        private SeqRequestLogsFeature.PropertyAppender appendProperties;

        public SeqRequestLogsSettings(string url)
        {
            this.url = url;
            validator.ValidateAndThrow(this);
        }

        /// <summary>
        /// The seq server url to log to
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.SeqUrl property")]
        public SeqRequestLogsSettings Url(string url)
        {
            this.url = url;
            return this;
        }

        /// <summary>
        /// Optional seq api key for logging
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.ApiKey property")]
        public SeqRequestLogsSettings ApiKey(string apiKey)
        {
            this.apiKey = apiKey;
            return this;
        }

        /// <summary>
        /// Tap into log events stream, still called even if disabled from Seq Logging 
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.RawEventLogger property")]
        public SeqRequestLogsSettings AddLogEvent(SeqRequestLogsFeature.RawLogEvent rawLogEvent)
        {
            this.rawLogEvent = rawLogEvent;
            return this;
        }

        /// <summary>
        /// Controls access to Config Service for setting logging options at runtime
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.RequiredRoles property")]
        public SeqRequestLogsSettings RequiredRoles(params string[] role)
        {
            requiredRoles.AddRange(role);
            return this;
        }

        /// <summary>
        /// Change the RequestLogger provider. Default is SeqRequestLogger
        /// </summary>
        /// <param name="logger"> the request logger to use</param>
        [Obsolete("Use SeqRequestLogsFeature.Logger property")]
        public SeqRequestLogsSettings UseCustomLogger(IRequestLogger logger)
        {
            this.logger = logger;
            return this;
        }

        /// <summary>
        /// Adds custom properties to each log event
        /// </summary>
        /// <param name="appender">the property appender delegate</param>
        /// <returns></returns>
        [Obsolete("Use SeqRequestLogsFeature.AppendProperties property")]
        public SeqRequestLogsSettings AppendProperties(SeqRequestLogsFeature.PropertyAppender appender)
        {
            appendProperties = appender;
            return this;
        }

        /// <summary>
        /// Enables the request logging
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.Enabled property")]
        public SeqRequestLogsSettings Enabled(bool enable = true)
        {
            enabled = enable;
            return this;
        }

        /// <summary>
        /// Turn On/Off Tracking of Exceptions
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.EnableErrorTracking property")]
        public SeqRequestLogsSettings EnableErrorTracking(bool enable = true)
        {
            enableErrorTracking = enable;
            return this;
        }

        /// <summary>
        /// Turn On/Off Session Tracking
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.EnableSessionTracking property")]
        public SeqRequestLogsSettings EnableSessionTracking(bool enable = true)
        {
            enableSessionTracking = enable;
            return this;
        }

        /// <summary>
        /// Turn On/Off Logging of Raw Request Body, default is Off
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.EnableRequestBodyTracking property")]
        public SeqRequestLogsSettings EnableRequestBodyTracking(bool enable = true)
        {
            enableRequestBodyTracking = enable;
            return this;
        }

        /// <summary>
        /// Turn On/Off Tracking of Responses
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.EnableResponseTracking property")]
        public SeqRequestLogsSettings EnableResponseTracking(bool enable = true)
        {
            enableResponseTracking = enable;
            return this;
        }

        /// <summary>
        /// Don't log requests of these types. By default RequestLog's are excluded
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.ExcludeRequestDtoTypes property")]
        public SeqRequestLogsSettings ExcludeRequestDtoTypes(params Type[] types)
        {
            excludeRequestDtoTypes.AddRange(types);
            return this;
        }

        /// <summary>
        /// Removes the default excluded requestDto types
        /// Will log calls to RequestLog
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.ExcludeRequestDtoTypes.Clear()")]
        public SeqRequestLogsSettings ClearExcludeRequestDtoTypes()
        {
            excludeRequestDtoTypes.Clear();
            return this;
        }

        /// <summary>
        /// Don't log request bodys for services with sensitive information.
        /// By default Auth and Registration requests are hidden.
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.HideRequestBodyForRequestDtoTypes property")]
        public SeqRequestLogsSettings HideRequestBodyForRequestDtoTypes(params Type[] types)
        {
            hideRequestBodyForRequestDtoTypes.AddRange(types);
            return this;
        }

        /// <summary>
        /// Removes the default dto types for hiding the requestbody
        /// Will potentially log sensitive information from Authenticate and Registration dto's
        /// </summary>
        [Obsolete("Use SeqRequestLogsFeature.HideRequestBodyForRequestDtoTypes.Clear()")]
        public SeqRequestLogsSettings ClearHideRequestBodyForRequestDtoTypes()
        {
            hideRequestBodyForRequestDtoTypes.Clear();
            return this;
        }

        internal void PopulateProperties(SeqRequestLogsFeature feature)
        {
            if (!string.IsNullOrWhiteSpace(apiKey)) feature.ApiKey = apiKey;

            if (!string.IsNullOrWhiteSpace(url)) feature.SeqUrl = url;

            if (appendProperties != null) feature.AppendProperties = appendProperties;

            if (enabled.HasValue) feature.Enabled = enabled.Value;

            if (enableErrorTracking.HasValue) feature.EnableErrorTracking = enableErrorTracking.Value;

            if (enableRequestBodyTracking.HasValue) feature.EnableRequestBodyTracking = enableRequestBodyTracking.Value;

            if (enableResponseTracking.HasValue) feature.EnableResponseTracking = enableResponseTracking.Value;

            if (enableSessionTracking.HasValue) feature.EnableSessionTracking = enableSessionTracking.Value;

            if (!excludeRequestDtoTypes.IsNullOrEmpty()) feature.ExcludeRequestDtoTypes = excludeRequestDtoTypes;

            if (!hideRequestBodyForRequestDtoTypes.IsNullOrEmpty())
                feature.HideRequestBodyForRequestDtoTypes = hideRequestBodyForRequestDtoTypes;

            if (!requiredRoles.IsNullOrEmpty()) feature.RequiredRoles = requiredRoles;

            if (logger != null) feature.Logger = logger;

            if (rawLogEvent != null) feature.RawEventLogger = rawLogEvent;
        }
        
        private class Validator : AbstractValidator<SeqRequestLogsSettings>
        {
            public Validator()
            {
                RuleFor(cs => cs.url)
                    .NotEmpty()
                    .Must(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                    .WithMessage("Seq Url is not a valid url");
            }
        }
    }
}