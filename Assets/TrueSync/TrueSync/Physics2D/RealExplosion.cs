namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TrueSync;

    public sealed class RealExplosion : PhysicsLogic
    {
        private List<ShapeData> _data;
        private RayDataComparer _rdc;
        public FP EdgeRatio;
        public bool IgnoreWhenInsideShape;
        public FP MaxAngle;
        private static readonly FP MaxEdgeOffset = (MathHelper.Pi / 90);
        public int MaxShapes;
        public int MinRays;

        public RealExplosion(TrueSync.Physics2D.World world) : base(world, PhysicsLogicType.Explosion)
        {
            this.EdgeRatio = 0.025f;
            this.IgnoreWhenInsideShape = false;
            this.MaxAngle = MathHelper.Pi / 15;
            this.MaxShapes = 100;
            this.MinRays = 5;
            this._data = new List<ShapeData>();
            this._rdc = new RayDataComparer();
            this._data = new List<ShapeData>();
        }

        public Dictionary<Fixture, TSVector2> Activate(TSVector2 pos, FP radius, FP maxForce)
        {
            AABB aabb;
            aabb.LowerBound = pos + new TSVector2(-radius, -radius);
            aabb.UpperBound = pos + new TSVector2(radius, radius);
            Fixture[] shapes = new Fixture[this.MaxShapes];
            Fixture[] containedShapes = new Fixture[5];
            bool exit = false;
            int shapeCount = 0;
            int containedShapeCount = 0;
            base.World.QueryAABB(delegate (Fixture fixture) {
                int num1;
                if (fixture.TestPoint(ref pos))
                {
                    if (this.IgnoreWhenInsideShape)
                    {
                        exit = true;
                        return false;
                    }
                    num1 = containedShapeCount;
                    containedShapeCount = num1 + 1;
                    containedShapes[num1] = fixture;
                }
                else
                {
                    num1 = shapeCount;
                    shapeCount = num1 + 1;
                    shapes[num1] = fixture;
                }
                return true;
            }, ref aabb);
            if (exit)
            {
                return new Dictionary<Fixture, TSVector2>();
            }
            Dictionary<Fixture, TSVector2> dictionary = new Dictionary<Fixture, TSVector2>(shapeCount + containedShapeCount);
            FP[] array = new FP[shapeCount * 2];
            int index = 0;
            for (int i = 0; i < shapeCount; i++)
            {
                PolygonShape shape;
                CircleShape shape2 = shapes[i].Shape as CircleShape;
                if (shape2 > null)
                {
                    Vertices vertices = new Vertices();
                    TSVector2 item = TSVector2.zero + new TSVector2(shape2.Radius, 0);
                    vertices.Add(item);
                    item = TSVector2.zero + new TSVector2(0, shape2.Radius);
                    vertices.Add(item);
                    item = TSVector2.zero + new TSVector2(-shape2.Radius, shape2.Radius);
                    vertices.Add(item);
                    item = TSVector2.zero + new TSVector2(0, -shape2.Radius);
                    vertices.Add(item);
                    shape = new PolygonShape(vertices, 0);
                }
                else
                {
                    shape = shapes[i].Shape as PolygonShape;
                }
                if ((shapes[i].Body.BodyType == BodyType.Dynamic) && (shape > null))
                {
                    TSVector2 vector2 = shapes[i].Body.GetWorldPoint(shape.MassData.Centroid) - pos;
                    FP fp = FP.Atan2(vector2.y, vector2.x);
                    FP maxValue = FP.MaxValue;
                    FP minValue = FP.MinValue;
                    FP fp4 = 0f;
                    FP fp5 = 0f;
                    for (int n = 0; n < shape.Vertices.Count; n++)
                    {
                        TSVector2 vector3 = shapes[i].Body.GetWorldPoint(shape.Vertices[n]) - pos;
                        FP fp6 = FP.Atan2(vector3.y, vector3.x);
                        FP fp7 = fp6 - fp;
                        fp7 = (fp7 - MathHelper.Pi) % (2 * MathHelper.Pi);
                        if (fp7 < 0f)
                        {
                            fp7 += 2 * MathHelper.Pi;
                        }
                        fp7 -= MathHelper.Pi;
                        if (FP.Abs(fp7) <= MathHelper.Pi)
                        {
                            if (fp7 > minValue)
                            {
                                minValue = fp7;
                                fp5 = fp6;
                            }
                            if (fp7 < maxValue)
                            {
                                maxValue = fp7;
                                fp4 = fp6;
                            }
                        }
                    }
                    array[index] = fp4;
                    index++;
                    array[index] = fp5;
                    index++;
                }
            }
            Array.Sort<FP>(array, 0, index, this._rdc);
            this._data.Clear();
            bool flag = true;
            for (int j = 0; j < index; j++)
            {
                bool hitClosest;
                Fixture fixture = null;
                int num5 = (j == (index - 1)) ? 0 : (j + 1);
                if (array[j] != array[num5])
                {
                    FP fp8;
                    if (j == (index - 1))
                    {
                        fp8 = (array[0] + (MathHelper.Pi * 2)) + array[j];
                    }
                    else
                    {
                        fp8 = array[j + 1] + array[j];
                    }
                    fp8 /= 2;
                    TSVector2 vector4 = pos;
                    TSVector2 vector5 = ((TSVector2) (radius * new TSVector2(FP.Cos(fp8), FP.Sin(fp8)))) + pos;
                    hitClosest = false;
                    base.World.RayCast(delegate (Fixture f, TSVector2 p, TSVector2 n, FP fr) {
                        Body body = f.Body;
                        if (!this.IsActiveOn(body))
                        {
                            return 0;
                        }
                        hitClosest = true;
                        fixture = f;
                        return fr;
                    }, vector4, vector5);
                    if (hitClosest && (fixture.Body.BodyType == BodyType.Dynamic))
                    {
                        if ((this._data.Any<ShapeData>() && (this._data.Last<ShapeData>().Body == fixture.Body)) && !flag)
                        {
                            int num7 = this._data.Count - 1;
                            ShapeData data2 = this._data[num7];
                            data2.Max = array[num5];
                            this._data[num7] = data2;
                        }
                        else
                        {
                            ShapeData data3;
                            data3.Body = fixture.Body;
                            data3.Min = array[j];
                            data3.Max = array[num5];
                            this._data.Add(data3);
                        }
                        if ((((this._data.Count > 1) && (j == (index - 1))) && (this._data.Last<ShapeData>().Body == this._data.First<ShapeData>().Body)) && (this._data.Last<ShapeData>().Max == this._data.First<ShapeData>().Min))
                        {
                            ShapeData data4 = this._data[0];
                            data4.Min = this._data.Last<ShapeData>().Min;
                            this._data.RemoveAt(this._data.Count - 1);
                            this._data[0] = data4;
                            while (this._data.First<ShapeData>().Min >= this._data.First<ShapeData>().Max)
                            {
                                data4.Min -= MathHelper.Pi * 2;
                                this._data[0] = data4;
                            }
                        }
                        int num6 = this._data.Count - 1;
                        ShapeData data = this._data[num6];
                        while ((this._data.Count > 0) && (this._data.Last<ShapeData>().Min >= this._data.Last<ShapeData>().Max))
                        {
                            data.Min = this._data.Last<ShapeData>().Min - (2 * MathHelper.Pi);
                            this._data[num6] = data;
                        }
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            for (int k = 0; k < this._data.Count; k++)
            {
                if (this.IsActiveOn(this._data[k].Body))
                {
                    FP fp9 = this._data[k].Max - this._data[k].Min;
                    FP fp10 = MathHelper.Min(MaxEdgeOffset, this.EdgeRatio * fp9);
                    int num9 = FP.Ceiling(((fp9 - (2f * fp10)) - ((this.MinRays - 1) * this.MaxAngle)) / this.MaxAngle).AsInt();
                    if (num9 < 0)
                    {
                        num9 = 0;
                    }
                    FP fp11 = (fp9 - (fp10 * 2f)) / ((this.MinRays + num9) - 1);
                    for (FP fp13 = this._data[k].Min + fp10; (fp13 < this._data[k].Max) || MathUtils.FPEquals(fp13, this._data[k].Max, 0.0001f); fp13 += fp11)
                    {
                        TSVector2 vector6 = pos;
                        TSVector2 vector7 = pos + (radius * new TSVector2(FP.Cos(fp13), FP.Sin(fp13)));
                        TSVector2 zero = TSVector2.zero;
                        FP fraction = FP.MaxValue;
                        List<Fixture> fixtureList = this._data[k].Body.FixtureList;
                        for (int num10 = 0; num10 < fixtureList.Count; num10++)
                        {
                            RayCastInput input;
                            RayCastOutput output;
                            Fixture key = fixtureList[num10];
                            input.Point1 = vector6;
                            input.Point2 = vector7;
                            input.MaxFraction = 50f;
                            if (key.RayCast(out output, ref input, 0) && (fraction > output.Fraction))
                            {
                                fraction = output.Fraction;
                                zero = (TSVector2) ((output.Fraction * vector7) + ((1 - output.Fraction) * vector6));
                            }
                            FP fp15 = ((((fp9 / (this.MinRays + num9)) * maxForce) * 180f) / MathHelper.Pi) * (1f - TSMath.Min(FP.One, fraction));
                            TSVector2 impulse = (TSVector2) (TSVector2.Dot((TSVector2) (fp15 * new TSVector2(FP.Cos(fp13), FP.Sin(fp13))), -output.Normal) * new TSVector2(FP.Cos(fp13), FP.Sin(fp13)));
                            this._data[k].Body.ApplyLinearImpulse(ref impulse, ref zero);
                            if (dictionary.ContainsKey(key))
                            {
                                Dictionary<Fixture, TSVector2> dictionary3 = dictionary;
                                Fixture fixture2 = key;
                                dictionary3[fixture2] += impulse;
                            }
                            else
                            {
                                dictionary.Add(key, impulse);
                            }
                            if (fraction > 1f)
                            {
                                zero = vector7;
                            }
                        }
                    }
                }
            }
            for (int m = 0; m < containedShapeCount; m++)
            {
                Fixture fixture3 = containedShapes[m];
                if (this.IsActiveOn(fixture3.Body))
                {
                    TSVector2 worldPoint;
                    FP fp16 = ((this.MinRays * maxForce) * 180f) / MathHelper.Pi;
                    CircleShape shape3 = fixture3.Shape as CircleShape;
                    if (shape3 > null)
                    {
                        worldPoint = fixture3.Body.GetWorldPoint(shape3.Position);
                    }
                    else
                    {
                        PolygonShape shape4 = fixture3.Shape as PolygonShape;
                        worldPoint = fixture3.Body.GetWorldPoint(shape4.MassData.Centroid);
                    }
                    TSVector2 vector11 = (TSVector2) (fp16 * (worldPoint - pos));
                    fixture3.Body.ApplyLinearImpulse(ref vector11, ref worldPoint);
                    if (!dictionary.ContainsKey(fixture3))
                    {
                        dictionary.Add(fixture3, vector11);
                    }
                }
            }
            return dictionary;
        }
    }
}

