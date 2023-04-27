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

using System.Linq;

namespace Nofun.Util
{
    public class LTUFixedCapCache<T>: DictBasedCache<T> where T : ICacheEntry
    {
        private int cacheLimit;

        protected override void AddToCache(uint key, T entry)
        {
            if (cache.Count >= cacheLimit)
            {
                Purge();
            }

            if (!cache.ContainsKey(key))
            {
                cache.Add(key, entry);
            }
        }

        public LTUFixedCapCache(int cacheLimit = 4096)
            : base()
        {
            this.cacheLimit = cacheLimit;
        }

        /// <summary>
        /// Purge half of the cache, sorted by oldest time since used.
        /// </summary>
        public override void Purge()
        {
            var purgeList = cache.OrderBy(x => x.Value.LastAccessed).Select(x => x.Key).ToList();
            for (int i = 0; i < purgeList.Count / 2; i++)
            {
                cache.Remove(purgeList[i]);
            }
        }
    }
}