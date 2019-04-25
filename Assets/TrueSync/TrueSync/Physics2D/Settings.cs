namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    public static class Settings
    {
        public static readonly FP AABBExtension = 0.1f;
        public static readonly FP AABBMultiplier = 2;
        public const bool AllCollisionCallbacksAgree = true;
        public static bool AllowSleep = false;
        public static readonly FP AngularSleepTolerance = (0.01111111f * Pi);
        public static readonly FP AngularSlop = (0.01111111f * Pi);
        public const bool AutoClearForces = true;
        public static readonly FP Baumgarte = 0.2f;
        public static bool ContinuousPhysics = false;
        public static Category DefaultFixtureCollidesWith = Category.All;
        public static Category DefaultFixtureCollisionCategories = Category.Cat1;
        public static Category DefaultFixtureIgnoreCCDWith = Category.None;
        public const bool EnableDiagnostics = false;
        public const bool EnableSubStepping = false;
        public const bool EnableWarmstarting = true;
        public static readonly FP Epsilon = 1.192093E-07f;
        public static readonly FP EpsilonSqr = (Epsilon * Epsilon);
        public static readonly FP LinearSleepTolerance = 0.01f;
        public static readonly FP LinearSlop = 0.005f;
        public static readonly FP MaxAngularCorrection = (0.04444445f * Pi);
        public static readonly FP MaxFP = FP.MaxValue;
        public const int MaxGJKIterations = 20;
        public static readonly FP MaxLinearCorrection = 0.2f;
        public const int MaxManifoldPoints = 2;
        public static int MaxPolygonVertices = 0x40;
        public static readonly FP MaxRotation = (0.5f * Pi);
        public static readonly FP MaxRotationSquared = (MaxRotation * MaxRotation);
        public const int MaxSubSteps = 8;
        public const int MaxTOIContacts = 0x20;
        public static readonly FP MaxTranslation = 2f;
        public static readonly FP MaxTranslationSquared = (MaxTranslation * MaxTranslation);
        public static readonly FP Pi = FP.Pi;
        public static readonly FP PolygonRadius = (2f * LinearSlop);
        public static int PositionIterations = 3;
        public const bool SkipSanityChecks = false;
        public static readonly FP TimeToSleep = 0.5f;
        public static int TOIPositionIterations = 20;
        public static int TOIVelocityIterations = VelocityIterations;
        public static bool UseConvexHullPolygons = false;
        public static bool UseFPECollisionCategories;
        public static int VelocityIterations = 8;
        public static readonly FP VelocityThreshold = 1f;

        public static FP MixFriction(FP friction1, FP friction2)
        {
            return FP.Sqrt(friction1 * friction2);
        }

        public static FP MixRestitution(FP restitution1, FP restitution2)
        {
            return ((restitution1 > restitution2) ? restitution1 : restitution2);
        }
    }
}

