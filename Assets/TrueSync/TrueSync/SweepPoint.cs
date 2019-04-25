namespace TrueSync
{
    using System;

    public class SweepPoint
    {
        public int Axis;
        public bool Begin;
        public IBroadphaseEntity Body;

        public SweepPoint(IBroadphaseEntity body, bool begin, int axis)
        {
            this.Body = body;
            this.Begin = begin;
            this.Axis = axis;
        }

        public FP Value
        {
            get
            {
                if (this.Begin)
                {
                    if (this.Axis == 0)
                    {
                        return this.Body.BoundingBox.min.x;
                    }
                    if (this.Axis == 1)
                    {
                        return this.Body.BoundingBox.min.y;
                    }
                    return this.Body.BoundingBox.min.z;
                }
                if (this.Axis == 0)
                {
                    return this.Body.BoundingBox.max.x;
                }
                if (this.Axis == 1)
                {
                    return this.Body.BoundingBox.max.y;
                }
                return this.Body.BoundingBox.max.z;
            }
        }
    }
}

