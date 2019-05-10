using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class DistanceProxy
	{
		internal FP Radius;

		internal Vertices Vertices = new Vertices();

		public void Set(Shape shape, int index)
		{
			switch (shape.ShapeType)
			{
			case ShapeType.Circle:
			{
				CircleShape circleShape = (CircleShape)shape;
				this.Vertices.Clear();
				this.Vertices.Add(circleShape.Position);
				this.Radius = circleShape.Radius;
				break;
			}
			case ShapeType.Edge:
			{
				EdgeShape edgeShape = (EdgeShape)shape;
				this.Vertices.Clear();
				this.Vertices.Add(edgeShape.Vertex1);
				this.Vertices.Add(edgeShape.Vertex2);
				this.Radius = edgeShape.Radius;
				break;
			}
			case ShapeType.Polygon:
			{
				PolygonShape polygonShape = (PolygonShape)shape;
				this.Vertices.Clear();
				for (int i = 0; i < polygonShape.Vertices.Count; i++)
				{
					this.Vertices.Add(polygonShape.Vertices[i]);
				}
				this.Radius = polygonShape.Radius;
				break;
			}
			case ShapeType.Chain:
			{
				ChainShape chainShape = (ChainShape)shape;
				Debug.Assert(0 <= index && index < chainShape.Vertices.Count);
				this.Vertices.Clear();
				this.Vertices.Add(chainShape.Vertices[index]);
				this.Vertices.Add((index + 1 < chainShape.Vertices.Count) ? chainShape.Vertices[index + 1] : chainShape.Vertices[0]);
				this.Radius = chainShape.Radius;
				break;
			}
			default:
				Debug.Assert(false);
				break;
			}
		}

		public int GetSupport(TSVector2 direction)
		{
			int result = 0;
			FP y = TSVector2.Dot(this.Vertices[0], direction);
			for (int i = 1; i < this.Vertices.Count; i++)
			{
				FP fP = TSVector2.Dot(this.Vertices[i], direction);
				bool flag = fP > y;
				if (flag)
				{
					result = i;
					y = fP;
				}
			}
			return result;
		}

		public TSVector2 GetSupportVertex(TSVector2 direction)
		{
			int index = 0;
			FP y = TSVector2.Dot(this.Vertices[0], direction);
			for (int i = 1; i < this.Vertices.Count; i++)
			{
				FP fP = TSVector2.Dot(this.Vertices[i], direction);
				bool flag = fP > y;
				if (flag)
				{
					index = i;
					y = fP;
				}
			}
			return this.Vertices[index];
		}
	}
}
