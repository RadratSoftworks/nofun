/*
 * (C) 2023 Radrat Softworks
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;

namespace Nofun.Util
{
    /// <summary>
    /// A cache that allow prune all entries that have not been used for a specified amount of time.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LTUHardPruneCache<T> : LTUFixedCapCache<T> where T : ICacheEntry
    {
        private int unusedDurationInSecondsToPrune;

        public LTUHardPruneCache(int limit = 100, int unusedDurationInSecondsToPrune = 240)
            : base(limit)
        {
            this.unusedDurationInSecondsToPrune = unusedDurationInSecondsToPrune;
        }

        /// <summary>
        /// Purge half of the cache, sorted by oldest time since used.
        /// </summary>
        public void PrunePurge()
        {
            var now = DateTime.Now;
            var purgeList = cache.Where(entry => (now - entry.Value.LastAccessed).Seconds >= unusedDurationInSecondsToPrune).ToList();

            foreach (var purgable in purgeList)
            {
                cache.Remove(purgable.Key);
            }
        }
    }
}