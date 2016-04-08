using ServiceStack.Web;
using System;
using System.Linq;

namespace ServiceStack.Seq.RequestLogsFeature
{
    public class SeqRequestLogConfigService : Service
    {
        public SeqRequestLogConfig Any(SeqRequestLogConfig req)
        {
            var logger = TryResolve<IRequestLogger>() as SeqRequestLogger;
            if (logger != null)
            {
                if (logger.RequiredRoles.Any())
                {
                    var session = GetSession();
                    if (session != null && !logger.RequiredRoles.Any(t => session.HasRole(t)))
                    {
                        return null;
                    }
                }
                if (req.Enabled.HasValue) logger.Enabled = req.Enabled.Value;
                if (req.EnableErrorTracking.HasValue) logger.EnableErrorTracking = req.EnableErrorTracking.Value;
                if (req.EnableRequestBodyTracking.HasValue) logger.EnableRequestBodyTracking = req.EnableRequestBodyTracking.Value;
                if (req.EnableResponseTracking.HasValue)
                {
                    if (!req.EnableRequestBodyTracking.Value)
                        logger.EnableResponseTracking = req.EnableResponseTracking.Value;
                    else 
                    {
                        if (AppHostBase.Instance.GetPlugin<SeqRequestLogsFeature>().EnableResponseTracking)
                        {
                            logger.EnableResponseTracking = true;
                        }
                        else
                        {
                            throw new Exception("EnableResponseTracking cannot be enabled if not initially requested at AppHost startup. This feature requires PreRequestFilters");
                        }
                    }

                }
                if (req.EnableSessionTracking.HasValue) logger.EnableSessionTracking = req.EnableSessionTracking.Value;
                return logger.ConvertTo<SeqRequestLogConfig>();
            }
            throw new Exception("Could not resolve SeqRequestLogger");
        }
    }
}
