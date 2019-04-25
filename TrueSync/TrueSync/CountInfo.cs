namespace TrueSync
{
    using System;

    public class CountInfo
    {
        public long count;
        public long sum;

        public float average()
        {
            if (this.count == 0L)
            {
                return 0f;
            }
            return (((float) this.sum) / ((float) this.count));
        }

        public string averageFormatted()
        {
            return this.average().ToString("F2");
        }
    }
}

