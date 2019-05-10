using System;

namespace TrueSync.Physics2D
{
	public static class JointFactory
	{
		public static MotorJoint CreateMotorJoint(World world, Body bodyA, Body bodyB, bool useWorldCoordinates = false)
		{
			MotorJoint motorJoint = new MotorJoint(bodyA, bodyB, useWorldCoordinates);
			world.AddJoint(motorJoint);
			return motorJoint;
		}

		public static RevoluteJoint CreateRevoluteJoint(World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false)
		{
			RevoluteJoint revoluteJoint = new RevoluteJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
			world.AddJoint(revoluteJoint);
			return revoluteJoint;
		}

		public static RevoluteJoint CreateRevoluteJoint(World world, Body bodyA, Body bodyB, TSVector2 anchor)
		{
			TSVector2 localPoint = bodyA.GetLocalPoint(bodyB.GetWorldPoint(anchor));
			RevoluteJoint revoluteJoint = new RevoluteJoint(bodyA, bodyB, localPoint, anchor, false);
			world.AddJoint(revoluteJoint);
			return revoluteJoint;
		}

		public static RopeJoint CreateRopeJoint(World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false)
		{
			RopeJoint ropeJoint = new RopeJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
			world.AddJoint(ropeJoint);
			return ropeJoint;
		}

		public static WeldJoint CreateWeldJoint(World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false)
		{
			WeldJoint weldJoint = new WeldJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
			world.AddJoint(weldJoint);
			return weldJoint;
		}

		public static PrismaticJoint CreatePrismaticJoint(World world, Body bodyA, Body bodyB, TSVector2 anchor, TSVector2 axis, bool useWorldCoordinates = false)
		{
			PrismaticJoint prismaticJoint = new PrismaticJoint(bodyA, bodyB, anchor, axis, useWorldCoordinates);
			world.AddJoint(prismaticJoint);
			return prismaticJoint;
		}

		public static WheelJoint CreateWheelJoint(World world, Body bodyA, Body bodyB, TSVector2 anchor, TSVector2 axis, bool useWorldCoordinates = false)
		{
			WheelJoint wheelJoint = new WheelJoint(bodyA, bodyB, anchor, axis, useWorldCoordinates);
			world.AddJoint(wheelJoint);
			return wheelJoint;
		}

		public static WheelJoint CreateWheelJoint(World world, Body bodyA, Body bodyB, TSVector2 axis)
		{
			return JointFactory.CreateWheelJoint(world, bodyA, bodyB, TSVector2.zero, axis, false);
		}

		public static AngleJoint CreateAngleJoint(World world, Body bodyA, Body bodyB)
		{
			AngleJoint angleJoint = new AngleJoint(bodyA, bodyB);
			world.AddJoint(angleJoint);
			return angleJoint;
		}

		public static DistanceJoint CreateDistanceJoint(World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false)
		{
			DistanceJoint distanceJoint = new DistanceJoint(bodyA, bodyB, anchorA, anchorB, useWorldCoordinates);
			world.AddJoint(distanceJoint);
			return distanceJoint;
		}

		public static DistanceJoint CreateDistanceJoint(World world, Body bodyA, Body bodyB)
		{
			return JointFactory.CreateDistanceJoint(world, bodyA, bodyB, TSVector2.zero, TSVector2.zero, false);
		}

		public static FrictionJoint CreateFrictionJoint(World world, Body bodyA, Body bodyB, TSVector2 anchor, bool useWorldCoordinates = false)
		{
			FrictionJoint frictionJoint = new FrictionJoint(bodyA, bodyB, anchor, useWorldCoordinates);
			world.AddJoint(frictionJoint);
			return frictionJoint;
		}

		public static FrictionJoint CreateFrictionJoint(World world, Body bodyA, Body bodyB)
		{
			return JointFactory.CreateFrictionJoint(world, bodyA, bodyB, TSVector2.zero, false);
		}

		public static GearJoint CreateGearJoint(World world, Body bodyA, Body bodyB, Joint jointA, Joint jointB, FP ratio)
		{
			GearJoint gearJoint = new GearJoint(bodyA, bodyB, jointA, jointB, ratio);
			world.AddJoint(gearJoint);
			return gearJoint;
		}

		public static PulleyJoint CreatePulleyJoint(World world, Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, TSVector2 worldAnchorA, TSVector2 worldAnchorB, FP ratio, bool useWorldCoordinates = false)
		{
			PulleyJoint pulleyJoint = new PulleyJoint(bodyA, bodyB, anchorA, anchorB, worldAnchorA, worldAnchorB, ratio, useWorldCoordinates);
			world.AddJoint(pulleyJoint);
			return pulleyJoint;
		}

		public static FixedMouseJoint CreateFixedMouseJoint(World world, Body body, TSVector2 worldAnchor)
		{
			FixedMouseJoint fixedMouseJoint = new FixedMouseJoint(body, worldAnchor);
			world.AddJoint(fixedMouseJoint);
			return fixedMouseJoint;
		}
	}
}
