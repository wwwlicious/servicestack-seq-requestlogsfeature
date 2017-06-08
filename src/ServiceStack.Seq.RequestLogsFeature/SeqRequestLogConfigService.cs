// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Linq;

    using ServiceStack.Web;
    
    [Restrict(VisibilityTo = RequestAttributes.None)]
    public class SeqRequestLogConfigService : Service
    {
        public SeqRequestLogConfig Any(SeqRequestLogConfig req)
        {
            // resolve request logger
            var logger = TryResolve<IRequestLogger>() as SeqRequestLogger;
            if (logger == null) throw new Exception("Could not resolve SeqRequestLogger");

            // restrict permissions to roles if configured
            if (logger.RequiredRoles.Any())
            {
                var session = GetSession();
                if (session != null && !logger.RequiredRoles.Any(t => session.HasRole(t, base.AuthRepository)))
                {
                    return null;
                }
            }

            if (req.Enabled.HasValue) logger.Enabled = req.Enabled.Value;
            if (req.EnableErrorTracking.HasValue) logger.EnableErrorTracking = req.EnableErrorTracking.Value;
            if (req.EnableRequestBodyTracking.HasValue) logger.EnableRequestBodyTracking = req.EnableRequestBodyTracking.Value;
            if (req.EnableResponseTracking.HasValue) 
            {
                if (req.EnableRequestBodyTracking != null && !req.EnableRequestBodyTracking.Value)
                    logger.EnableResponseTracking = req.EnableResponseTracking.Value;
                else
                {
                    if (HostContext.GetPlugin<SeqRequestLogsFeature>().EnableResponseTracking)
                    {
                        logger.EnableResponseTracking = true;
                    }
                    else
                    {
                        throw new Exception(
                            "EnableResponseTracking cannot be enabled if not initially requested at AppHost startup. This feature requires PreRequestFilters");
                    }
                }
            }

            if (req.EnableSessionTracking.HasValue) logger.EnableSessionTracking = req.EnableSessionTracking.Value;
            return logger.ConvertTo<SeqRequestLogConfig>();
        }
    }
}
