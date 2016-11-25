namespace ServiceStack.Seq.RequestLogsFeature
{
    public static class ConfigKeys
    {
        private const string keyPrefix = "servicestack.seq.requestlogs.";

        public static string SeqUrl => $"{keyPrefix}.seq.url";
        public static string ApiKey => $"{keyPrefix}.seq.apikey";
        public static string Enabled => $"{keyPrefix}.enabled";
        public static string EnableErrorTracking => $"{keyPrefix}.errortracking.enabled";
        public static string EnableRequestBodyTracking => $"{keyPrefix}.requestbodytracking.enabled";
        public static string EnableSessionTracking => $"{keyPrefix}.sessiontracking.enabled";
        public static string EnableResponseTracking => $"{keyPrefix}.responsetracking.enabled";
    }
}