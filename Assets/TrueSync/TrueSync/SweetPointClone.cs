namespace TrueSync
{
    using System;

    public class SweetPointClone
    {
        public int axis;
        public bool begin;
        public IBroadphaseEntity body;

        public void Clone(SweepPoint sp)
        {
            this.body = sp.Body;
            this.begin = sp.Begin;
            this.axis = sp.Axis;
        }

        public void Restore(SweepPoint sp)
        {
            sp.Body = this.body;
            sp.Begin = this.begin;
            sp.Axis = this.axis;
        }
    }
}

