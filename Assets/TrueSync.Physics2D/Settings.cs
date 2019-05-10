using System;

namespace TrueSync.Physics2D
{
	public static class Settings
	{
		public static readonly FP MaxFP = FP.MaxValue;

		public static readonly FP Epsilon = 1.1920929E-07f;

		public static readonly FP EpsilonSqr = Settings.Epsilon * Settings.Epsilon;

		public static readonly FP Pi = FP.Pi;

		public const bool AllCollisionCallbacksAgree = true;

		public const bool EnableDiagnostics = false;

		public const bool SkipSanityChecks = false;

		public static int VelocityIterations = 8;

		public static int PositionIterations = 3;

		public static bool ContinuousPhysics = false;

		public static bool UseConvexHullPolygons = false;

		public static int TOIVelocityIterations = Settings.VelocityIterations;

		public static int TOIPositionIterations = 20;

		public const int MaxSubSteps = 8;

		public const bool EnableWarmstarting = true;

		public static bool AllowSleep = false;

		public static int MaxPolygonVertices = 64;

		public static bool UseFPECollisionCategories;

		public static Category DefaultFixtureCollisionCategories = Category.Cat1;

		public static Category DefaultFixtureCollidesWith = Category.All;

		public static Category DefaultFixtureIgnoreCCDWith = Category.None;

		public const int MaxManifoldPoints = 2;

		public static readonly FP AABBExtension = 0.1f;

		public static readonly FP AABBMultiplier = 2;

		public static readonly FP LinearSlop = 0.005f;

		public static readonly FP AngularSlop = 0.0111111114f * Settings.Pi;

		public static readonly FP PolygonRadius = 2f * Settings.LinearSlop;

		public const int MaxTOIContacts = 32;

		public static readonly FP VelocityThreshold = 1f;

		public static readonly FP MaxLinearCorrection = 0.2f;

		public static readonly FP MaxAngularCorrection = 0.0444444455f * Settings.Pi;

		public static readonly FP Baumgarte = 0.2f;

		public static readonly FP TimeToSleep = 0.5f;

		public static readonly FP LinearSleepTolerance = 0.01f;

		public static readonly FP AngularSleepTolerance = 0.0111111114f * Settings.Pi;

		public static readonly FP MaxTranslation = 2f;

		public static readonly FP MaxTranslationSquared = Settings.MaxTranslation * Settings.MaxTranslation;

		public static readonly FP MaxRotation = 0.5f * Settings.Pi;

		public static readonly FP MaxRotationSquared = Settings.MaxRotation * Settings.MaxRotation;

		public const int MaxGJKIterations = 20;

		public const bool EnableSubStepping = false;

		public const bool AutoClearForces = true;

		public static FP MixFriction(FP friction1, FP friction2)
		{
			return FP.Sqrt(friction1 * friction2);
		}

		public static FP MixRestitution(FP restitution1, FP restitution2)
		{
			return (restitution1 > restitution2) ? restitution1 : restitution2;
		}
	}
}
