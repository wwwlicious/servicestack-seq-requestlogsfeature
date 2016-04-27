// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Collections.Generic;

    using ServiceStack.Admin;
    using ServiceStack.FluentValidation;
    using ServiceStack.Web;

    public class SeqRequestLogsSettings
    {
        private readonly Validator validator = new Validator();

        private readonly List<Type> excludeRequestDtoTypes =
            new List<Type>(new[] { typeof(RequestLogs) });

        private readonly List<Type> hideRequestBodyForRequestDtoTypes =
            new List<Type>(new[] { typeof(Authenticate), typeof(Register) });

        private readonly List<string> requiredRoles = new List<string>();

        private string url;

        private string apiKey;

        private bool enabled = true;

        private bool enableErrorTracking = true;

        private bool enableRequestBodyTracking;

        private bool enableSessionTracking;

        private bool enableResponseTracking;

        private IRequestLogger logger;

        private RawLogEvent rawLogEvent;

        private PropertyAppender appendProperties;

        public SeqRequestLogsSettings(string url)
        {
            this.url = url;
            validator.ValidateAndThrow(this);
        }

        public delegate void RawLogEvent(
            IRequest request, 
            object requestDto, 
            object response, 
            TimeSpan requestDuration);

        public delegate Dictionary<string, object> PropertyAppender(
            IRequest request,
            object requestDto,
            object response,
            TimeSpan SoapDuration);

        /// <summary>
        /// The seq server url to log to
        /// </summary>
        public SeqRequestLogsSettings Url(string url)
        {
            this.url = url;
            return this;
        }

        /// <summary>
        /// Optional seq api key for logging
        /// </summary>
        public SeqRequestLogsSettings ApiKey(string apiKey)
        {
            this.apiKey = apiKey;
            return this;
        }

        /// <summary>
        /// Tap into log events stream, still called even if disabled from Seq Logging 
        /// </summary>
        public SeqRequestLogsSettings AddLogEvent(RawLogEvent rawLogEvent)
        {
            this.rawLogEvent = rawLogEvent;
            return this;
        }

        /// <summary>
        /// Controls access to Config Service for setting logging options at runtime
        /// </summary>
        public SeqRequestLogsSettings RequiredRoles(params string[] role)
        {
            requiredRoles.AddRange(role);
            return this;
        }

        /// <summary>
        /// Change the RequestLogger provider. Default is SeqRequestLogger
        /// </summary>
        /// <param name="logger"> the request logger to use</param>
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
        public SeqRequestLogsSettings AppendProperties(PropertyAppender appender)
        {
            this.appendProperties = appender;
            return this;
        }
        
        /// <summary>
        /// Enables the request logging
        /// </summary>
        public SeqRequestLogsSettings Enabled(bool enable = true)
        {
            this.enabled = enable;
            return this;
        }

        /// <summary>
        /// Turn On/Off Tracking of Exceptions
        /// </summary>
        public SeqRequestLogsSettings EnableErrorTracking(bool enable = true)
        {
            this.enableErrorTracking = enable;
            return this;
        }

        /// <summary>
        /// Turn On/Off Session Tracking
        /// </summary>
        public SeqRequestLogsSettings EnableSessionTracking(bool enable = true)
        {
            this.enableSessionTracking = enable;
            return this;
        }

        /// <summary>
        /// Turn On/Off Logging of Raw Request Body, default is Off
        /// </summary>
        public SeqRequestLogsSettings EnableRequestBodyTracking(bool enable = true)
        {
            this.enableRequestBodyTracking = enable;
            return this;
        }

        /// <summary>
        /// Turn On/Off Tracking of Responses
        /// </summary>
        public SeqRequestLogsSettings EnableResponseTracking(bool enable = true)
        {
            this.enableResponseTracking = enable;
            return this;
        }

        /// <summary>
        /// Don't log requests of these types. By default RequestLog's are excluded
        /// </summary>
        public SeqRequestLogsSettings ExcludeRequestDtoTypes(params Type[] types)
        {
            this.excludeRequestDtoTypes.AddRange(types);
            return this;
        }

        /// <summary>
        /// Removes the default excluded requestDto types
        /// Will log calls to RequestLog
        /// </summary>
        public SeqRequestLogsSettings ClearExcludeRequestDtoTypes()
        {
            this.excludeRequestDtoTypes.Clear();
            return this;
        }

        /// <summary>
        /// Don't log request bodys for services with sensitive information.
        /// By default Auth and Registration requests are hidden.
        /// </summary>
        public SeqRequestLogsSettings HideRequestBodyForRequestDtoTypes(params Type[] types)
        {
            this.hideRequestBodyForRequestDtoTypes.AddRange(types);
            return this;
        }

        /// <summary>
        /// Removes the default dto types for hiding the requestbody
        /// Will potentially log sensitive information from Authenticate and Registration dto's
        /// </summary>
        public SeqRequestLogsSettings ClearHideRequestBodyForRequestDtoTypes()
        {
            this.hideRequestBodyForRequestDtoTypes.Clear();
            return this;
        }

        internal PropertyAppender GetAppendProperties()
        {
            return appendProperties;
        }

        internal string GetUrl()
        {
            validator.ValidateAndThrow(this);
            return url;
        }

        internal IRequestLogger GetLogger()
        {
            return logger ?? new SeqRequestLogger(this);
        }

        internal string GetApiKey()
        {
            return apiKey;
        }

        internal string[] GetRequiredRoles()
        {
            return requiredRoles.ToArray();
        }

        internal bool GetEnabled()
        {
            return enabled;
        }

        internal bool GetEnableSessionTracking()
        {
            return enableSessionTracking;
        }

        internal bool GetEnableResponseTracking()
        {
            return enableResponseTracking;
        }

        internal bool GetEnableRequestBodyTracking()
        {
            return enableRequestBodyTracking;
        }

        internal bool GetEnableErrorTracking()
        {
            return enableErrorTracking;
        }

        internal Type[] GetExcludeRequestDtoTypes()
        {
            return excludeRequestDtoTypes.ToArray();
        }

        internal Type[] GetHideRequestBodyForRequestDtoTypes()
        {
            return hideRequestBodyForRequestDtoTypes.ToArray();
        }

        /// <summary>
        /// Tap into log events stream, still called even if disabled from Seq Logging 
        /// </summary>
        internal RawLogEvent GetRawLogEvent()
        {
            return rawLogEvent;
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