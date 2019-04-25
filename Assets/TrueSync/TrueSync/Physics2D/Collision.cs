namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class Collision
    {
        [ThreadStatic]
        private static DistanceInput _input;

        private static int ClipSegmentToLine(out FixedArray2<ClipVertex> vOut, ref FixedArray2<ClipVertex> vIn, TSVector2 normal, FP offset, int vertexIndexA)
        {
            vOut = new FixedArray2<ClipVertex>();
            ClipVertex vertex = vIn[0];
            ClipVertex vertex2 = vIn[1];
            int num = 0;
            FP fp = ((normal.x * vertex.V.x) + (normal.y * vertex.V.y)) - offset;
            FP fp2 = ((normal.x * vertex2.V.x) + (normal.y * vertex2.V.y)) - offset;
            if (fp <= 0f)
            {
                vOut[num++] = vertex;
            }
            if (fp2 <= 0f)
            {
                vOut[num++] = vertex2;
            }
            if ((fp * fp2) < 0f)
            {
                FP fp3 = fp / (fp - fp2);
                ClipVertex vertex3 = vOut[num];
                vertex3.V.x = vertex.V.x + (fp3 * (vertex2.V.x - vertex.V.x));
                vertex3.V.y = vertex.V.y + (fp3 * (vertex2.V.y - vertex.V.y));
                vertex3.ID.Features.IndexA = (byte) vertexIndexA;
                vertex3.ID.Features.IndexB = vertex.ID.Features.IndexB;
                vertex3.ID.Features.TypeA = 0;
                vertex3.ID.Features.TypeB = 1;
                vOut[num] = vertex3;
                num++;
            }
            return num;
        }

        public static void CollideCircles(ref Manifold manifold, CircleShape circleA, ref Transform xfA, CircleShape circleB, ref Transform xfB)
        {
            manifold.PointCount = 0;
            TSVector2 vector = MathUtils.Mul(ref xfA, circleA.Position);
            TSVector2 vector3 = MathUtils.Mul(ref xfB, circleB.Position) - vector;
            FP fp = TSVector2.Dot(vector3, vector3);
            FP fp2 = circleA.Radius + circleB.Radius;
            if (fp <= (fp2 * fp2))
            {
                manifold.Type = ManifoldType.Circles;
                manifold.LocalPoint = circleA.Position;
                manifold.LocalNormal = TSVector2.zero;
                manifold.PointCount = 1;
                ManifoldPoint point = manifold.Points[0];
                point.LocalPoint = circleB.Position;
                point.Id.Key = 0;
                manifold.Points[0] = point;
            }
        }

        public static void CollideEdgeAndCircle(ref Manifold manifold, EdgeShape edgeA, ref Transform transformA, CircleShape circleB, ref Transform transformB)
        {
            ContactFeature feature;
            TSVector2 vector5;
            TSVector2 vector6;
            manifold.PointCount = 0;
            TSVector2 vector = MathUtils.MulT(ref transformA, MathUtils.Mul(ref transformB, ref circleB._position));
            TSVector2 vector2 = edgeA.Vertex1;
            TSVector2 vector3 = edgeA.Vertex2;
            TSVector2 vector4 = vector3 - vector2;
            FP fp = TSVector2.Dot(vector4, vector3 - vector);
            FP fp2 = TSVector2.Dot(vector4, vector - vector2);
            FP fp3 = edgeA.Radius + circleB.Radius;
            feature.IndexB = 0;
            feature.TypeB = 0;
            if (fp2 <= 0f)
            {
                FP fp6;
                vector5 = vector2;
                vector6 = vector - vector5;
                TSVector2.Dot(ref vector6, ref vector6, out fp6);
                if (fp6 <= (fp3 * fp3))
                {
                    if (edgeA.HasVertex0)
                    {
                        TSVector2 vector8 = edgeA.Vertex0;
                        TSVector2 vector9 = vector2;
                        TSVector2 vector10 = vector9 - vector8;
                        if (TSVector2.Dot(vector10, vector9 - vector) > 0f)
                        {
                            return;
                        }
                    }
                    feature.IndexA = 0;
                    feature.TypeA = 0;
                    manifold.PointCount = 1;
                    manifold.Type = ManifoldType.Circles;
                    manifold.LocalNormal = TSVector2.zero;
                    manifold.LocalPoint = vector5;
                    ManifoldPoint point2 = new ManifoldPoint();
                    point2.Id.Key = 0;
                    point2.Id.Features = feature;
                    point2.LocalPoint = circleB.Position;
                    manifold.Points[0] = point2;
                }
            }
            else if (fp <= 0f)
            {
                FP fp8;
                vector5 = vector3;
                vector6 = vector - vector5;
                TSVector2.Dot(ref vector6, ref vector6, out fp8);
                if (fp8 <= (fp3 * fp3))
                {
                    if (edgeA.HasVertex3)
                    {
                        TSVector2 vector11 = edgeA.Vertex3;
                        TSVector2 vector12 = vector3;
                        TSVector2 vector13 = vector11 - vector12;
                        if (TSVector2.Dot(vector13, vector - vector12) > 0f)
                        {
                            return;
                        }
                    }
                    feature.IndexA = 1;
                    feature.TypeA = 0;
                    manifold.PointCount = 1;
                    manifold.Type = ManifoldType.Circles;
                    manifold.LocalNormal = TSVector2.zero;
                    manifold.LocalPoint = vector5;
                    ManifoldPoint point3 = new ManifoldPoint();
                    point3.Id.Key = 0;
                    point3.Id.Features = feature;
                    point3.LocalPoint = circleB.Position;
                    manifold.Points[0] = point3;
                }
            }
            else
            {
                FP fp4;
                FP fp5;
                TSVector2.Dot(ref vector4, ref vector4, out fp4);
                Debug.Assert(fp4 > 0f);
                vector5 = (TSVector2) ((1f / fp4) * ((fp * vector2) + (fp2 * vector3)));
                vector6 = vector - vector5;
                TSVector2.Dot(ref vector6, ref vector6, out fp5);
                if (fp5 <= (fp3 * fp3))
                {
                    TSVector2 vector7 = new TSVector2(-vector4.y, vector4.x);
                    if (TSVector2.Dot(vector7, vector - vector2) < 0f)
                    {
                        vector7 = new TSVector2(-vector7.x, -vector7.y);
                    }
                    vector7.Normalize();
                    feature.IndexA = 0;
                    feature.TypeA = 1;
                    manifold.PointCount = 1;
                    manifold.Type = ManifoldType.FaceA;
                    manifold.LocalNormal = vector7;
                    manifold.LocalPoint = vector2;
                    ManifoldPoint point = new ManifoldPoint();
                    point.Id.Key = 0;
                    point.Id.Features = feature;
                    point.LocalPoint = circleB.Position;
                    manifold.Points[0] = point;
                }
            }
        }

        public static void CollideEdgeAndPolygon(ref Manifold manifold, EdgeShape edgeA, ref Transform xfA, PolygonShape polygonB, ref Transform xfB)
        {
            new EPCollider().Collide(ref manifold, edgeA, ref xfA, polygonB, ref xfB);
        }

        public static void CollidePolygonAndCircle(ref Manifold manifold, PolygonShape polygonA, ref Transform xfA, CircleShape circleB, ref Transform xfB)
        {
            manifold.PointCount = 0;
            TSVector2 v = MathUtils.Mul(ref xfB, circleB.Position);
            TSVector2 vector2 = MathUtils.MulT(ref xfA, v);
            int num = 0;
            FP fp = -Settings.MaxFP;
            FP fp2 = polygonA.Radius + circleB.Radius;
            int count = polygonA.Vertices.Count;
            for (int i = 0; i < count; i++)
            {
                TSVector2 vector5 = polygonA.Normals[i];
                TSVector2 vector6 = vector2 - polygonA.Vertices[i];
                FP fp5 = (vector5.x * vector6.x) + (vector5.y * vector6.y);
                if (fp5 > fp2)
                {
                    return;
                }
                if (fp5 > fp)
                {
                    fp = fp5;
                    num = i;
                }
            }
            int num3 = num;
            int num4 = ((num3 + 1) < count) ? (num3 + 1) : 0;
            TSVector2 vector3 = polygonA.Vertices[num3];
            TSVector2 vector4 = polygonA.Vertices[num4];
            if (fp < Settings.Epsilon)
            {
                manifold.PointCount = 1;
                manifold.Type = ManifoldType.FaceA;
                manifold.LocalNormal = polygonA.Normals[num];
                manifold.LocalPoint = (TSVector2) (0.5f * (vector3 + vector4));
                ManifoldPoint point = manifold.Points[0];
                point.LocalPoint = circleB.Position;
                point.Id.Key = 0;
                manifold.Points[0] = point;
            }
            else
            {
                FP fp3 = ((vector2.x - vector3.x) * (vector4.x - vector3.x)) + ((vector2.y - vector3.y) * (vector4.y - vector3.y));
                FP fp4 = ((vector2.x - vector4.x) * (vector3.x - vector4.x)) + ((vector2.y - vector4.y) * (vector3.y - vector4.y));
                if (fp3 <= 0f)
                {
                    FP fp6 = ((vector2.x - vector3.x) * (vector2.x - vector3.x)) + ((vector2.y - vector3.y) * (vector2.y - vector3.y));
                    if (fp6 <= (fp2 * fp2))
                    {
                        manifold.PointCount = 1;
                        manifold.Type = ManifoldType.FaceA;
                        manifold.LocalNormal = vector2 - vector3;
                        FP fp7 = 1f / FP.Sqrt((manifold.LocalNormal.x * manifold.LocalNormal.x) + (manifold.LocalNormal.y * manifold.LocalNormal.y));
                        manifold.LocalNormal.x *= fp7;
                        manifold.LocalNormal.y *= fp7;
                        manifold.LocalPoint = vector3;
                        ManifoldPoint point2 = manifold.Points[0];
                        point2.LocalPoint = circleB.Position;
                        point2.Id.Key = 0;
                        manifold.Points[0] = point2;
                    }
                }
                else if (fp4 <= 0f)
                {
                    FP fp8 = ((vector2.x - vector4.x) * (vector2.x - vector4.x)) + ((vector2.y - vector4.y) * (vector2.y - vector4.y));
                    if (fp8 <= (fp2 * fp2))
                    {
                        manifold.PointCount = 1;
                        manifold.Type = ManifoldType.FaceA;
                        manifold.LocalNormal = vector2 - vector4;
                        FP fp9 = 1f / FP.Sqrt((manifold.LocalNormal.x * manifold.LocalNormal.x) + (manifold.LocalNormal.y * manifold.LocalNormal.y));
                        manifold.LocalNormal.x *= fp9;
                        manifold.LocalNormal.y *= fp9;
                        manifold.LocalPoint = vector4;
                        ManifoldPoint point3 = manifold.Points[0];
                        point3.LocalPoint = circleB.Position;
                        point3.Id.Key = 0;
                        manifold.Points[0] = point3;
                    }
                }
                else
                {
                    TSVector2 vector7 = (TSVector2) (0.5f * (vector3 + vector4));
                    TSVector2 vector8 = vector2 - vector7;
                    TSVector2 vector9 = polygonA.Normals[num3];
                    FP fp10 = (vector8.x * vector9.x) + (vector8.y * vector9.y);
                    if (fp10 <= fp2)
                    {
                        manifold.PointCount = 1;
                        manifold.Type = ManifoldType.FaceA;
                        manifold.LocalNormal = polygonA.Normals[num3];
                        manifold.LocalPoint = vector7;
                        ManifoldPoint point4 = manifold.Points[0];
                        point4.LocalPoint = circleB.Position;
                        point4.Id.Key = 0;
                        manifold.Points[0] = point4;
                    }
                }
            }
        }

        public static void CollidePolygons(ref Manifold manifold, PolygonShape polyA, ref Transform transformA, PolygonShape polyB, ref Transform transformB)
        {
            manifold.PointCount = 0;
            FP fp = polyA.Radius + polyB.Radius;
            int edgeIndex = 0;
            FP fp2 = FindMaxSeparation(out edgeIndex, polyA, ref transformA, polyB, ref transformB);
            if (fp2 <= fp)
            {
                int num2 = 0;
                FP fp3 = FindMaxSeparation(out num2, polyB, ref transformB, polyA, ref transformA);
                if (fp3 <= fp)
                {
                    PolygonShape shape;
                    PolygonShape shape2;
                    Transform transform;
                    Transform transform2;
                    int num3;
                    bool flag;
                    FixedArray2<ClipVertex> array;
                    FixedArray2<ClipVertex> array2;
                    FixedArray2<ClipVertex> array3;
                    FP fp4 = 0.98f;
                    FP fp5 = 0.001f;
                    if (fp3 > ((fp4 * fp2) + fp5))
                    {
                        shape = polyB;
                        shape2 = polyA;
                        transform = transformB;
                        transform2 = transformA;
                        num3 = num2;
                        manifold.Type = ManifoldType.FaceB;
                        flag = true;
                    }
                    else
                    {
                        shape = polyA;
                        shape2 = polyB;
                        transform = transformA;
                        transform2 = transformB;
                        num3 = edgeIndex;
                        manifold.Type = ManifoldType.FaceA;
                        flag = false;
                    }
                    FindIncidentEdge(out array, shape, ref transform, num3, shape2, ref transform2);
                    int count = shape.Vertices.Count;
                    int vertexIndexA = num3;
                    int num6 = ((num3 + 1) < count) ? (num3 + 1) : 0;
                    TSVector2 v = shape.Vertices[vertexIndexA];
                    TSVector2 vector2 = shape.Vertices[num6];
                    TSVector2 vector3 = vector2 - v;
                    vector3.Normalize();
                    TSVector2 vector4 = new TSVector2(vector3.y, -vector3.x);
                    TSVector2 vector5 = (TSVector2) (0.5f * (v + vector2));
                    TSVector2 normal = MathUtils.Mul(transform.q, vector3);
                    FP y = normal.y;
                    FP fp7 = -normal.x;
                    v = MathUtils.Mul(ref transform, v);
                    vector2 = MathUtils.Mul(ref transform, vector2);
                    FP fp8 = (y * v.x) + (fp7 * v.y);
                    FP offset = -((normal.x * v.x) + (normal.y * v.y)) + fp;
                    FP fp10 = ((normal.x * vector2.x) + (normal.y * vector2.y)) + fp;
                    if ((ClipSegmentToLine(out array2, ref array, -normal, offset, vertexIndexA) >= 2) && (ClipSegmentToLine(out array3, ref array2, normal, fp10, num6) >= 2))
                    {
                        manifold.LocalNormal = vector4;
                        manifold.LocalPoint = vector5;
                        int num8 = 0;
                        for (int i = 0; i < 2; i++)
                        {
                            TSVector2 vector7 = array3[i].V;
                            FP fp11 = ((y * vector7.x) + (fp7 * vector7.y)) - fp8;
                            if (fp11 <= fp)
                            {
                                ManifoldPoint point = manifold.Points[num8];
                                point.LocalPoint = MathUtils.MulT(ref transform2, array3[i].V);
                                point.Id = array3[i].ID;
                                if (flag)
                                {
                                    ContactFeature features = point.Id.Features;
                                    point.Id.Features.IndexA = features.IndexB;
                                    point.Id.Features.IndexB = features.IndexA;
                                    point.Id.Features.TypeA = features.TypeB;
                                    point.Id.Features.TypeB = features.TypeA;
                                }
                                manifold.Points[num8] = point;
                                num8++;
                            }
                        }
                        manifold.PointCount = num8;
                    }
                }
            }
        }

        private static FP EdgeSeparation(PolygonShape poly1, ref Transform xf1, int edge1, PolygonShape poly2, ref Transform xf2)
        {
            List<TSVector2> vertices = poly1.Vertices;
            List<TSVector2> normals = poly1.Normals;
            int count = poly2.Vertices.Count;
            List<TSVector2> list3 = poly2.Vertices;
            Debug.Assert((0 <= edge1) && (edge1 < poly1.Vertices.Count));
            TSVector2 v = MathUtils.Mul(xf1.q, normals[edge1]);
            TSVector2 vector2 = MathUtils.MulT(xf2.q, v);
            int num2 = 0;
            FP maxFP = Settings.MaxFP;
            for (int i = 0; i < count; i++)
            {
                FP fp3 = TSVector2.Dot(list3[i], vector2);
                if (fp3 < maxFP)
                {
                    maxFP = fp3;
                    num2 = i;
                }
            }
            TSVector2 vector3 = MathUtils.Mul(ref xf1, vertices[edge1]);
            return TSVector2.Dot(MathUtils.Mul(ref xf2, list3[num2]) - vector3, v);
        }

        private static void FindIncidentEdge(out FixedArray2<ClipVertex> c, PolygonShape poly1, ref Transform xf1, int edge1, PolygonShape poly2, ref Transform xf2)
        {
            c = new FixedArray2<ClipVertex>();
            Vertices normals = poly1.Normals;
            int count = poly2.Vertices.Count;
            Vertices vertices = poly2.Vertices;
            Vertices vertices3 = poly2.Normals;
            Debug.Assert((0 <= edge1) && (edge1 < poly1.Vertices.Count));
            TSVector2 vector = MathUtils.MulT(xf2.q, MathUtils.Mul(xf1.q, normals[edge1]));
            int num2 = 0;
            FP maxFP = Settings.MaxFP;
            for (int i = 0; i < count; i++)
            {
                FP fp2 = TSVector2.Dot(vector, vertices3[i]);
                if (fp2 < maxFP)
                {
                    maxFP = fp2;
                    num2 = i;
                }
            }
            int num3 = num2;
            int num4 = ((num3 + 1) < count) ? (num3 + 1) : 0;
            ClipVertex vertex = c[0];
            vertex.V = MathUtils.Mul(ref xf2, vertices[num3]);
            vertex.ID.Features.IndexA = (byte) edge1;
            vertex.ID.Features.IndexB = (byte) num3;
            vertex.ID.Features.TypeA = 1;
            vertex.ID.Features.TypeB = 0;
            c[0] = vertex;
            ClipVertex vertex2 = c[1];
            vertex2.V = MathUtils.Mul(ref xf2, vertices[num4]);
            vertex2.ID.Features.IndexA = (byte) edge1;
            vertex2.ID.Features.IndexB = (byte) num4;
            vertex2.ID.Features.TypeA = 1;
            vertex2.ID.Features.TypeB = 0;
            c[1] = vertex2;
        }

        private static FP FindMaxSeparation(out int edgeIndex, PolygonShape poly1, ref Transform xf1, PolygonShape poly2, ref Transform xf2)
        {
            int num5;
            FP fp5;
            int num6;
            int count = poly1.Vertices.Count;
            List<TSVector2> normals = poly1.Normals;
            TSVector2 v = MathUtils.Mul(ref xf2, poly2.MassData.Centroid) - MathUtils.Mul(ref xf1, poly1.MassData.Centroid);
            TSVector2 vector2 = MathUtils.MulT(xf1.q, v);
            int num2 = 0;
            FP fp = -Settings.MaxFP;
            for (int i = 0; i < count; i++)
            {
                FP fp6 = TSVector2.Dot(normals[i], vector2);
                if (fp6 > fp)
                {
                    fp = fp6;
                    num2 = i;
                }
            }
            FP fp2 = EdgeSeparation(poly1, ref xf1, num2, poly2, ref xf2);
            int num3 = ((num2 - 1) >= 0) ? (num2 - 1) : (count - 1);
            FP fp3 = EdgeSeparation(poly1, ref xf1, num3, poly2, ref xf2);
            int num4 = ((num2 + 1) < count) ? (num2 + 1) : 0;
            FP fp4 = EdgeSeparation(poly1, ref xf1, num4, poly2, ref xf2);
            if ((fp3 > fp2) && (fp3 > fp4))
            {
                num6 = -1;
                num5 = num3;
                fp5 = fp3;
            }
            else if (fp4 > fp2)
            {
                num6 = 1;
                num5 = num4;
                fp5 = fp4;
            }
            else
            {
                edgeIndex = num2;
                return fp2;
            }
        Label_013D:
            if (num6 == -1)
            {
                num2 = ((num5 - 1) >= 0) ? (num5 - 1) : (count - 1);
            }
            else
            {
                num2 = ((num5 + 1) < count) ? (num5 + 1) : 0;
            }
            fp2 = EdgeSeparation(poly1, ref xf1, num2, poly2, ref xf2);
            if (fp2 > fp5)
            {
                num5 = num2;
                fp5 = fp2;
                goto Label_013D;
            }
            edgeIndex = num5;
            return fp5;
        }

        public static void GetPointStates(out FixedArray2<PointState> state1, out FixedArray2<PointState> state2, ref Manifold manifold1, ref Manifold manifold2)
        {
            state1 = new FixedArray2<PointState>();
            state2 = new FixedArray2<PointState>();
            for (int i = 0; i < manifold1.PointCount; i++)
            {
                ContactID id = manifold1.Points[i].Id;
                state1[i] = PointState.Remove;
                for (int k = 0; k < manifold2.PointCount; k++)
                {
                    if (manifold2.Points[k].Id.Key == id.Key)
                    {
                        state1[i] = PointState.Persist;
                        break;
                    }
                }
            }
            for (int j = 0; j < manifold2.PointCount; j++)
            {
                ContactID tid2 = manifold2.Points[j].Id;
                state2[j] = PointState.Add;
                for (int m = 0; m < manifold1.PointCount; m++)
                {
                    if (manifold1.Points[m].Id.Key == tid2.Key)
                    {
                        state2[j] = PointState.Persist;
                        break;
                    }
                }
            }
        }

        public static bool TestOverlap(TrueSync.Physics2D.Shape shapeA, int indexA, TrueSync.Physics2D.Shape shapeB, int indexB, ref Transform xfA, ref Transform xfB)
        {
            SimplexCache cache;
            DistanceOutput output;
            _input = _input ?? new DistanceInput();
            _input.ProxyA.Set(shapeA, indexA);
            _input.ProxyB.Set(shapeB, indexB);
            _input.TransformA = xfA;
            _input.TransformB = xfB;
            _input.UseRadii = true;
            Distance.ComputeDistance(out output, out cache, _input);
            return (output.Distance < (10f * Settings.Epsilon));
        }

        private class EPCollider
        {
            private TSVector2 _centroidB;
            private bool _front;
            private TSVector2 _lowerLimit;
            private TSVector2 _normal;
            private TSVector2 _normal0;
            private TSVector2 _normal1;
            private TSVector2 _normal2;
            private TempPolygon _polygonB = new TempPolygon();
            private FP _radius;
            private TSVector2 _upperLimit;
            private TSVector2 _v0;
            private TSVector2 _v1;
            private TSVector2 _v2;
            private TSVector2 _v3;
            private Transform _xf;

            public void Collide(ref Manifold manifold, EdgeShape edgeA, ref Transform xfA, PolygonShape polygonB, ref Transform xfB)
            {
                this._xf = MathUtils.MulT(xfA, xfB);
                this._centroidB = MathUtils.Mul(ref this._xf, polygonB.MassData.Centroid);
                this._v0 = edgeA.Vertex0;
                this._v1 = edgeA._vertex1;
                this._v2 = edgeA._vertex2;
                this._v3 = edgeA.Vertex3;
                bool flag = edgeA.HasVertex0;
                bool flag2 = edgeA.HasVertex3;
                TSVector2 b = this._v2 - this._v1;
                b.Normalize();
                this._normal1 = new TSVector2(b.y, -b.x);
                FP fp = TSVector2.Dot(this._normal1, this._centroidB - this._v1);
                FP fp2 = 0f;
                FP fp3 = 0f;
                bool flag3 = false;
                bool flag4 = false;
                if (flag)
                {
                    TSVector2 a = this._v1 - this._v0;
                    a.Normalize();
                    this._normal0 = new TSVector2(a.y, -a.x);
                    flag3 = MathUtils.Cross(a, b) >= 0f;
                    fp2 = TSVector2.Dot(this._normal0, this._centroidB - this._v0);
                }
                if (flag2)
                {
                    TSVector2 vector3 = this._v3 - this._v2;
                    vector3.Normalize();
                    this._normal2 = new TSVector2(vector3.y, -vector3.x);
                    flag4 = MathUtils.Cross(b, vector3) > 0f;
                    fp3 = TSVector2.Dot(this._normal2, this._centroidB - this._v2);
                }
                if (flag & flag2)
                {
                    if (flag3 & flag4)
                    {
                        this._front = ((fp2 >= 0f) || (fp >= 0f)) || (fp3 >= 0f);
                        if (this._front)
                        {
                            this._normal = this._normal1;
                            this._lowerLimit = this._normal0;
                            this._upperLimit = this._normal2;
                        }
                        else
                        {
                            this._normal = -this._normal1;
                            this._lowerLimit = -this._normal1;
                            this._upperLimit = -this._normal1;
                        }
                    }
                    else if (flag3)
                    {
                        this._front = (fp2 >= 0f) || ((fp >= 0f) && (fp3 >= 0f));
                        if (this._front)
                        {
                            this._normal = this._normal1;
                            this._lowerLimit = this._normal0;
                            this._upperLimit = this._normal1;
                        }
                        else
                        {
                            this._normal = -this._normal1;
                            this._lowerLimit = -this._normal2;
                            this._upperLimit = -this._normal1;
                        }
                    }
                    else if (flag4)
                    {
                        this._front = (fp3 >= 0f) || ((fp2 >= 0f) && (fp >= 0f));
                        if (this._front)
                        {
                            this._normal = this._normal1;
                            this._lowerLimit = this._normal1;
                            this._upperLimit = this._normal2;
                        }
                        else
                        {
                            this._normal = -this._normal1;
                            this._lowerLimit = -this._normal1;
                            this._upperLimit = -this._normal0;
                        }
                    }
                    else
                    {
                        this._front = ((fp2 >= 0f) && (fp >= 0f)) && (fp3 >= 0f);
                        if (this._front)
                        {
                            this._normal = this._normal1;
                            this._lowerLimit = this._normal1;
                            this._upperLimit = this._normal1;
                        }
                        else
                        {
                            this._normal = -this._normal1;
                            this._lowerLimit = -this._normal2;
                            this._upperLimit = -this._normal0;
                        }
                    }
                }
                else if (flag)
                {
                    if (flag3)
                    {
                        this._front = (fp2 >= 0f) || (fp >= 0f);
                        if (this._front)
                        {
                            this._normal = this._normal1;
                            this._lowerLimit = this._normal0;
                            this._upperLimit = -this._normal1;
                        }
                        else
                        {
                            this._normal = -this._normal1;
                            this._lowerLimit = this._normal1;
                            this._upperLimit = -this._normal1;
                        }
                    }
                    else
                    {
                        this._front = (fp2 >= 0f) && (fp >= 0f);
                        if (this._front)
                        {
                            this._normal = this._normal1;
                            this._lowerLimit = this._normal1;
                            this._upperLimit = -this._normal1;
                        }
                        else
                        {
                            this._normal = -this._normal1;
                            this._lowerLimit = this._normal1;
                            this._upperLimit = -this._normal0;
                        }
                    }
                }
                else if (flag2)
                {
                    if (flag4)
                    {
                        this._front = (fp >= 0f) || (fp3 >= 0f);
                        if (this._front)
                        {
                            this._normal = this._normal1;
                            this._lowerLimit = -this._normal1;
                            this._upperLimit = this._normal2;
                        }
                        else
                        {
                            this._normal = -this._normal1;
                            this._lowerLimit = -this._normal1;
                            this._upperLimit = this._normal1;
                        }
                    }
                    else
                    {
                        this._front = (fp >= 0f) && (fp3 >= 0f);
                        if (this._front)
                        {
                            this._normal = this._normal1;
                            this._lowerLimit = -this._normal1;
                            this._upperLimit = this._normal1;
                        }
                        else
                        {
                            this._normal = -this._normal1;
                            this._lowerLimit = -this._normal2;
                            this._upperLimit = this._normal1;
                        }
                    }
                }
                else
                {
                    this._front = fp >= 0f;
                    if (this._front)
                    {
                        this._normal = this._normal1;
                        this._lowerLimit = -this._normal1;
                        this._upperLimit = -this._normal1;
                    }
                    else
                    {
                        this._normal = -this._normal1;
                        this._lowerLimit = this._normal1;
                        this._upperLimit = this._normal1;
                    }
                }
                this._polygonB.Count = polygonB.Vertices.Count;
                for (int i = 0; i < polygonB.Vertices.Count; i++)
                {
                    this._polygonB.Vertices[i] = MathUtils.Mul(ref this._xf, polygonB.Vertices[i]);
                    this._polygonB.Normals[i] = MathUtils.Mul(this._xf.q, polygonB.Normals[i]);
                }
                this._radius = 2f * Settings.PolygonRadius;
                manifold.PointCount = 0;
                EPAxis axis = this.ComputeEdgeSeparation();
                if ((axis.Type != EPAxisType.Unknown) && (axis.Separation <= this._radius))
                {
                    EPAxis axis2 = this.ComputePolygonSeparation();
                    if ((axis2.Type == EPAxisType.Unknown) || (axis2.Separation <= this._radius))
                    {
                        EPAxis axis3;
                        ReferenceFace face;
                        FixedArray2<ClipVertex> array2;
                        FixedArray2<ClipVertex> array3;
                        FP fp4 = 0.98f;
                        FP fp5 = 0.001f;
                        if (axis2.Type == EPAxisType.Unknown)
                        {
                            axis3 = axis;
                        }
                        else if (axis2.Separation > ((fp4 * axis.Separation) + fp5))
                        {
                            axis3 = axis2;
                        }
                        else
                        {
                            axis3 = axis;
                        }
                        FixedArray2<ClipVertex> vIn = new FixedArray2<ClipVertex>();
                        if (axis3.Type == EPAxisType.EdgeA)
                        {
                            manifold.Type = ManifoldType.FaceA;
                            int num4 = 0;
                            FP fp6 = TSVector2.Dot(this._normal, this._polygonB.Normals[0]);
                            for (int j = 1; j < this._polygonB.Count; j++)
                            {
                                FP fp7 = TSVector2.Dot(this._normal, this._polygonB.Normals[j]);
                                if (fp7 < fp6)
                                {
                                    fp6 = fp7;
                                    num4 = j;
                                }
                            }
                            int index = num4;
                            int num6 = ((index + 1) < this._polygonB.Count) ? (index + 1) : 0;
                            ClipVertex vertex = vIn[0];
                            vertex.V = this._polygonB.Vertices[index];
                            vertex.ID.Features.IndexA = 0;
                            vertex.ID.Features.IndexB = (byte) index;
                            vertex.ID.Features.TypeA = 1;
                            vertex.ID.Features.TypeB = 0;
                            vIn[0] = vertex;
                            ClipVertex vertex2 = vIn[1];
                            vertex2.V = this._polygonB.Vertices[num6];
                            vertex2.ID.Features.IndexA = 0;
                            vertex2.ID.Features.IndexB = (byte) num6;
                            vertex2.ID.Features.TypeA = 1;
                            vertex2.ID.Features.TypeB = 0;
                            vIn[1] = vertex2;
                            if (this._front)
                            {
                                face.i1 = 0;
                                face.i2 = 1;
                                face.v1 = this._v1;
                                face.v2 = this._v2;
                                face.normal = this._normal1;
                            }
                            else
                            {
                                face.i1 = 1;
                                face.i2 = 0;
                                face.v1 = this._v2;
                                face.v2 = this._v1;
                                face.normal = -this._normal1;
                            }
                        }
                        else
                        {
                            manifold.Type = ManifoldType.FaceB;
                            ClipVertex vertex3 = vIn[0];
                            vertex3.V = this._v1;
                            vertex3.ID.Features.IndexA = 0;
                            vertex3.ID.Features.IndexB = (byte) axis3.Index;
                            vertex3.ID.Features.TypeA = 0;
                            vertex3.ID.Features.TypeB = 1;
                            vIn[0] = vertex3;
                            ClipVertex vertex4 = vIn[1];
                            vertex4.V = this._v2;
                            vertex4.ID.Features.IndexA = 0;
                            vertex4.ID.Features.IndexB = (byte) axis3.Index;
                            vertex4.ID.Features.TypeA = 0;
                            vertex4.ID.Features.TypeB = 1;
                            vIn[1] = vertex4;
                            face.i1 = axis3.Index;
                            face.i2 = ((face.i1 + 1) < this._polygonB.Count) ? (face.i1 + 1) : 0;
                            face.v1 = this._polygonB.Vertices[face.i1];
                            face.v2 = this._polygonB.Vertices[face.i2];
                            face.normal = this._polygonB.Normals[face.i1];
                        }
                        face.sideNormal1 = new TSVector2(face.normal.y, -face.normal.x);
                        face.sideNormal2 = -face.sideNormal1;
                        face.sideOffset1 = TSVector2.Dot(face.sideNormal1, face.v1);
                        face.sideOffset2 = TSVector2.Dot(face.sideNormal2, face.v2);
                        if ((Collision.ClipSegmentToLine(out array2, ref vIn, face.sideNormal1, face.sideOffset1, face.i1) >= 2) && (Collision.ClipSegmentToLine(out array3, ref array2, face.sideNormal2, face.sideOffset2, face.i2) >= 2))
                        {
                            if (axis3.Type == EPAxisType.EdgeA)
                            {
                                manifold.LocalNormal = face.normal;
                                manifold.LocalPoint = face.v1;
                            }
                            else
                            {
                                manifold.LocalNormal = polygonB.Normals[face.i1];
                                manifold.LocalPoint = polygonB.Vertices[face.i1];
                            }
                            int num2 = 0;
                            for (int k = 0; k < 2; k++)
                            {
                                if (TSVector2.Dot(face.normal, array3[k].V - face.v1) <= this._radius)
                                {
                                    ManifoldPoint point = manifold.Points[num2];
                                    if (axis3.Type == EPAxisType.EdgeA)
                                    {
                                        point.LocalPoint = MathUtils.MulT(ref this._xf, array3[k].V);
                                        point.Id = array3[k].ID;
                                    }
                                    else
                                    {
                                        point.LocalPoint = array3[k].V;
                                        point.Id.Features.TypeA = array3[k].ID.Features.TypeB;
                                        point.Id.Features.TypeB = array3[k].ID.Features.TypeA;
                                        point.Id.Features.IndexA = array3[k].ID.Features.IndexB;
                                        point.Id.Features.IndexB = array3[k].ID.Features.IndexA;
                                    }
                                    manifold.Points[num2] = point;
                                    num2++;
                                }
                            }
                            manifold.PointCount = num2;
                        }
                    }
                }
            }

            private EPAxis ComputeEdgeSeparation()
            {
                EPAxis axis;
                axis.Type = EPAxisType.EdgeA;
                axis.Index = this._front ? 0 : 1;
                axis.Separation = Settings.MaxFP;
                for (int i = 0; i < this._polygonB.Count; i++)
                {
                    FP fp = TSVector2.Dot(this._normal, this._polygonB.Vertices[i] - this._v1);
                    if (fp < axis.Separation)
                    {
                        axis.Separation = fp;
                    }
                }
                return axis;
            }

            private EPAxis ComputePolygonSeparation()
            {
                EPAxis axis;
                axis.Type = EPAxisType.Unknown;
                axis.Index = -1;
                axis.Separation = -Settings.MaxFP;
                TSVector2 vector = new TSVector2(-this._normal.y, this._normal.x);
                for (int i = 0; i < this._polygonB.Count; i++)
                {
                    TSVector2 vector2 = -this._polygonB.Normals[i];
                    FP fp = TSVector2.Dot(vector2, this._polygonB.Vertices[i] - this._v1);
                    FP fp2 = TSVector2.Dot(vector2, this._polygonB.Vertices[i] - this._v2);
                    FP fp3 = TSMath.Min(fp, fp2);
                    if (fp3 > this._radius)
                    {
                        axis.Type = EPAxisType.EdgeB;
                        axis.Index = i;
                        axis.Separation = fp3;
                        return axis;
                    }
                    if (TSVector2.Dot(vector2, vector) >= 0f)
                    {
                        if (TSVector2.Dot(vector2 - this._upperLimit, this._normal) < -Settings.AngularSlop)
                        {
                            continue;
                        }
                    }
                    else if (TSVector2.Dot(vector2 - this._lowerLimit, this._normal) < -Settings.AngularSlop)
                    {
                        continue;
                    }
                    if (fp3 > axis.Separation)
                    {
                        axis.Type = EPAxisType.EdgeB;
                        axis.Index = i;
                        axis.Separation = fp3;
                    }
                }
                return axis;
            }
        }
    }
}

