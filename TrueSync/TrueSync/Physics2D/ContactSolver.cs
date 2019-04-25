namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class ContactSolver
    {
        public TrueSync.Physics2D.Contact[] _contacts;
        public int _count;
        public ContactPositionConstraint[] _positionConstraints;
        public Position[] _positions;
        public TimeStep _step;
        public Velocity[] _velocities;
        public ContactVelocityConstraint[] _velocityConstraints;

        public void InitializeVelocityConstraints()
        {
            for (int i = 0; i < this._count; i++)
            {
                TSVector2 vector7;
                FixedArray2<TSVector2> array;
                ContactVelocityConstraint constraint = this._velocityConstraints[i];
                ContactPositionConstraint constraint2 = this._positionConstraints[i];
                FP radiusA = constraint2.radiusA;
                FP radiusB = constraint2.radiusB;
                Manifold manifold = this._contacts[constraint.contactIndex].Manifold;
                int indexA = constraint.indexA;
                int indexB = constraint.indexB;
                FP invMassA = constraint.invMassA;
                FP invMassB = constraint.invMassB;
                FP invIA = constraint.invIA;
                FP invIB = constraint.invIB;
                TSVector2 localCenterA = constraint2.localCenterA;
                TSVector2 localCenterB = constraint2.localCenterB;
                TSVector2 c = this._positions[indexA].c;
                FP a = this._positions[indexA].a;
                TSVector2 v = this._velocities[indexA].v;
                FP w = this._velocities[indexA].w;
                TSVector2 vector5 = this._positions[indexB].c;
                FP angle = this._positions[indexB].a;
                TSVector2 vector6 = this._velocities[indexB].v;
                FP s = this._velocities[indexB].w;
                Debug.Assert(manifold.PointCount > 0);
                Transform xfA = new Transform();
                Transform xfB = new Transform();
                xfA.q.Set(a);
                xfB.q.Set(angle);
                xfA.p = c - MathUtils.Mul(xfA.q, localCenterA);
                xfB.p = vector5 - MathUtils.Mul(xfB.q, localCenterB);
                WorldManifold.Initialize(ref manifold, ref xfA, radiusA, ref xfB, radiusB, out vector7, out array);
                constraint.normal = vector7;
                int pointCount = constraint.pointCount;
                for (int j = 0; j < pointCount; j++)
                {
                    VelocityConstraintPoint point = constraint.points[j];
                    point.rA = array[j] - c;
                    point.rB = array[j] - vector5;
                    FP fp11 = MathUtils.Cross(point.rA, constraint.normal);
                    FP fp12 = MathUtils.Cross(point.rB, constraint.normal);
                    FP fp13 = ((invMassA + invMassB) + ((invIA * fp11) * fp11)) + ((invIB * fp12) * fp12);
                    point.normalMass = (fp13 > 0f) ? (1f / fp13) : 0f;
                    TSVector2 b = MathUtils.Cross(constraint.normal, 1f);
                    FP fp14 = MathUtils.Cross(point.rA, b);
                    FP fp15 = MathUtils.Cross(point.rB, b);
                    FP fp16 = ((invMassA + invMassB) + ((invIA * fp14) * fp14)) + ((invIB * fp15) * fp15);
                    point.tangentMass = (fp16 > 0f) ? (1f / fp16) : 0f;
                    point.velocityBias = 0f;
                    FP fp17 = TSVector2.Dot(constraint.normal, ((vector6 + MathUtils.Cross(s, point.rB)) - v) - MathUtils.Cross(w, point.rA));
                    if (fp17 < -Settings.VelocityThreshold)
                    {
                        point.velocityBias = -constraint.restitution * fp17;
                    }
                }
                if (constraint.pointCount == 2)
                {
                    VelocityConstraintPoint point2 = constraint.points[0];
                    VelocityConstraintPoint point3 = constraint.points[1];
                    FP fp18 = MathUtils.Cross(point2.rA, constraint.normal);
                    FP fp19 = MathUtils.Cross(point2.rB, constraint.normal);
                    FP fp20 = MathUtils.Cross(point3.rA, constraint.normal);
                    FP fp21 = MathUtils.Cross(point3.rB, constraint.normal);
                    FP x = ((invMassA + invMassB) + ((invIA * fp18) * fp18)) + ((invIB * fp19) * fp19);
                    FP y = ((invMassA + invMassB) + ((invIA * fp20) * fp20)) + ((invIB * fp21) * fp21);
                    FP fp24 = ((invMassA + invMassB) + ((invIA * fp18) * fp20)) + ((invIB * fp19) * fp21);
                    FP fp25 = 1000f;
                    if ((x * x) < (fp25 * ((x * y) - (fp24 * fp24))))
                    {
                        constraint.K.ex = new TSVector2(x, fp24);
                        constraint.K.ey = new TSVector2(fp24, y);
                        constraint.normalMass = constraint.K.Inverse;
                    }
                    else
                    {
                        constraint.pointCount = 1;
                    }
                }
            }
        }

        public void Reset(TimeStep step, int count, TrueSync.Physics2D.Contact[] contacts, Position[] positions, Velocity[] velocities, bool warmstarting = true)
        {
            this._step = step;
            this._count = count;
            this._positions = positions;
            this._velocities = velocities;
            this._contacts = contacts;
            if ((this._velocityConstraints == null) || (this._velocityConstraints.Length < count))
            {
                this._velocityConstraints = new ContactVelocityConstraint[count * 2];
                this._positionConstraints = new ContactPositionConstraint[count * 2];
                for (int j = 0; j < this._velocityConstraints.Length; j++)
                {
                    this._velocityConstraints[j] = new ContactVelocityConstraint();
                }
                for (int k = 0; k < this._positionConstraints.Length; k++)
                {
                    this._positionConstraints[k] = new ContactPositionConstraint();
                }
            }
            for (int i = 0; i < this._count; i++)
            {
                TrueSync.Physics2D.Contact contact = contacts[i];
                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                TrueSync.Physics2D.Shape shape = fixtureA.Shape;
                TrueSync.Physics2D.Shape shape2 = fixtureB.Shape;
                FP radius = shape.Radius;
                FP fp2 = shape2.Radius;
                Body body = fixtureA.Body;
                Body body2 = fixtureB.Body;
                Manifold manifold = contact.Manifold;
                int pointCount = manifold.PointCount;
                Debug.Assert(pointCount > 0);
                ContactVelocityConstraint constraint = this._velocityConstraints[i];
                constraint.friction = contact.Friction;
                constraint.restitution = contact.Restitution;
                constraint.tangentSpeed = contact.TangentSpeed;
                constraint.indexA = body.IslandIndex;
                constraint.indexB = body2.IslandIndex;
                constraint.invMassA = body._invMass;
                constraint.invMassB = body2._invMass;
                constraint.invIA = body._invI;
                constraint.invIB = body2._invI;
                constraint.contactIndex = i;
                constraint.pointCount = pointCount;
                constraint.K.SetZero();
                constraint.normalMass.SetZero();
                ContactPositionConstraint constraint2 = this._positionConstraints[i];
                constraint2.indexA = body.IslandIndex;
                constraint2.indexB = body2.IslandIndex;
                constraint2.invMassA = body._invMass;
                constraint2.invMassB = body2._invMass;
                constraint2.localCenterA = body._sweep.LocalCenter;
                constraint2.localCenterB = body2._sweep.LocalCenter;
                constraint2.invIA = body._invI;
                constraint2.invIB = body2._invI;
                constraint2.localNormal = manifold.LocalNormal;
                constraint2.localPoint = manifold.LocalPoint;
                constraint2.pointCount = pointCount;
                constraint2.radiusA = radius;
                constraint2.radiusB = fp2;
                constraint2.type = manifold.Type;
                for (int m = 0; m < pointCount; m++)
                {
                    ManifoldPoint point = manifold.Points[m];
                    VelocityConstraintPoint point2 = constraint.points[m];
                    point2.normalImpulse = this._step.dtRatio * point.NormalImpulse;
                    point2.tangentImpulse = this._step.dtRatio * point.TangentImpulse;
                    point2.rA = TSVector2.zero;
                    point2.rB = TSVector2.zero;
                    point2.normalMass = 0f;
                    point2.tangentMass = 0f;
                    point2.velocityBias = 0f;
                    constraint2.localPoints[m] = point.LocalPoint;
                }
            }
        }

        public bool SolvePositionConstraints()
        {
            FP fp = 0f;
            for (int i = 0; i < this._count; i++)
            {
                ContactPositionConstraint pc = this._positionConstraints[i];
                int indexA = pc.indexA;
                int indexB = pc.indexB;
                TSVector2 localCenterA = pc.localCenterA;
                FP invMassA = pc.invMassA;
                FP invIA = pc.invIA;
                TSVector2 localCenterB = pc.localCenterB;
                FP invMassB = pc.invMassB;
                FP invIB = pc.invIB;
                int pointCount = pc.pointCount;
                TSVector2 c = this._positions[indexA].c;
                FP a = this._positions[indexA].a;
                TSVector2 vector4 = this._positions[indexB].c;
                FP angle = this._positions[indexB].a;
                for (int j = 0; j < pointCount; j++)
                {
                    TSVector2 vector5;
                    TSVector2 vector6;
                    FP fp8;
                    Transform xfA = new Transform();
                    Transform xfB = new Transform();
                    xfA.q.Set(a);
                    xfB.q.Set(angle);
                    xfA.p = c - MathUtils.Mul(xfA.q, localCenterA);
                    xfB.p = vector4 - MathUtils.Mul(xfB.q, localCenterB);
                    PositionSolverManifold.Initialize(pc, xfA, xfB, j, out vector5, out vector6, out fp8);
                    TSVector2 vector7 = vector6 - c;
                    TSVector2 vector8 = vector6 - vector4;
                    fp = TSMath.Min(fp, fp8);
                    FP fp9 = MathUtils.Clamp(Settings.Baumgarte * (fp8 + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0f);
                    FP fp10 = MathUtils.Cross(vector7, vector5);
                    FP fp11 = MathUtils.Cross(vector8, vector5);
                    FP fp12 = ((invMassA + invMassB) + ((invIA * fp10) * fp10)) + ((invIB * fp11) * fp11);
                    FP fp13 = (fp12 > 0f) ? (-fp9 / fp12) : 0f;
                    TSVector2 b = (TSVector2) (fp13 * vector5);
                    c -= invMassA * b;
                    a -= invIA * MathUtils.Cross(vector7, b);
                    vector4 += invMassB * b;
                    angle += invIB * MathUtils.Cross(vector8, b);
                }
                this._positions[indexA].c = c;
                this._positions[indexA].a = a;
                this._positions[indexB].c = vector4;
                this._positions[indexB].a = angle;
            }
            return (fp >= (-3f * Settings.LinearSlop));
        }

        public bool SolveTOIPositionConstraints(int toiIndexA, int toiIndexB)
        {
            FP fp = 0f;
            for (int i = 0; i < this._count; i++)
            {
                ContactPositionConstraint pc = this._positionConstraints[i];
                int indexA = pc.indexA;
                int indexB = pc.indexB;
                TSVector2 localCenterA = pc.localCenterA;
                TSVector2 localCenterB = pc.localCenterB;
                int pointCount = pc.pointCount;
                FP invMassA = 0f;
                FP invIA = 0f;
                if ((indexA == toiIndexA) || (indexA == toiIndexB))
                {
                    invMassA = pc.invMassA;
                    invIA = pc.invIA;
                }
                FP invMassB = 0f;
                FP invIB = 0f;
                if ((indexB == toiIndexA) || (indexB == toiIndexB))
                {
                    invMassB = pc.invMassB;
                    invIB = pc.invIB;
                }
                TSVector2 c = this._positions[indexA].c;
                FP a = this._positions[indexA].a;
                TSVector2 vector4 = this._positions[indexB].c;
                FP angle = this._positions[indexB].a;
                for (int j = 0; j < pointCount; j++)
                {
                    TSVector2 vector5;
                    TSVector2 vector6;
                    FP fp8;
                    Transform xfA = new Transform();
                    Transform xfB = new Transform();
                    xfA.q.Set(a);
                    xfB.q.Set(angle);
                    xfA.p = c - MathUtils.Mul(xfA.q, localCenterA);
                    xfB.p = vector4 - MathUtils.Mul(xfB.q, localCenterB);
                    PositionSolverManifold.Initialize(pc, xfA, xfB, j, out vector5, out vector6, out fp8);
                    TSVector2 vector7 = vector6 - c;
                    TSVector2 vector8 = vector6 - vector4;
                    fp = TSMath.Min(fp, fp8);
                    FP fp9 = MathUtils.Clamp(Settings.Baumgarte * (fp8 + Settings.LinearSlop), -Settings.MaxLinearCorrection, 0f);
                    FP fp10 = MathUtils.Cross(vector7, vector5);
                    FP fp11 = MathUtils.Cross(vector8, vector5);
                    FP fp12 = ((invMassA + invMassB) + ((invIA * fp10) * fp10)) + ((invIB * fp11) * fp11);
                    FP fp13 = (fp12 > 0f) ? (-fp9 / fp12) : 0f;
                    TSVector2 b = (TSVector2) (fp13 * vector5);
                    c -= invMassA * b;
                    a -= invIA * MathUtils.Cross(vector7, b);
                    vector4 += invMassB * b;
                    angle += invIB * MathUtils.Cross(vector8, b);
                }
                this._positions[indexA].c = c;
                this._positions[indexA].a = a;
                this._positions[indexB].c = vector4;
                this._positions[indexB].a = angle;
            }
            return (fp >= (-1.5f * Settings.LinearSlop));
        }

        public void SolveVelocityConstraints()
        {
            for (int i = 0; i < this._count; i++)
            {
                ContactVelocityConstraint constraint = this._velocityConstraints[i];
                int indexA = constraint.indexA;
                int indexB = constraint.indexB;
                FP invMassA = constraint.invMassA;
                FP invIA = constraint.invIA;
                FP invMassB = constraint.invMassB;
                FP invIB = constraint.invIB;
                int pointCount = constraint.pointCount;
                TSVector2 v = this._velocities[indexA].v;
                FP w = this._velocities[indexA].w;
                TSVector2 vector2 = this._velocities[indexB].v;
                FP s = this._velocities[indexB].w;
                TSVector2 normal = constraint.normal;
                TSVector2 vector4 = MathUtils.Cross(normal, 1f);
                FP friction = constraint.friction;
                Debug.Assert((pointCount == 1) || (pointCount == 2));
                for (int j = 0; j < pointCount; j++)
                {
                    VelocityConstraintPoint point = constraint.points[j];
                    TSVector2 vector5 = ((vector2 + MathUtils.Cross(s, point.rB)) - v) - MathUtils.Cross(w, point.rA);
                    FP fp8 = TSVector2.Dot(vector5, vector4) - constraint.tangentSpeed;
                    FP fp9 = point.tangentMass * -fp8;
                    FP high = friction * point.normalImpulse;
                    FP fp11 = MathUtils.Clamp(point.tangentImpulse + fp9, -high, high);
                    fp9 = fp11 - point.tangentImpulse;
                    point.tangentImpulse = fp11;
                    TSVector2 b = (TSVector2) (fp9 * vector4);
                    v -= invMassA * b;
                    w -= invIA * MathUtils.Cross(point.rA, b);
                    vector2 += invMassB * b;
                    s += invIB * MathUtils.Cross(point.rB, b);
                }
                if (constraint.pointCount == 1)
                {
                    VelocityConstraintPoint point2 = constraint.points[0];
                    TSVector2 vector7 = ((vector2 + MathUtils.Cross(s, point2.rB)) - v) - MathUtils.Cross(w, point2.rA);
                    FP fp12 = TSVector2.Dot(vector7, normal);
                    FP fp13 = -point2.normalMass * (fp12 - point2.velocityBias);
                    FP fp14 = TSMath.Max(point2.normalImpulse + fp13, 0f);
                    fp13 = fp14 - point2.normalImpulse;
                    point2.normalImpulse = fp14;
                    TSVector2 vector8 = (TSVector2) (fp13 * normal);
                    v -= invMassA * vector8;
                    w -= invIA * MathUtils.Cross(point2.rA, vector8);
                    vector2 += invMassB * vector8;
                    s += invIB * MathUtils.Cross(point2.rB, vector8);
                }
                else
                {
                    VelocityConstraintPoint point3 = constraint.points[0];
                    VelocityConstraintPoint point4 = constraint.points[1];
                    TSVector2 vector9 = new TSVector2(point3.normalImpulse, point4.normalImpulse);
                    Debug.Assert((vector9.x >= 0f) && (vector9.y >= 0f));
                    TSVector2 vector10 = ((vector2 + MathUtils.Cross(s, point3.rB)) - v) - MathUtils.Cross(w, point3.rA);
                    TSVector2 vector11 = ((vector2 + MathUtils.Cross(s, point4.rB)) - v) - MathUtils.Cross(w, point4.rA);
                    FP x = TSVector2.Dot(vector10, normal);
                    FP y = TSVector2.Dot(vector11, normal);
                    TSVector2 vector12 = new TSVector2 {
                        x = x - point3.velocityBias,
                        y = y - point4.velocityBias
                    };
                    vector12 -= MathUtils.Mul(ref constraint.K, vector9);
                    TSVector2 vector13 = -MathUtils.Mul(ref constraint.normalMass, vector12);
                    if ((vector13.x >= 0f) && (vector13.y >= 0f))
                    {
                        TSVector2 vector14 = vector13 - vector9;
                        TSVector2 vector15 = (TSVector2) (vector14.x * normal);
                        TSVector2 vector16 = (TSVector2) (vector14.y * normal);
                        v -= invMassA * (vector15 + vector16);
                        w -= invIA * (MathUtils.Cross(point3.rA, vector15) + MathUtils.Cross(point4.rA, vector16));
                        vector2 += invMassB * (vector15 + vector16);
                        s += invIB * (MathUtils.Cross(point3.rB, vector15) + MathUtils.Cross(point4.rB, vector16));
                        point3.normalImpulse = vector13.x;
                        point4.normalImpulse = vector13.y;
                    }
                    else
                    {
                        vector13.x = -point3.normalMass * vector12.x;
                        vector13.y = 0f;
                        x = 0f;
                        y = (constraint.K.ex.y * vector13.x) + vector12.y;
                        if ((vector13.x >= 0f) && (y >= 0f))
                        {
                            TSVector2 vector17 = vector13 - vector9;
                            TSVector2 vector18 = (TSVector2) (vector17.x * normal);
                            TSVector2 vector19 = (TSVector2) (vector17.y * normal);
                            v -= invMassA * (vector18 + vector19);
                            w -= invIA * (MathUtils.Cross(point3.rA, vector18) + MathUtils.Cross(point4.rA, vector19));
                            vector2 += invMassB * (vector18 + vector19);
                            s += invIB * (MathUtils.Cross(point3.rB, vector18) + MathUtils.Cross(point4.rB, vector19));
                            point3.normalImpulse = vector13.x;
                            point4.normalImpulse = vector13.y;
                        }
                        else
                        {
                            vector13.x = 0f;
                            vector13.y = -point4.normalMass * vector12.y;
                            x = (constraint.K.ey.x * vector13.y) + vector12.x;
                            y = 0f;
                            if ((vector13.y >= 0f) && (x >= 0f))
                            {
                                TSVector2 vector20 = vector13 - vector9;
                                TSVector2 vector21 = (TSVector2) (vector20.x * normal);
                                TSVector2 vector22 = (TSVector2) (vector20.y * normal);
                                v -= invMassA * (vector21 + vector22);
                                w -= invIA * (MathUtils.Cross(point3.rA, vector21) + MathUtils.Cross(point4.rA, vector22));
                                vector2 += invMassB * (vector21 + vector22);
                                s += invIB * (MathUtils.Cross(point3.rB, vector21) + MathUtils.Cross(point4.rB, vector22));
                                point3.normalImpulse = vector13.x;
                                point4.normalImpulse = vector13.y;
                            }
                            else
                            {
                                vector13.x = 0f;
                                vector13.y = 0f;
                                x = vector12.x;
                                y = vector12.y;
                                if ((x >= 0f) && (y >= 0f))
                                {
                                    TSVector2 vector23 = vector13 - vector9;
                                    TSVector2 vector24 = (TSVector2) (vector23.x * normal);
                                    TSVector2 vector25 = (TSVector2) (vector23.y * normal);
                                    v -= invMassA * (vector24 + vector25);
                                    w -= invIA * (MathUtils.Cross(point3.rA, vector24) + MathUtils.Cross(point4.rA, vector25));
                                    vector2 += invMassB * (vector24 + vector25);
                                    s += invIB * (MathUtils.Cross(point3.rB, vector24) + MathUtils.Cross(point4.rB, vector25));
                                    point3.normalImpulse = vector13.x;
                                    point4.normalImpulse = vector13.y;
                                }
                            }
                        }
                    }
                }
                this._velocities[indexA].v = v;
                this._velocities[indexA].w = w;
                this._velocities[indexB].v = vector2;
                this._velocities[indexB].w = s;
            }
        }

        public void StoreImpulses()
        {
            for (int i = 0; i < this._count; i++)
            {
                ContactVelocityConstraint constraint = this._velocityConstraints[i];
                Manifold manifold = this._contacts[constraint.contactIndex].Manifold;
                for (int j = 0; j < constraint.pointCount; j++)
                {
                    ManifoldPoint point = manifold.Points[j];
                    point.NormalImpulse = constraint.points[j].normalImpulse;
                    point.TangentImpulse = constraint.points[j].tangentImpulse;
                    manifold.Points[j] = point;
                }
                this._contacts[constraint.contactIndex].Manifold = manifold;
            }
        }

        public void WarmStart()
        {
            for (int i = 0; i < this._count; i++)
            {
                ContactVelocityConstraint constraint = this._velocityConstraints[i];
                int indexA = constraint.indexA;
                int indexB = constraint.indexB;
                FP invMassA = constraint.invMassA;
                FP invIA = constraint.invIA;
                FP invMassB = constraint.invMassB;
                FP invIB = constraint.invIB;
                int pointCount = constraint.pointCount;
                TSVector2 v = this._velocities[indexA].v;
                FP w = this._velocities[indexA].w;
                TSVector2 vector2 = this._velocities[indexB].v;
                FP fp6 = this._velocities[indexB].w;
                TSVector2 normal = constraint.normal;
                TSVector2 vector4 = MathUtils.Cross(normal, 1f);
                for (int j = 0; j < pointCount; j++)
                {
                    VelocityConstraintPoint point = constraint.points[j];
                    TSVector2 b = (TSVector2) ((point.normalImpulse * normal) + (point.tangentImpulse * vector4));
                    w -= invIA * MathUtils.Cross(point.rA, b);
                    v -= invMassA * b;
                    fp6 += invIB * MathUtils.Cross(point.rB, b);
                    vector2 += invMassB * b;
                }
                this._velocities[indexA].v = v;
                this._velocities[indexA].w = w;
                this._velocities[indexB].v = vector2;
                this._velocities[indexB].w = fp6;
            }
        }

        private static class PositionSolverManifold
        {
            public static void Initialize(ContactPositionConstraint pc, Transform xfA, Transform xfB, int index, out TSVector2 normal, out TSVector2 point, out FP separation)
            {
                Debug.Assert(pc.pointCount > 0);
                switch (pc.type)
                {
                    case ManifoldType.Circles:
                    {
                        TSVector2 vector = MathUtils.Mul(ref xfA, pc.localPoint);
                        TSVector2 vector2 = MathUtils.Mul(ref xfB, pc.localPoints[0]);
                        normal = vector2 - vector;
                        normal.Normalize();
                        point = (TSVector2) (0.5f * (vector + vector2));
                        separation = (TSVector2.Dot(vector2 - vector, normal) - pc.radiusA) - pc.radiusB;
                        break;
                    }
                    case ManifoldType.FaceA:
                    {
                        normal = MathUtils.Mul(xfA.q, pc.localNormal);
                        TSVector2 vector3 = MathUtils.Mul(ref xfA, pc.localPoint);
                        TSVector2 vector4 = MathUtils.Mul(ref xfB, pc.localPoints[index]);
                        separation = (TSVector2.Dot(vector4 - vector3, normal) - pc.radiusA) - pc.radiusB;
                        point = vector4;
                        break;
                    }
                    case ManifoldType.FaceB:
                    {
                        normal = MathUtils.Mul(xfB.q, pc.localNormal);
                        TSVector2 vector5 = MathUtils.Mul(ref xfB, pc.localPoint);
                        TSVector2 vector6 = MathUtils.Mul(ref xfA, pc.localPoints[index]);
                        separation = (TSVector2.Dot(vector6 - vector5, normal) - pc.radiusA) - pc.radiusB;
                        point = vector6;
                        normal = -normal;
                        break;
                    }
                    default:
                        normal = TSVector2.zero;
                        point = TSVector2.zero;
                        separation = 0;
                        break;
                }
            }
        }

        public static class WorldManifold
        {
            public static void Initialize(ref Manifold manifold, ref Transform xfA, FP radiusA, ref Transform xfB, FP radiusB, out TSVector2 normal, out FixedArray2<TSVector2> points)
            {
                normal = TSVector2.zero;
                points = new FixedArray2<TSVector2>();
                if (manifold.PointCount != 0)
                {
                    switch (manifold.Type)
                    {
                        case ManifoldType.Circles:
                        {
                            normal = new TSVector2(1f, 0f);
                            TSVector2 vector = MathUtils.Mul(ref xfA, manifold.LocalPoint);
                            TSVector2 vector2 = MathUtils.Mul(ref xfB, manifold.Points[0].LocalPoint);
                            if (TSVector2.DistanceSquared(vector, vector2) > Settings.EpsilonSqr)
                            {
                                normal = vector2 - vector;
                                normal.Normalize();
                            }
                            TSVector2 vector3 = vector + (radiusA * normal);
                            TSVector2 vector4 = vector2 - (radiusB * normal);
                            points[0] = (TSVector2) (0.5f * (vector3 + vector4));
                            break;
                        }
                        case ManifoldType.FaceA:
                        {
                            normal = MathUtils.Mul(xfA.q, manifold.LocalNormal);
                            TSVector2 vector5 = MathUtils.Mul(ref xfA, manifold.LocalPoint);
                            for (int i = 0; i < manifold.PointCount; i++)
                            {
                                TSVector2 vector6 = MathUtils.Mul(ref xfB, manifold.Points[i].LocalPoint);
                                TSVector2 vector7 = vector6 + ((radiusA - TSVector2.Dot(vector6 - vector5, normal)) * normal);
                                TSVector2 vector8 = vector6 - (radiusB * normal);
                                points[i] = (TSVector2) (0.5f * (vector7 + vector8));
                            }
                            break;
                        }
                        case ManifoldType.FaceB:
                        {
                            normal = MathUtils.Mul(xfB.q, manifold.LocalNormal);
                            TSVector2 vector9 = MathUtils.Mul(ref xfB, manifold.LocalPoint);
                            for (int j = 0; j < manifold.PointCount; j++)
                            {
                                TSVector2 vector10 = MathUtils.Mul(ref xfA, manifold.Points[j].LocalPoint);
                                TSVector2 vector11 = vector10 + ((radiusB - TSVector2.Dot(vector10 - vector9, normal)) * normal);
                                TSVector2 vector12 = vector10 - (radiusA * normal);
                                points[j] = (TSVector2) (0.5f * (vector12 + vector11));
                            }
                            normal = -normal;
                            break;
                        }
                    }
                }
            }
        }
    }
}

