using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class PolygonTools
	{
		public static void TransformVertices(Vertices vertices, TSVector2 center, FP angle)
		{
			Transform transform = default(Transform);
			transform.p = center;
			transform.q.Set(angle);
			for (int i = 0; i < vertices.Count; i++)
			{
				vertices[i] = MathUtils.Mul(ref transform, vertices[i]) - center;
			}
		}

		public static Vertices CreateRectangle(FP hx, FP hy)
		{
			return new Vertices(4)
			{
				new TSVector2(-hx, -hy),
				new TSVector2(hx, -hy),
				new TSVector2(hx, hy),
				new TSVector2(-hx, hy)
			};
		}

		public static Vertices CreateRectangle(FP hx, FP hy, TSVector2 center, FP angle)
		{
			Vertices vertices = PolygonTools.CreateRectangle(hx, hy);
			Transform transform = default(Transform);
			transform.p = center;
			transform.q.Set(angle);
			for (int i = 0; i < 4; i++)
			{
				vertices[i] = MathUtils.Mul(ref transform, vertices[i]) - center;
			}
			return vertices;
		}

		public static Vertices CreateRoundedRectangle(FP width, FP height, FP xRadius, FP yRadius, int segments)
		{
			bool flag = yRadius > height / 2 || xRadius > width / 2;
			if (flag)
			{
				throw new Exception("Rounding amount can't be more than half the height and width respectively.");
			}
			bool flag2 = segments < 0;
			if (flag2)
			{
				throw new Exception("Segments must be zero or more.");
			}
			Debug.Assert(Settings.MaxPolygonVertices >= 8);
			Vertices vertices = new Vertices();
			bool flag3 = segments == 0;
			if (flag3)
			{
				vertices.Add(new TSVector2(width * 0.5f - xRadius, -height * 0.5f));
				vertices.Add(new TSVector2(width * 0.5f, -height * 0.5f + yRadius));
				vertices.Add(new TSVector2(width * 0.5f, height * 0.5f - yRadius));
				vertices.Add(new TSVector2(width * 0.5f - xRadius, height * 0.5f));
				vertices.Add(new TSVector2(-width * 0.5f + xRadius, height * 0.5f));
				vertices.Add(new TSVector2(-width * 0.5f, height * 0.5f - yRadius));
				vertices.Add(new TSVector2(-width * 0.5f, -height * 0.5f + yRadius));
				vertices.Add(new TSVector2(-width * 0.5f + xRadius, -height * 0.5f));
			}
			else
			{
				int num = segments * 4 + 8;
				FP x = MathHelper.TwoPi / (num - 4);
				int num2 = num / 4;
				TSVector2 value = new TSVector2(width / 2 - xRadius, height / 2 - yRadius);
				vertices.Add(value + new TSVector2(xRadius, -yRadius + yRadius));
				short num3 = 0;
				for (int i = 1; i < num; i++)
				{
					bool flag4 = i - num2 == 0 || i - num2 * 3 == 0;
					if (flag4)
					{
						value.x *= -1;
						num3 -= 1;
					}
					else
					{
						bool flag5 = i - num2 * 2 == 0;
						if (flag5)
						{
							value.y *= -1;
							num3 -= 1;
						}
					}
					vertices.Add(value + new TSVector2(xRadius * FP.Cos(x * -(i + (int)num3)), -yRadius * FP.Sin(x * -(i + (int)num3))));
				}
			}
			return vertices;
		}

		public static Vertices CreateLine(TSVector2 start, TSVector2 end)
		{
			return new Vertices(2)
			{
				start,
				end
			};
		}

		public static Vertices CreateCircle(FP radius, int numberOfEdges)
		{
			return PolygonTools.CreateEllipse(radius, radius, numberOfEdges);
		}

		public static Vertices CreateEllipse(FP xRadius, FP yRadius, int numberOfEdges)
		{
			Vertices vertices = new Vertices();
			FP x = MathHelper.TwoPi / numberOfEdges;
			vertices.Add(new TSVector2(xRadius, 0));
			for (int i = numberOfEdges - 1; i > 0; i--)
			{
				vertices.Add(new TSVector2(xRadius * FP.Cos(x * i), -yRadius * FP.Sin(x * i)));
			}
			return vertices;
		}

		public static Vertices CreateArc(FP radians, int sides, FP radius)
		{
			Debug.Assert(radians > 0, "The arc needs to be larger than 0");
			Debug.Assert(sides > 1, "The arc needs to have more than 1 sides");
			Debug.Assert(radius > 0, "The arc needs to have a radius larger than 0");
			Vertices vertices = new Vertices();
			FP x = radians / sides;
			for (int i = sides - 1; i > 0; i--)
			{
				vertices.Add(new TSVector2(radius * FP.Cos(x * i), radius * FP.Sin(x * i)));
			}
			return vertices;
		}

		public static Vertices CreateCapsule(FP height, FP endRadius, int edges)
		{
			bool flag = endRadius >= height / 2;
			if (flag)
			{
				throw new ArgumentException("The radius must be lower than height / 2. Higher values of radius would create a circle, and not a half circle.", "endRadius");
			}
			return PolygonTools.CreateCapsule(height, endRadius, edges, endRadius, edges);
		}

		public static Vertices CreateCapsule(FP height, FP topRadius, int topEdges, FP bottomRadius, int bottomEdges)
		{
			bool flag = height <= 0;
			if (flag)
			{
				throw new ArgumentException("Height must be longer than 0", "height");
			}
			bool flag2 = topRadius <= 0;
			if (flag2)
			{
				throw new ArgumentException("The top radius must be more than 0", "topRadius");
			}
			bool flag3 = topEdges <= 0;
			if (flag3)
			{
				throw new ArgumentException("Top edges must be more than 0", "topEdges");
			}
			bool flag4 = bottomRadius <= 0;
			if (flag4)
			{
				throw new ArgumentException("The bottom radius must be more than 0", "bottomRadius");
			}
			bool flag5 = bottomEdges <= 0;
			if (flag5)
			{
				throw new ArgumentException("Bottom edges must be more than 0", "bottomEdges");
			}
			bool flag6 = topRadius >= height / 2;
			if (flag6)
			{
				throw new ArgumentException("The top radius must be lower than height / 2. Higher values of top radius would create a circle, and not a half circle.", "topRadius");
			}
			bool flag7 = bottomRadius >= height / 2;
			if (flag7)
			{
				throw new ArgumentException("The bottom radius must be lower than height / 2. Higher values of bottom radius would create a circle, and not a half circle.", "bottomRadius");
			}
			Vertices vertices = new Vertices();
			FP fP = (height - topRadius - bottomRadius) * 0.5f;
			vertices.Add(new TSVector2(topRadius, fP));
			FP x = MathHelper.Pi / topEdges;
			for (int i = 1; i < topEdges; i++)
			{
				vertices.Add(new TSVector2(topRadius * FP.Cos(x * i), topRadius * FP.Sin(x * i) + fP));
			}
			vertices.Add(new TSVector2(-topRadius, fP));
			vertices.Add(new TSVector2(-bottomRadius, -fP));
			x = MathHelper.Pi / bottomEdges;
			for (int j = 1; j < bottomEdges; j++)
			{
				vertices.Add(new TSVector2(-bottomRadius * FP.Cos(x * j), -bottomRadius * FP.Sin(x * j) - fP));
			}
			vertices.Add(new TSVector2(bottomRadius, -fP));
			return vertices;
		}

		public static Vertices CreateGear(FP radius, int numberOfTeeth, FP tipPercentage, FP toothHeight)
		{
			Vertices vertices = new Vertices();
			FP x = MathHelper.TwoPi / numberOfTeeth;
			tipPercentage /= 100f;
			MathHelper.Clamp(tipPercentage, 0f, 1f);
			FP fP = x / 2f * tipPercentage;
			FP fP2 = (x - fP * 2f) / 2f;
			for (int i = numberOfTeeth - 1; i >= 0; i--)
			{
				bool flag = fP > 0f;
				if (flag)
				{
					vertices.Add(new TSVector2(radius * FP.Cos(x * i + fP2 * 2f + fP), -radius * FP.Sin(x * i + fP2 * 2f + fP)));
					vertices.Add(new TSVector2((radius + toothHeight) * FP.Cos(x * i + fP2 + fP), -(radius + toothHeight) * FP.Sin(x * i + fP2 + fP)));
				}
				vertices.Add(new TSVector2((radius + toothHeight) * FP.Cos(x * i + fP2), -(radius + toothHeight) * FP.Sin(x * i + fP2)));
				vertices.Add(new TSVector2(radius * FP.Cos(x * i), -radius * FP.Sin(x * i)));
			}
			return vertices;
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
	}
}
