// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using Web;
    using Text;
    using ServiceStack;
    using Logging;

    public class SeqRequestLogger : IRequestLogger
    {
        private readonly SeqRequestLogsFeature feature;

        private static int requestId;

        private readonly string eventsUri;

        public SeqRequestLogger(SeqRequestLogsFeature feature)
        {
            this.feature = feature;
            eventsUri = $"{feature.SeqUrl}/api/events/raw";
        }

        private void BufferedLogEntries(SeqRequestLogEntry entry)
        {
            // TODO add buffering to logging for perf
            // scope to force json camel casing off
            using (JsConfig.With(emitCamelCaseNames: false))
            {
                eventsUri.PostJsonToUrlAsync(
                    new SeqLogRequest(entry),
                    webRequest =>
                        {
                            if(!string.IsNullOrWhiteSpace(feature.ApiKey))
                                webRequest.Headers.Add("X-Seq-ApiKey", feature.ApiKey);
                        });
            }
        }

        public bool Enabled
        {
            get { return feature.Enabled; }
            set { feature.Enabled = value; }
        }

        public bool EnableSessionTracking
        {
            get { return feature.EnableSessionTracking; }
            set { feature.EnableSessionTracking = value; }
        }

        public bool EnableRequestBodyTracking
        {
            get { return feature.EnableRequestBodyTracking; }
            set { feature.EnableRequestBodyTracking = value; }
        }

        public bool EnableResponseTracking
        {
            get { return feature.EnableResponseTracking; }
            set { feature.EnableResponseTracking = value; }
        }

        public bool EnableErrorTracking
        {
            get { return feature.EnableErrorTracking; }
            set { feature.EnableErrorTracking = value; }
        }

        public string[] RequiredRoles
        {
            get { return feature.RequiredRoles?.ToArray(); }
            set { feature.RequiredRoles = value?.ToList(); }
        }

        public Type[] ExcludeRequestDtoTypes
        {
            get { return feature.ExcludeRequestDtoTypes?.ToArray(); }
            set { feature.ExcludeRequestDtoTypes = value?.ToList(); }
        }

        public Type[] HideRequestBodyForRequestDtoTypes
        {
            get { return feature.HideRequestBodyForRequestDtoTypes?.ToArray(); }
            set { feature.HideRequestBodyForRequestDtoTypes = value?.ToList(); }
        }

        /// <summary>
        /// Input: request, requestDto, response, requestDuration
        /// Output: List of Properties to append to Seq Log entry
        /// </summary>
        public SeqRequestLogsFeature.PropertyAppender AppendProperties
        {
            get { return feature.AppendProperties; }
            set { feature.AppendProperties = value; }
        }

        public void Log(IRequest request, object requestDto, object response, TimeSpan requestDuration)
        {
            try
            {
                // bypasses all flags to run raw log event delegate if configured
                feature.RawEventLogger?.Invoke(request, requestDto, response, requestDuration);

                if (!Enabled) return;

                var requestType = requestDto?.GetType();

                if (ExcludeRequestType(requestType)) return;

                var entry = CreateEntry(request, requestDto, response, requestDuration, requestType);
                BufferedLogEntries(entry);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(SeqRequestLogger))
                    .Error("SeqRequestLogger threw unexpected exception", ex);
            }
        }

        public List<RequestLogEntry> GetLatestLogs(int? take)
        {
            // use seq browser for reading logs
            throw new NotSupportedException($"use seq browser {feature.SeqUrl} for reading logs");
        }

        protected SeqRequestLogEntry CreateEntry(
            IRequest request,
            object requestDto,
            object response,
            TimeSpan requestDuration,
            Type requestType)
        {
            var requestLogEntry = new SeqRequestLogEntry();
            requestLogEntry.Timestamp = DateTime.UtcNow.ToString("o");
            requestLogEntry.MessageTemplate = "HTTP {HttpMethod} {PathInfo} responded {StatusCode} in {ElapsedMilliseconds}ms";
            requestLogEntry.Properties.Add("IsRequestLog", "True"); // Used for filtering requests easily
            requestLogEntry.Properties.Add("SourceContext", "ServiceStack.Seq.RequestLogsFeature");
            requestLogEntry.Properties.Add("ElapsedMilliseconds", requestDuration.TotalMilliseconds);
            requestLogEntry.Properties.Add("RequestCount", Interlocked.Increment(ref requestId).ToString());
            requestLogEntry.Properties.Add("ServiceName", HostContext.AppHost.ServiceName);

            if (request != null)
            {
                requestLogEntry.Properties.Add("HttpMethod", request.Verb);
                requestLogEntry.Properties.Add("AbsoluteUri", request.AbsoluteUri);
                requestLogEntry.Properties.Add("PathInfo", request.PathInfo);
                requestLogEntry.Properties.Add("IpAddress", request.UserHostAddress);
                requestLogEntry.Properties.Add("ForwardedFor", request.Headers[HttpHeaders.XForwardedFor]);
                requestLogEntry.Properties.Add("Referer", request.Headers[HttpHeaders.Referer]);
                requestLogEntry.Properties.Add("Session", EnableSessionTracking ? request.GetSession(false) : null);
                requestLogEntry.Properties.Add("Items", request.Items.WithoutDuplicates());
                requestLogEntry.Properties.Add("StatusCode", request.Response?.StatusCode);
                requestLogEntry.Properties.Add("StatusDescription", request.Response?.StatusDescription);
                requestLogEntry.Properties.Add("ResponseStatus", request.Response?.GetResponseStatus());
            }

            var isClosed = request.Response.IsClosed;
            if (!isClosed)
            {
                requestLogEntry.Properties.Add("UserAuthId", request.GetItemOrCookie(HttpHeaders.XUserAuthId));
                requestLogEntry.Properties.Add("SessionId", request.GetSessionId());
            }
            
            if (HideRequestBodyForRequestDtoTypes != null
                && requestType != null
                && !HideRequestBodyForRequestDtoTypes.Contains(requestType))
            {
                requestLogEntry.Properties.Add("RequestDto", requestDto);
                if (request != null)
                {
                    if (!isClosed)
                    {
                        requestLogEntry.Properties.Add("FormData", request.FormData.ToDictionary());
                    }

                    if (EnableRequestBodyTracking)
                    {
                        requestLogEntry.Properties.Add("RequestBody", request.GetRawBody());
                    }
                }
            }
            
            if (!response.IsErrorResponse())
            {
                if (EnableResponseTracking)
                {
                    requestLogEntry.Properties.Add("ResponseDto", response);
                }
            }
            else if (EnableErrorTracking)
            {
                var errorResponse = response as IHttpError;
                if (errorResponse != null)
                {
                    requestLogEntry.Level = errorResponse.StatusCode >= HttpStatusCode.BadRequest
                                            && errorResponse.StatusCode < HttpStatusCode.InternalServerError
                                                ? "Warning"
                                                : "Error";
                    requestLogEntry.Properties["StatusCode"] = (int)errorResponse.StatusCode;
                    requestLogEntry.Properties.Add("ErrorCode", errorResponse.ErrorCode);
                    requestLogEntry.Properties.Add("ErrorMessage", errorResponse.Message);
                    requestLogEntry.Properties.Add("StackTrace", errorResponse.StackTrace);
                }

                var ex = response as Exception;
                if (ex != null)
                {
                    if (ex.InnerException != null)
                    {
                        requestLogEntry.Exception = ex.InnerException.ToString();
                        requestLogEntry.Properties.Add("ExceptionSource", ex.InnerException.Source);
                        requestLogEntry.Properties.Add("ExceptionData", ex.InnerException.Data);
                    }
                    else
                    {
                        requestLogEntry.Exception = ex.ToString();
                    }
                }
            }

            if (AppendProperties != null)
            {
                foreach (var kvPair in AppendProperties?.Invoke(request, requestDto, response, requestDuration).Safe())
                {
                    requestLogEntry.Properties.GetOrAdd(kvPair.Key, key => kvPair.Value);
                }
            }

            foreach (var header in request.Headers.ToDictionary())
            {
                if (!requestLogEntry.Properties.ContainsValue(header.Value))
                {
                    requestLogEntry.Properties.Add($"Header-{header.Key}", header.Value);
                }
            }

            return requestLogEntry;
        }

        protected bool ExcludeRequestType(Type requestType)
        {
            return ExcludeRequestDtoTypes != null
                   && requestType != null
                   && ExcludeRequestDtoTypes.Contains(requestType);
        }

        public bool LimitToServiceRequests { get; set; }
        public Func<IRequest, bool> SkipLogging { get; set; }
    }
}
