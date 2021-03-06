// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System.Collections.Generic;

    public static class LogExtensions
    {
        public static Dictionary<string, object> WithoutDuplicates(this Dictionary<string, object> items)
        {
            items.Remove("__session");
            items.Remove("_requestDurationStopwatch");
            items.Remove("x-mac-requestId");
            return items;
        }
    }
}