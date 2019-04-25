namespace TrueSync.Physics2D
{
    using System;

    internal class Sink : Node
    {
        public TrueSync.Physics2D.Trapezoid Trapezoid;

        private Sink(TrueSync.Physics2D.Trapezoid trapezoid) : base(null, null)
        {
            this.Trapezoid = trapezoid;
            trapezoid.Sink = this;
        }

        public static Sink Isink(TrueSync.Physics2D.Trapezoid trapezoid)
        {
            if (trapezoid.Sink == null)
            {
                return new Sink(trapezoid);
            }
            return trapezoid.Sink;
        }

        public override Sink Locate(TrueSync.Physics2D.Edge edge)
        {
            return this;
        }
    }
}

