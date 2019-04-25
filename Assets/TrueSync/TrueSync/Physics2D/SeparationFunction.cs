namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class SeparationFunction
    {
        [ThreadStatic]
        private static TSVector2 _axis;
        [ThreadStatic]
        private static TSVector2 _localPoint;
        [ThreadStatic]
        private static DistanceProxy _proxyA;
        [ThreadStatic]
        private static DistanceProxy _proxyB;
        [ThreadStatic]
        private static Sweep _sweepA;
        [ThreadStatic]
        private static Sweep _sweepB;
        [ThreadStatic]
        private static SeparationFunctionType _type;

        public static FP Evaluate(int indexA, int indexB, FP t)
        {
            Transform transform;
            Transform transform2;
            _sweepA.GetTransform(out transform, t);
            _sweepB.GetTransform(out transform2, t);
            switch (_type)
            {
                case SeparationFunctionType.Points:
                {
                    TSVector2 v = _proxyA.Vertices[indexA];
                    TSVector2 vector2 = _proxyB.Vertices[indexB];
                    TSVector2 vector3 = MathUtils.Mul(ref transform, v);
                    return TSVector2.Dot(MathUtils.Mul(ref transform2, vector2) - vector3, _axis);
                }
                case SeparationFunctionType.FaceA:
                {
                    TSVector2 vector5 = MathUtils.Mul(ref transform.q, _axis);
                    TSVector2 vector6 = MathUtils.Mul(ref transform, _localPoint);
                    TSVector2 vector7 = _proxyB.Vertices[indexB];
                    return TSVector2.Dot(MathUtils.Mul(ref transform2, vector7) - vector6, vector5);
                }
                case SeparationFunctionType.FaceB:
                {
                    TSVector2 vector9 = MathUtils.Mul(ref transform2.q, _axis);
                    TSVector2 vector10 = MathUtils.Mul(ref transform2, _localPoint);
                    TSVector2 vector11 = _proxyA.Vertices[indexA];
                    return TSVector2.Dot(MathUtils.Mul(ref transform, vector11) - vector10, vector9);
                }
            }
            Debug.Assert(false);
            return 0f;
        }

        public static FP FindMinSeparation(out int indexA, out int indexB, FP t)
        {
            Transform transform;
            Transform transform2;
            _sweepA.GetTransform(out transform, t);
            _sweepB.GetTransform(out transform2, t);
            switch (_type)
            {
                case SeparationFunctionType.Points:
                {
                    TSVector2 direction = MathUtils.MulT(ref transform.q, _axis);
                    TSVector2 vector2 = MathUtils.MulT(ref transform2.q, -_axis);
                    indexA = _proxyA.GetSupport(direction);
                    indexB = _proxyB.GetSupport(vector2);
                    TSVector2 v = _proxyA.Vertices[indexA];
                    TSVector2 vector4 = _proxyB.Vertices[indexB];
                    TSVector2 vector5 = MathUtils.Mul(ref transform, v);
                    return TSVector2.Dot(MathUtils.Mul(ref transform2, vector4) - vector5, _axis);
                }
                case SeparationFunctionType.FaceA:
                {
                    TSVector2 vector7 = MathUtils.Mul(ref transform.q, _axis);
                    TSVector2 vector8 = MathUtils.Mul(ref transform, _localPoint);
                    TSVector2 vector9 = MathUtils.MulT(ref transform2.q, -vector7);
                    indexA = -1;
                    indexB = _proxyB.GetSupport(vector9);
                    TSVector2 vector10 = _proxyB.Vertices[indexB];
                    return TSVector2.Dot(MathUtils.Mul(ref transform2, vector10) - vector8, vector7);
                }
                case SeparationFunctionType.FaceB:
                {
                    TSVector2 vector12 = MathUtils.Mul(ref transform2.q, _axis);
                    TSVector2 vector13 = MathUtils.Mul(ref transform2, _localPoint);
                    TSVector2 vector14 = MathUtils.MulT(ref transform.q, -vector12);
                    indexB = -1;
                    indexA = _proxyA.GetSupport(vector14);
                    TSVector2 vector15 = _proxyA.Vertices[indexA];
                    return TSVector2.Dot(MathUtils.Mul(ref transform, vector15) - vector13, vector12);
                }
            }
            Debug.Assert(false);
            indexA = -1;
            indexB = -1;
            return 0f;
        }

        public static void Set(ref SimplexCache cache, DistanceProxy proxyA, ref Sweep sweepA, DistanceProxy proxyB, ref Sweep sweepB, FP t1)
        {
            Transform transform;
            Transform transform2;
            _localPoint = TSVector2.zero;
            _proxyA = proxyA;
            _proxyB = proxyB;
            int count = cache.Count;
            Debug.Assert((0 < count) && (count < 3));
            _sweepA = sweepA;
            _sweepB = sweepB;
            _sweepA.GetTransform(out transform, t1);
            _sweepB.GetTransform(out transform2, t1);
            if (count == 1)
            {
                _type = SeparationFunctionType.Points;
                TSVector2 v = _proxyA.Vertices[cache.IndexA[0]];
                TSVector2 vector2 = _proxyB.Vertices[cache.IndexB[0]];
                TSVector2 vector3 = MathUtils.Mul(ref transform, v);
                _axis = MathUtils.Mul(ref transform2, vector2) - vector3;
                _axis.Normalize();
            }
            else if (cache.IndexA[0] == cache.IndexA[1])
            {
                _type = SeparationFunctionType.FaceB;
                TSVector2 vector5 = proxyB.Vertices[cache.IndexB[0]];
                TSVector2 vector6 = proxyB.Vertices[cache.IndexB[1]];
                TSVector2 vector7 = vector6 - vector5;
                _axis = new TSVector2(vector7.y, -vector7.x);
                _axis.Normalize();
                TSVector2 vector8 = MathUtils.Mul(ref transform2.q, _axis);
                _localPoint = (TSVector2) (0.5f * (vector5 + vector6));
                TSVector2 vector9 = MathUtils.Mul(ref transform2, _localPoint);
                TSVector2 vector10 = proxyA.Vertices[cache.IndexA[0]];
                if (TSVector2.Dot(MathUtils.Mul(ref transform, vector10) - vector9, vector8) < 0f)
                {
                    _axis = -_axis;
                }
            }
            else
            {
                _type = SeparationFunctionType.FaceA;
                TSVector2 vector12 = _proxyA.Vertices[cache.IndexA[0]];
                TSVector2 vector13 = _proxyA.Vertices[cache.IndexA[1]];
                TSVector2 vector14 = vector13 - vector12;
                _axis = new TSVector2(vector14.y, -vector14.x);
                _axis.Normalize();
                TSVector2 vector15 = MathUtils.Mul(ref transform.q, _axis);
                _localPoint = (TSVector2) (0.5f * (vector12 + vector13));
                TSVector2 vector16 = MathUtils.Mul(ref transform, _localPoint);
                TSVector2 vector17 = _proxyB.Vertices[cache.IndexB[0]];
                if (TSVector2.Dot(MathUtils.Mul(ref transform2, vector17) - vector16, vector15) < 0f)
                {
                    _axis = -_axis;
                }
            }
        }
    }
}

