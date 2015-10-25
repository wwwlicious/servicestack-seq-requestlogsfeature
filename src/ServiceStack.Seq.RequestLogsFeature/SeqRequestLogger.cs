namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using ServiceStack.Common;
    using ServiceStack.Common.Web;
    using ServiceStack.Text;
    using ServiceStack.ServiceHost;
    using ServiceStack.ServiceInterface;
    using ServiceStack.ServiceInterface.ServiceModel;
    using ServiceStack.ServiceModel;

    public class SeqRequestLogger : IRequestLogger
    {
        private static int requestId;

        private readonly string seqUrl;

        private readonly string apiKey;

        public SeqRequestLogger(string seqUrl, string apiKey = null)
        {
            seqUrl.ThrowIfNullOrEmpty(nameof(seqUrl));
            this.seqUrl = seqUrl;
            this.apiKey = apiKey;
        }

        public bool EnableSessionTracking { get; set; }

        public bool EnableRequestBodyTracking { get; set; }

        public bool EnableResponseTracking { get; set; }

        public bool EnableErrorTracking { get; set; }

        /// <summary>
        /// Meaningless as reads and therefore access is controlled by seq
        /// </summary>
        public string[] RequiredRoles { get; set; } = new string[0];

        public Type[] ExcludeRequestDtoTypes { get; set; }

        public void Log(IRequestContext requestContext, object requestDto, object response, TimeSpan elapsed)
        {
            var type = requestDto?.GetType();
            if (ExcludeRequestDtoTypes != null && type != null && ExcludeRequestDtoTypes.Contains(type))
                return;
            var requestLogEntry = new SeqRequestLogEntry {
                                                             Timestamp = DateTime.UtcNow.ToString("o")
                                                         };
            requestLogEntry.Properties.Add("RequestDuration", elapsed.ToString());
            requestLogEntry.Properties.Add("RequestCount", Interlocked.Increment(ref requestId).ToString());

            var httpReq = requestContext?.Get<IHttpRequest>();
            if (httpReq != null)
            {
                requestLogEntry.MessageTemplate = "SeqRequestLogsFeature request from {0}".Fmt(httpReq.AbsoluteUri.Split('?')[0]);
                requestLogEntry.Properties.Add("HttpMethod", httpReq.HttpMethod);
                requestLogEntry.Properties.Add("AbsoluteUri", httpReq.AbsoluteUri);
                requestLogEntry.Properties.Add("PathInfo", httpReq.PathInfo);
                requestLogEntry.Properties.Add("IpAddress", requestContext.IpAddress);
                requestLogEntry.Properties.Add("ForwardedFor", httpReq.Headers["X-Forwarded-For"]);
                requestLogEntry.Properties.Add("Referer", httpReq.Headers["Referer"]);
                requestLogEntry.Properties.Add("Headers", httpReq.Headers.ToDictionary());
                requestLogEntry.Properties.Add("UserAuthId", httpReq.GetItemOrCookie("X-UAId"));
                requestLogEntry.Properties.Add("SessionId", httpReq.GetSessionId());
                requestLogEntry.Properties.Add("Items", httpReq.Items);
                requestLogEntry.Properties.Add("Session", EnableSessionTracking ? httpReq.GetSession(false) : null);
            }
            if (HideRequestBodyForRequestDtoTypes != null && type != null && !HideRequestBodyForRequestDtoTypes.Contains(type))
            {
                requestLogEntry.Properties.Add("RequestDto", requestDto);
                if (httpReq != null)
                {
                    requestLogEntry.Properties.Add("FormData", httpReq.FormData.ToDictionary());
                    if (this.EnableRequestBodyTracking)
                        requestLogEntry.Properties.Add("RequestBody", httpReq.GetRawBody());
                }
            }
            if (!response.IsErrorResponse())
            {
                if (EnableResponseTracking)
                {
                    requestLogEntry.Properties.Add("ResponseDto", response);
                    requestLogEntry.Properties.Add("ResponseStatus", response.ToResponseStatus());
                    var httpResponse = response as IHttpResult;
                    if (httpResponse != null)
                    {
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

            // TODO inefficient as uses 1 event : 1 http post to seq
            // replace with something to buffer/queue and  
            // batch entries for posting
            Task.Run(
                () =>
                {
                    "{0}/api/events/raw".Fmt(seqUrl).PostJsonToUrl(
                        new SeqLogRequest(requestLogEntry),
                        request => request.Headers.Add("X-Seq-ApiKey", apiKey));
                });
        }

        public List<RequestLogEntry> GetLatestLogs(int? take)
        {
            // use seq browser for reading logs
            throw new NotSupportedException("use seq browser {0} for reading logs".Fmt(seqUrl));
        }

        public Type[] HideRequestBodyForRequestDtoTypes { get; set; }
    }
}