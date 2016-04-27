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

    using ServiceStack.Web;
    using ServiceStack.Text;
    using ServiceStack;

    public class SeqRequestLogger : IRequestLogger
    {
        private readonly SeqRequestLogsSettings settings;

        private static int requestId;

        private readonly string eventsUri;

        private readonly string apiKey;

        public SeqRequestLogger(SeqRequestLogsSettings settings)
        {
            this.settings = settings;

            // set interface props, custom props are access via settings
            Enabled = settings.GetEnabled();
            EnableErrorTracking = settings.GetEnableErrorTracking();
            EnableRequestBodyTracking = settings.GetEnableRequestBodyTracking();
            EnableResponseTracking = settings.GetEnableResponseTracking();
            EnableSessionTracking = settings.GetEnableSessionTracking();
            ExcludeRequestDtoTypes = settings.GetExcludeRequestDtoTypes();
            HideRequestBodyForRequestDtoTypes = settings.GetHideRequestBodyForRequestDtoTypes();
            RequiredRoles = settings.GetRequiredRoles();
            AppendProperties = settings.GetAppendProperties();

            eventsUri = $"{settings.GetUrl()}/api/events/raw";
            apiKey = settings.GetApiKey();
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
                            webRequest.Headers.Add("X-Seq-ApiKey", apiKey);
                        });
            }
        }

        public bool Enabled { get; set; }

        public bool EnableSessionTracking { get; set; }

        public bool EnableRequestBodyTracking { get; set; }

        public bool EnableResponseTracking { get; set; }

        public bool EnableErrorTracking { get; set; }
        
        public string[] RequiredRoles { get; set; }

        public Type[] ExcludeRequestDtoTypes { get; set; }

        public Type[] HideRequestBodyForRequestDtoTypes { get; set; }

        /// <summary>
        /// Input: request, requestDto, response, requestDuration
        /// Output: List of Properties to append to Seq Log entry
        /// </summary>
        public SeqRequestLogsSettings.PropertyAppender AppendProperties { get; set; }

        public void Log(IRequest request, object requestDto, object response, TimeSpan requestDuration)
        {
            // bypasses all flags to run raw log event delegate if configured
            settings.GetRawLogEvent()?.Invoke(request, requestDto, response, requestDuration);
            
            if (!Enabled) return;

            var requestType = requestDto?.GetType();

            if (ExcludeRequestType(requestType)) return;

            var entry = CreateEntry(request, requestDto, response, requestDuration, requestType);

            BufferedLogEntries(entry);
        }

        public List<RequestLogEntry> GetLatestLogs(int? take)
        {
            // use seq browser for reading logs
            throw new NotSupportedException($"use seq browser {settings.GetUrl()} for reading logs");
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
            requestLogEntry.Properties.Add("IsRequestLog", "True"); // Used for filtering requests easily

            var totalMilliseconds = requestDuration.TotalMilliseconds;
            
            requestLogEntry.Properties.Add("ElapsedMilliseconds", totalMilliseconds);
            requestLogEntry.Properties.Add("RequestCount", Interlocked.Increment(ref requestId).ToString());

            if (request != null)
            {
                requestLogEntry.MessageTemplate = $"SeqRequestLogsFeature request from {request.AbsoluteUri.Split('?')[0]}";
                requestLogEntry.Properties.Add("HttpMethod", request.Verb);
                requestLogEntry.Properties.Add("AbsoluteUri", request.AbsoluteUri);
                requestLogEntry.Properties.Add("PathInfo", request.PathInfo);
                requestLogEntry.Properties.Add("IpAddress", request.UserHostAddress);
                requestLogEntry.Properties.Add("ForwardedFor", request.Headers[HttpHeaders.XForwardedFor]);
                requestLogEntry.Properties.Add("Referer", request.Headers[HttpHeaders.Referer]);
                foreach (var header in request.Headers.ToDictionary())
                {
                    requestLogEntry.Properties.Add($"Header-{header.Key}", header.Value);
                }
                requestLogEntry.Properties.Add("UserAuthId", request.GetItemOrCookie(HttpHeaders.XUserAuthId));
                requestLogEntry.Properties.Add("SessionId", request.GetSessionId());
                requestLogEntry.Properties.Add("Session", EnableSessionTracking ? request.GetSession(false) : null);
                requestLogEntry.Properties.Add("Items", request.Items.WithoutDuplicates());
            }

            if (HideRequestBodyForRequestDtoTypes != null
                && requestType != null
                && !HideRequestBodyForRequestDtoTypes.Contains(requestType))
            {
                requestLogEntry.Properties.Add("RequestDto", requestDto);
                if (request != null)
                {
                    requestLogEntry.Properties.Add("FormData", request.FormData.ToDictionary());

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
                    var httpResponse = response as IHttpResult;
                    if (httpResponse != null)
                    {
                        
                        requestLogEntry.Properties.Add("ResponseStatus", httpResponse.Response?.GetResponseStatus());
                        requestLogEntry.Properties.Add("StatusCode", httpResponse.Status.ToString());
                        requestLogEntry.Properties.Add("StatusDescription", httpResponse.StatusDescription);
                    }
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
                    requestLogEntry.Properties.Add("ErrorCode", errorResponse.ErrorCode);
                    requestLogEntry.Properties.Add("ErrorMessage", errorResponse.Message);
                    requestLogEntry.Properties.Add("StackTrace", errorResponse.StackTrace);
                }

                var ex = response as Exception;
                if(ex != null)  
                    requestLogEntry.Exception = ex.ToString();
            }

            if (AppendProperties != null)
            {
                foreach (var kvPair in AppendProperties?.Invoke(request, requestDto, response, requestDuration).Safe())
                {
                    requestLogEntry.Properties.GetOrAdd(kvPair.Key, key => kvPair.Value);
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
    }
}