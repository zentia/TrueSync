namespace TrueSync
{
    using System;

    public class CompoundStats
    {
        private const float BUFFER_LIFETIME = 2f;
        private const int BUFFER_WINDOW = 10;
        public GenericBufferWindow<Stats> bufferStats = new GenericBufferWindow<Stats>(10);
        public Stats globalStats = new Stats();
        private FP timerAcc = 0;

        public void AddValue(string key, long value)
        {
            this.bufferStats.Current().AddValue(key, value);
            this.globalStats.AddValue(key, value);
        }

        public void Increment(string key)
        {
            this.bufferStats.Current().Increment(key);
            this.globalStats.Increment(key);
        }

        public void UpdateTime(FP time)
        {
            this.timerAcc += time;
            if (this.timerAcc >= 2f)
            {
                this.bufferStats.MoveNext();
                this.bufferStats.Current().Clear();
                this.timerAcc = 0;
            }
        }
    }
}

