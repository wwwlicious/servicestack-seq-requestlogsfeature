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
        }

        public bool Enabled { get; set; }

        public bool EnableSessionTracking { get; set; }

        public bool EnableRequestBodyTracking { get; set; }

        public bool EnableResponseTracking { get; set; }

        public bool EnableErrorTracking { get; set; }
        
        public string[] RequiredRoles { get; set; }

        public Type[] ExcludeRequestDtoTypes { get; set; }


        /// <summary>
        /// Input: request, requestDto, response, requestDuration
        /// Output: List of Properties to append to Seq Log entry
        /// </summary>
        public Func<IRequest,object,object,TimeSpan,Dictionary<string, object>> AppendProperties { get; set; }

        /// <summary>
        /// Tap into log events stream, still called even if disabled from Seq Logging 
        /// </summary>
        public Action<IRequest, object, object, TimeSpan> RawLogEvent;

        public void Log(IRequest request, object requestDto, object response, TimeSpan requestDuration)
        {
            // bypasses all flags to run raw log event delegate if configured
            settings.GetRawLogEvent()?.Invoke(request, requestDto, response, requestDuration);

            
            if (!Enabled) return;

            var requestType = requestDto?.GetType();

            if (ExcludeRequestType(requestType)) return;

            var entry = CreateEntry(request, requestDto, response, requestDuration, requestType);

            // TODO inefficient as uses 1 event : 1 http post to seq
            // replace with something to buffer/queue and  
            // batch entries for posting
            using (var scope = JsConfig.With(emitCamelCaseNames: false))
            {
                // scope to force json camel casing off
                $"{settings.GetUrl()}/api/events/raw".PostJsonToUrlAsync(
                    new SeqLogRequest(entry),
                    webRequest => webRequest.Headers.Add("X-Seq-ApiKey", settings.GetApiKey()));
            }
        }

        protected SeqRequestLogEntry CreateEntry(
            IRequest request,
            object requestDto,
            object response,
            TimeSpan requestDuration,
            Type requestType)
        {
            var requestLogEntry = new SeqRequestLogEntry
            {
                Timestamp = DateTime.UtcNow.ToString("o")
            };
            requestLogEntry.Properties.Add("IsRequestLog", "True"); // Used for filtering requests easily
            requestLogEntry.Properties.Add("RequestDuration", requestDuration.TotalMilliseconds);
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
                requestLogEntry.Properties.Add("Headers", request.Headers.ToDictionary());
                requestLogEntry.Properties.Add("UserAuthId", request.GetItemOrCookie(HttpHeaders.XUserAuthId));
                requestLogEntry.Properties.Add("SessionId", request.GetSessionId());
                requestLogEntry.Properties.Add("Items", request.Items);
                requestLogEntry.Properties.Add("Session", EnableSessionTracking ? request.GetSession(false) : null);
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
                var errorResponse = response as IHttpResult;
                if (errorResponse != null)
                {
                    requestLogEntry.Level = errorResponse.StatusCode >= HttpStatusCode.BadRequest
                                            && errorResponse.StatusCode < HttpStatusCode.InternalServerError
                                                ? "Warning"
                                                : "Error";
                    requestLogEntry.Properties.Add("StatusCode", errorResponse.Status.ToString());
                    requestLogEntry.Properties.Add("StatusDescription", errorResponse.StatusDescription);
                    requestLogEntry.Properties.Add("ErrorResponse", errorResponse.Response);
                }

                var ex = response as Exception;
                requestLogEntry.Properties.Add("Error", ex);
            }
            foreach(var kvPair in AppendProperties?.Invoke(request, requestDto, response, requestDuration).Safe())
            {
                requestLogEntry.Properties.GetOrAdd(kvPair.Key, key => kvPair.Value);
            }
            return requestLogEntry;
        }

        protected bool ExcludeRequestType(Type requestType)
        {
            return ExcludeRequestDtoTypes != null
                   && requestType != null
                   && ExcludeRequestDtoTypes.Contains(requestType);
        }

        public List<RequestLogEntry> GetLatestLogs(int? take)
        {
            // use seq browser for reading logs
            throw new NotSupportedException($"use seq browser {settings.GetUrl()} for reading logs");
        }

        public Type[] HideRequestBodyForRequestDtoTypes { get; set; }
    }
}