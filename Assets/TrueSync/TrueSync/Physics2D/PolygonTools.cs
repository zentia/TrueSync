namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    public static class PolygonTools
    {
        public static Vertices CreateArc(FP radians, int sides, FP radius)
        {
            Debug.Assert(radians > 0, "The arc needs to be larger than 0");
            Debug.Assert(sides > 1, "The arc needs to have more than 1 sides");
            Debug.Assert(radius > 0, "The arc needs to have a radius larger than 0");
            Vertices vertices = new Vertices();
            FP fp = radians / sides;
            for (int i = sides - 1; i > 0; i--)
            {
                vertices.Add(new TSVector2(radius * FP.Cos(fp * i), radius * FP.Sin(fp * i)));
            }
            return vertices;
        }

        public static Vertices CreateCapsule(FP height, FP endRadius, int edges)
        {
            if (endRadius >= (height / 2))
            {
                throw new ArgumentException("The radius must be lower than height / 2. Higher values of radius would create a circle, and not a half circle.", "endRadius");
            }
            return CreateCapsule(height, endRadius, edges, endRadius, edges);
        }

        public static Vertices CreateCapsule(FP height, FP topRadius, int topEdges, FP bottomRadius, int bottomEdges)
        {
            if (height <= 0)
            {
                throw new ArgumentException("Height must be longer than 0", "height");
            }
            if (topRadius <= 0)
            {
                throw new ArgumentException("The top radius must be more than 0", "topRadius");
            }
            if (topEdges <= 0)
            {
                throw new ArgumentException("Top edges must be more than 0", "topEdges");
            }
            if (bottomRadius <= 0)
            {
                throw new ArgumentException("The bottom radius must be more than 0", "bottomRadius");
            }
            if (bottomEdges <= 0)
            {
                throw new ArgumentException("Bottom edges must be more than 0", "bottomEdges");
            }
            if (topRadius >= (height / 2))
            {
                throw new ArgumentException("The top radius must be lower than height / 2. Higher values of top radius would create a circle, and not a half circle.", "topRadius");
            }
            if (bottomRadius >= (height / 2))
            {
                throw new ArgumentException("The bottom radius must be lower than height / 2. Higher values of bottom radius would create a circle, and not a half circle.", "bottomRadius");
            }
            Vertices vertices = new Vertices();
            FP y = ((height - topRadius) - bottomRadius) * 0.5f;
            vertices.Add(new TSVector2(topRadius, y));
            FP fp2 = MathHelper.Pi / topEdges;
            for (int i = 1; i < topEdges; i++)
            {
                vertices.Add(new TSVector2(topRadius * FP.Cos(fp2 * i), (topRadius * FP.Sin(fp2 * i)) + y));
            }
            vertices.Add(new TSVector2(-topRadius, y));
            vertices.Add(new TSVector2(-bottomRadius, -y));
            fp2 = MathHelper.Pi / bottomEdges;
            for (int j = 1; j < bottomEdges; j++)
            {
                vertices.Add(new TSVector2(-bottomRadius * FP.Cos(fp2 * j), (-bottomRadius * FP.Sin(fp2 * j)) - y));
            }
            vertices.Add(new TSVector2(bottomRadius, -y));
            return vertices;
        }

        public static Vertices CreateCircle(FP radius, int numberOfEdges)
        {
            return CreateEllipse(radius, radius, numberOfEdges);
        }

        public static Vertices CreateEllipse(FP xRadius, FP yRadius, int numberOfEdges)
        {
            Vertices vertices = new Vertices();
            FP fp = MathHelper.TwoPi / numberOfEdges;
            vertices.Add(new TSVector2(xRadius, 0));
            for (int i = numberOfEdges - 1; i > 0; i--)
            {
                vertices.Add(new TSVector2(xRadius * FP.Cos(fp * i), -yRadius * FP.Sin(fp * i)));
            }
            return vertices;
        }

        public static Vertices CreateGear(FP radius, int numberOfTeeth, FP tipPercentage, FP toothHeight)
        {
            Vertices vertices = new Vertices();
            FP fp = MathHelper.TwoPi / numberOfTeeth;
            tipPercentage /= 100f;
            MathHelper.Clamp(tipPercentage, 0f, 1f);
            FP fp2 = (fp / 2f) * tipPercentage;
            FP fp3 = (fp - (fp2 * 2f)) / 2f;
            for (int i = numberOfTeeth - 1; i >= 0; i--)
            {
                if (fp2 > 0f)
                {
                    vertices.Add(new TSVector2(radius * FP.Cos(((fp * i) + (fp3 * 2f)) + fp2), -radius * FP.Sin(((fp * i) + (fp3 * 2f)) + fp2)));
                    vertices.Add(new TSVector2((radius + toothHeight) * FP.Cos(((fp * i) + fp3) + fp2), -(radius + toothHeight) * FP.Sin(((fp * i) + fp3) + fp2)));
                }
                vertices.Add(new TSVector2((radius + toothHeight) * FP.Cos((fp * i) + fp3), -(radius + toothHeight) * FP.Sin((fp * i) + fp3)));
                vertices.Add(new TSVector2(radius * FP.Cos(fp * i), -radius * FP.Sin(fp * i)));
            }
            return vertices;
        }

        public static Vertices CreateLine(TSVector2 start, TSVector2 end)
        {
            return new Vertices(2) { start, end };
        }

        public static Vertices CreatePolygon(uint[] data, int width)
        {
            return TextureConverter.DetectVertices(data, width);
        }

        public static Vertices CreatePolygon(uint[] data, int width, bool holeDetection)
        {
            return TextureConverter.DetectVertices(data, width, holeDetection);
        }

        public static List<Vertices> CreatePolygon(uint[] data, int width, FP hullTolerance, byte alphaTolerance, bool multiPartDetection, bool holeDetection)
        {
            return TextureConverter.DetectVertices(data, width, hullTolerance, alphaTolerance, multiPartDetection, holeDetection);
        }

        public static Vertices CreateRectangle(FP hx, FP hy)
        {
            return new Vertices(4) { new TSVector2(-hx, -hy), new TSVector2(hx, -hy), new TSVector2(hx, hy), new TSVector2(-hx, hy) };
        }

        public static Vertices CreateRectangle(FP hx, FP hy, TSVector2 center, FP angle)
        {
            Vertices vertices = CreateRectangle(hx, hy);
            Transform t = new Transform {
                p = center
            };
            t.q.Set(angle);
            for (int i = 0; i < 4; i++)
            {
                vertices[i] = MathUtils.Mul(ref t, vertices[i]) - center;
            }
            return vertices;
        }

        public static Vertices CreateRoundedRectangle(FP width, FP height, FP xRadius, FP yRadius, int segments)
        {
            if ((yRadius > (height / 2)) || (xRadius > (width / 2)))
            {
                throw new Exception("Rounding amount can't be more than half the height and width respectively.");
            }
            if (segments < 0)
            {
                throw new Exception("Segments must be zero or more.");
            }
            Debug.Assert(Settings.MaxPolygonVertices >= 8);
            Vertices vertices = new Vertices();
            if (segments == 0)
            {
                vertices.Add(new TSVector2((width * 0.5f) - xRadius, -height * 0.5f));
                vertices.Add(new TSVector2(width * 0.5f, (-height * 0.5f) + yRadius));
                vertices.Add(new TSVector2(width * 0.5f, (height * 0.5f) - yRadius));
                vertices.Add(new TSVector2((width * 0.5f) - xRadius, height * 0.5f));
                vertices.Add(new TSVector2((-width * 0.5f) + xRadius, height * 0.5f));
                vertices.Add(new TSVector2(-width * 0.5f, (height * 0.5f) - yRadius));
                vertices.Add(new TSVector2(-width * 0.5f, (-height * 0.5f) + yRadius));
                vertices.Add(new TSVector2((-width * 0.5f) + xRadius, -height * 0.5f));
                return vertices;
            }
            int num = (segments * 4) + 8;
            FP fp = MathHelper.TwoPi / (num - 4);
            int num2 = num / 4;
            TSVector2 vector = new TSVector2((width / 2) - xRadius, (height / 2) - yRadius);
            vertices.Add(vector + new TSVector2(xRadius, -yRadius + yRadius));
            short num3 = 0;
            for (int i = 1; i < num; i++)
            {
                if (((i - num2) == 0) || ((i - (num2 * 3)) == 0))
                {
                    vector.x *= -1;
                    num3 = (short) (num3 - 1);
                }
                else if ((i - (num2 * 2)) == 0)
                {
                    vector.y *= -1;
                    num3 = (short) (num3 - 1);
                }
                vertices.Add(vector + new TSVector2(xRadius * FP.Cos(fp * -(i + num3)), -yRadius * FP.Sin(fp * -(i + num3))));
            }
            return vertices;
        }

        public static void TransformVertices(Vertices vertices, TSVector2 center, FP angle)
        {
            Transform t = new Transform {
                p = center
            };
            t.q.Set(angle);
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = MathUtils.Mul(ref t, vertices[i]) - center;
            }
        }
    }
}

