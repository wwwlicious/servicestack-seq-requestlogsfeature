// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System.Collections.Generic;

    /// <summary>
    /// A log entry added by the IRequestLogger
    /// </summary>
    public class SeqRequestLogEntry
    {
        public SeqRequestLogEntry()
        {
            MessageTemplate = "Servicestack SeqRequestLogsFeature";
            Properties = new Dictionary<string, object>();
            Level = "Debug";
        }

        public string Timestamp { get; set; }

        public string Level { get; set; }

        public Dictionary<string, object> Properties { get; }

        public string MessageTemplate { get; set; }
    }
}