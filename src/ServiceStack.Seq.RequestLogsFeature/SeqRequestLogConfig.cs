// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    [Route("/SeqRequestLogConfig")]
    public class SeqRequestLogConfig : IReturn<SeqRequestLogConfig>
    {
        public bool? Enabled { get; set; }

        public bool? EnableSessionTracking { get; set; }

        public bool? EnableRequestBodyTracking { get; set; }

        public bool? EnableResponseTracking { get; set; }

        public bool? EnableErrorTracking { get; set; }
    }
}
