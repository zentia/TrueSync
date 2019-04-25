namespace TrueSync
{
    using System;

    public class PrismaticJoint : Joint
    {
        private FixedAngle fixedAngle;
        private PointPointDistance maxDistance;
        private PointPointDistance minDistance;
        private PointOnLine pointOnLine;

        public PrismaticJoint(World world, RigidBody body1, RigidBody body2) : base(world)
        {
            this.minDistance = null;
            this.maxDistance = null;
            this.fixedAngle = new FixedAngle(body1, body2);
            this.pointOnLine = new PointOnLine(body1, body2, body1.position, body2.position);
        }

        public PrismaticJoint(World world, RigidBody body1, RigidBody body2, FP minimumDistance, FP maximumDistance) : base(world)
        {
            this.minDistance = null;
            this.maxDistance = null;
            this.fixedAngle = new FixedAngle(body1, body2);
            this.pointOnLine = new PointOnLine(body1, body2, body1.position, body2.position);
            this.minDistance = new PointPointDistance(body1, body2, body1.position, body2.position);
            this.minDistance.Behavior = PointPointDistance.DistanceBehavior.LimitMinimumDistance;
            this.minDistance.Distance = minimumDistance;
            this.maxDistance = new PointPointDistance(body1, body2, body1.position, body2.position);
            this.maxDistance.Behavior = PointPointDistance.DistanceBehavior.LimitMaximumDistance;
            this.maxDistance.Distance = maximumDistance;
        }

        public PrismaticJoint(World world, RigidBody body1, RigidBody body2, TSVector pointOnBody1, TSVector pointOnBody2) : base(world)
        {
            this.minDistance = null;
            this.maxDistance = null;
            this.fixedAngle = new FixedAngle(body1, body2);
            this.pointOnLine = new PointOnLine(body1, body2, pointOnBody1, pointOnBody2);
        }

        public PrismaticJoint(World world, RigidBody body1, RigidBody body2, TSVector pointOnBody1, TSVector pointOnBody2, FP maximumDistance, FP minimumDistance) : base(world)
        {
            this.minDistance = null;
            this.maxDistance = null;
            this.fixedAngle = new FixedAngle(body1, body2);
            this.pointOnLine = new PointOnLine(body1, body2, pointOnBody1, pointOnBody2);
        }

        public override void Activate()
        {
            if (this.maxDistance > null)
            {
                base.World.AddConstraint(this.maxDistance);
            }
            if (this.minDistance > null)
            {
                base.World.AddConstraint(this.minDistance);
            }
            base.World.AddConstraint(this.fixedAngle);
            base.World.AddConstraint(this.pointOnLine);
        }

        public override void Deactivate()
        {
            if (this.maxDistance > null)
            {
                base.World.RemoveConstraint(this.maxDistance);
            }
            if (this.minDistance > null)
            {
                base.World.RemoveConstraint(this.minDistance);
            }
            base.World.RemoveConstraint(this.fixedAngle);
            base.World.RemoveConstraint(this.pointOnLine);
        }

        public FixedAngle FixedAngleConstraint
        {
            get
            {
                return this.fixedAngle;
            }
        }

        public PointPointDistance MaximumDistanceConstraint
        {
            get
            {
                return this.maxDistance;
            }
        }

        public PointPointDistance MinimumDistanceConstraint
        {
            get
            {
                return this.minDistance;
            }
        }

        public PointOnLine PointOnLineConstraint
        {
            get
            {
                return this.pointOnLine;
            }
        }
    }
}

