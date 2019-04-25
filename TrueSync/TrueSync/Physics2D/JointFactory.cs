namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class JointFactory
    {
        public static AngleJoint CreateAngleJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB)
        {
            AngleJoint joint = new AngleJoint(bodyA, bodyB);
            world.AddJoint(joint);
            return joint;
        }

        public static DistanceJoint CreateDistanceJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB)
        {
            return CreateDistanceJoint(world, bodyA, bodyB, TSVector2.zero, TSVector2.zero, false);
        }

        public static DistanceJoint CreateDistanceJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false)
        {
            DistanceJoint joint = new DistanceJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }

        public static FixedMouseJoint CreateFixedMouseJoint(TrueSync.Physics2D.World world, Body body, TSVector2 worldAnchor)
        {
            FixedMouseJoint joint = new FixedMouseJoint(body, worldAnchor);
            world.AddJoint(joint);
            return joint;
        }

        public static FrictionJoint CreateFrictionJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB)
        {
            return CreateFrictionJoint(world, bodyA, bodyB, TSVector2.zero, false);
        }

        public static FrictionJoint CreateFrictionJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchor, bool useWorldCoordinates = false)
        {
            FrictionJoint joint = new FrictionJoint(bodyA, bodyB, anchor, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }

        public static GearJoint CreateGearJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TrueSync.Physics2D.Joint jointA, TrueSync.Physics2D.Joint jointB, FP ratio)
        {
            GearJoint joint = new GearJoint(bodyA, bodyB, jointA, jointB, ratio);
            world.AddJoint(joint);
            return joint;
        }

        public static MotorJoint CreateMotorJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, bool useWorldCoordinates = false)
        {
            MotorJoint joint = new MotorJoint(bodyA, bodyB, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }

        public static TrueSync.Physics2D.PrismaticJoint CreatePrismaticJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchor, TSVector2 axis, bool useWorldCoordinates = false)
        {
            TrueSync.Physics2D.PrismaticJoint joint = new TrueSync.Physics2D.PrismaticJoint(bodyA, bodyB, anchor, axis, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }

        public static PulleyJoint CreatePulleyJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, TSVector2 worldAnchorA, TSVector2 worldAnchorB, FP ratio, bool useWorldCoordinates = false)
        {
            PulleyJoint joint = new PulleyJoint(bodyA, bodyB, anchorA, anchorB, worldAnchorA, worldAnchorB, ratio, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }

        public static RevoluteJoint CreateRevoluteJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchor)
        {
            TSVector2 localPoint = bodyA.GetLocalPoint(bodyB.GetWorldPoint(anchor));
            RevoluteJoint joint = new RevoluteJoint(bodyA, bodyB, localPoint, anchor, false);
            world.AddJoint(joint);
            return joint;
        }

        public static RevoluteJoint CreateRevoluteJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false)
        {
            RevoluteJoint joint = new RevoluteJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }

        public static RopeJoint CreateRopeJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false)
        {
            RopeJoint joint = new RopeJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }

        public static WeldJoint CreateWeldJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false)
        {
            WeldJoint joint = new WeldJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }

        public static WheelJoint CreateWheelJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 axis)
        {
            return CreateWheelJoint(world, bodyA, bodyB, TSVector2.zero, axis, false);
        }

        public static WheelJoint CreateWheelJoint(TrueSync.Physics2D.World world, Body bodyA, Body bodyB, TSVector2 anchor, TSVector2 axis, bool useWorldCoordinates = false)
        {
            WheelJoint joint = new WheelJoint(bodyA, bodyB, anchor, axis, useWorldCoordinates);
            world.AddJoint(joint);
            return joint;
        }
    }
}

