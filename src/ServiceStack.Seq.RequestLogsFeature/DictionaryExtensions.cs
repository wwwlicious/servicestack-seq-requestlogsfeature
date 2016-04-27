// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ServiceStack.Seq.RequestLogsFeature
{
    using System;
    using System.Collections.Generic;

    public static class DictionaryExtensions
    {
        public static V GetOrAdd<K, V>(this SortedDictionary<K, V> map, K key, Func<K, V> createFn)
        {
            //simulate ConcurrentDictionary.GetOrAdd
            lock (map)
            {
                V val;
                if (!map.TryGetValue(key, out val))
                    map[key] = val = createFn(key);

                return val;
            }
        }
    }
}