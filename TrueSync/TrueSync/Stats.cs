namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public class Stats
    {
        private Dictionary<string, CountInfo> counts = new Dictionary<string, CountInfo>();
        private static CountInfo emptyInfo = new CountInfo();

        public void AddValue(string key, long value)
        {
            this.Increment(key);
            CountInfo local1 = counts[key];
            local1.sum += value;
        }

        public void Clear()
        {
            this.counts.Clear();
        }

        public CountInfo GetInfo(string key)
        {
            if (this.counts.ContainsKey(key))
            {
                return this.counts[key];
            }
            return emptyInfo;
        }

        public void Increment(string key)
        {
            if (!counts.ContainsKey(key))
            {
                this.counts[key] = new CountInfo();
            }
            CountInfo local1 = this.counts[key];
            local1.count += 1L;
        }
    }
}

