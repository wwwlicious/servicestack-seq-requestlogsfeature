// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    public static class ConfigKeys
    {
        private const string KeyPrefix = "servicestack.seq.requestlogs.";

        public static string SeqUrl => $"{KeyPrefix}seq.url";
        public static string ApiKey => $"{KeyPrefix}seq.apikey";
        public static string Enabled => $"{KeyPrefix}enabled";
        public static string EnableErrorTracking => $"{KeyPrefix}errortracking.enabled";
        public static string EnableRequestBodyTracking => $"{KeyPrefix}requestbodytracking.enabled";
        public static string EnableSessionTracking => $"{KeyPrefix}sessiontracking.enabled";
        public static string EnableResponseTracking => $"{KeyPrefix}responsetracking.enabled";
        public static string RequiredRoles => $"{KeyPrefix}requiredroles";
    }
}