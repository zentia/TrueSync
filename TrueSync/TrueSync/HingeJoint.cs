namespace TrueSync
{
    using System;

    public class HingeJoint : Joint
    {
        private PointOnPoint[] worldPointConstraint;

        public HingeJoint(World world, RigidBody body1, RigidBody body2, TSVector position, TSVector hingeAxis) : base(world)
        {
            this.worldPointConstraint = new PointOnPoint[2];
            hingeAxis *= FP.Half;
            TSVector vector = position;
            TSVector.Add(ref vector, ref hingeAxis, out vector);
            TSVector vector2 = position;
            TSVector.Subtract(ref vector2, ref hingeAxis, out vector2);
            this.worldPointConstraint[0] = new PointOnPoint(body1, body2, vector);
            this.worldPointConstraint[1] = new PointOnPoint(body1, body2, vector2);
        }

        public override void Activate()
        {
            base.World.AddConstraint(this.worldPointConstraint[0]);
            base.World.AddConstraint(this.worldPointConstraint[1]);
        }

        public override void Deactivate()
        {
            base.World.RemoveConstraint(this.worldPointConstraint[0]);
            base.World.RemoveConstraint(this.worldPointConstraint[1]);
        }

        public FP AppliedImpulse
        {
            get
            {
                return (this.worldPointConstraint[0].AppliedImpulse + this.worldPointConstraint[1].AppliedImpulse);
            }
        }

        public PointOnPoint PointOnPointConstraint1
        {
            get
            {
                return this.worldPointConstraint[0];
            }
        }

        public PointOnPoint PointOnPointConstraint2
        {
            get
            {
                return this.worldPointConstraint[1];
            }
        }
    }
}

