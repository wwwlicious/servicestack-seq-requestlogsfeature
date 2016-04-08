// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    public class SeqRequestLogsFeature : IPlugin
    {
        public SeqRequestLogsFeature(SeqRequestLogsSettings settings)
        {
            Settings = settings;
        }

        public SeqRequestLogsSettings Settings { get; private set; }
       
        public void Register(IAppHost appHost)
        {
            var requestLogger = Settings.GetLogger();
            requestLogger.EnableSessionTracking = Settings.GetEnableSessionTracking();
            requestLogger.EnableResponseTracking = Settings.GetEnableResponseTracking();
            requestLogger.EnableRequestBodyTracking = Settings.GetEnableRequestBodyTracking();
            requestLogger.EnableErrorTracking = Settings.GetEnableErrorTracking();
            requestLogger.ExcludeRequestDtoTypes = Settings.GetExcludeRequestDtoTypes();
            requestLogger.HideRequestBodyForRequestDtoTypes = Settings.GetHideRequestBodyForRequestDtoTypes();

            appHost.Register(requestLogger);
            appHost.RegisterService(typeof(SeqRequestLogConfigService));
            if (Settings.GetEnableRequestBodyTracking())
            {
                appHost.PreRequestFilters.Insert(0, (httpReq, httpRes) =>
                {
                    httpReq.UseBufferedStream = Settings.GetEnableRequestBodyTracking();
                });
            }

            appHost.GetPlugin<MetadataFeature>()
                .AddDebugLink(Settings.GetUrl(), "Seq Request Logs")
                .AddPluginLink("/SeqRequestLogConfig", "Seq IRequestLogger Configuration");
        }
    }
}
