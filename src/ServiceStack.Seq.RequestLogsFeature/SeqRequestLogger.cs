namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using ServiceStack.Web;
    using ServiceStack.Text;

    public class SeqRequestLogger : IRequestLogger
    {
        private static int requestId;

        private readonly string seqUrl;

        private readonly string apiKey;

        public SeqRequestLogger(string seqUrl, string apiKey = null, bool enabled = true, bool enableErrorTracking = true, bool enabledRequestBodyTracking = false, bool enableSessionTracking = false, bool enableResponseTracking = false, Action<IRequest, object, object, TimeSpan> rawLogEvent = null)
        {
            seqUrl.ThrowIfNullOrEmpty(nameof(seqUrl));
            this.seqUrl = seqUrl;
            this.apiKey = apiKey;
            Enabled = enabled;
            EnableErrorTracking = enableErrorTracking;
            EnableRequestBodyTracking = enabledRequestBodyTracking;
            EnableResponseTracking = enableResponseTracking;
            EnableSessionTracking = enableSessionTracking;
            RawLogEvent = rawLogEvent;
        }
        public bool Enabled { get; set; }

        public bool EnableSessionTracking { get; set; }

        public bool EnableRequestBodyTracking { get; set; }

        public bool EnableResponseTracking { get; set; }

        public bool EnableErrorTracking { get; set; }

        /// <summary>
        /// Meaningless as reads and therefore access is controlled by seq
        /// </summary>
        public string[] RequiredRoles { get; set; } = new string[0];

        public Type[] ExcludeRequestDtoTypes { get; set; }

        /// <summary>
        /// Tap into log events stream, still called even if disabled from Seq Logging 
        /// </summary>
        public Action<IRequest, object, object, TimeSpan> RawLogEvent;

        public void Log(IRequest request, object requestDto, object response, TimeSpan requestDuration)
        {
            if (RawLogEvent != null)
                RawLogEvent(request, requestDto, response, requestDuration);

            if (!Enabled)
                return;

            var requestType = requestDto?.GetType();

            if (ExcludeRequestType(requestType))
                return;

            var entry = CreateEntry(request, requestDto, response, requestDuration, requestType);

            // TODO inefficient as uses 1 event : 1 http post to seq
            // replace with something to buffer/queue and  
            // batch entries for posting
            using (var scope = JsConfig.With(emitCamelCaseNames: false))
            {
                "{0}/api/events/raw".Fmt(seqUrl)
                    .PostJsonToUrlAsync(
                        new SeqLogRequest(entry),
                        webRequest => webRequest.Headers.Add("X-Seq-ApiKey", apiKey));
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
            requestLogEntry.Properties.Add("IsRequestLog", "True"); //Used for filtering requests easily
            requestLogEntry.Properties.Add("RequestDuration", requestDuration.ToString());
            requestLogEntry.Properties.Add("RequestCount", Interlocked.Increment(ref requestId).ToString());

            if (request != null)
            {
                requestLogEntry.MessageTemplate = "SeqRequestLogsFeature request from {0}".Fmt(request.AbsoluteUri.Split('?')[0]);
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
            throw new NotSupportedException("use seq browser {0} for reading logs".Fmt(seqUrl));
        }

        public Type[] HideRequestBodyForRequestDtoTypes { get; set; }
    }
}