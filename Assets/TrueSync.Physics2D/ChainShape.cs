using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class ChainShape : Shape
	{
		public Vertices Vertices;

		private TSVector2 _prevVertex;

		private TSVector2 _nextVertex;

		private bool _hasPrevVertex;

		private bool _hasNextVertex;

		private static EdgeShape _edgeShape = new EdgeShape();

		public override int ChildCount
		{
			get
			{
				return this.Vertices.Count - 1;
			}
		}

		public TSVector2 PrevVertex
		{
			get
			{
				return this._prevVertex;
			}
			set
			{
				this._prevVertex = value;
				this._hasPrevVertex = true;
			}
		}

		public TSVector2 NextVertex
		{
			get
			{
				return this._nextVertex;
			}
			set
			{
				this._nextVertex = value;
				this._hasNextVertex = true;
			}
		}

		public ChainShape() : base(0)
		{
			base.ShapeType = ShapeType.Chain;
			this._radius = Settings.PolygonRadius;
		}

		public ChainShape(Vertices vertices, bool createLoop = false) : base(0)
		{
			base.ShapeType = ShapeType.Chain;
			this._radius = Settings.PolygonRadius;
			Debug.Assert(vertices != null && vertices.Count >= 3);
			Debug.Assert(vertices[0] != vertices[vertices.Count - 1]);
			for (int i = 1; i < vertices.Count; i++)
			{
				TSVector2 value = vertices[i - 1];
				TSVector2 value2 = vertices[i];
				Debug.Assert(TSVector2.DistanceSquared(value, value2) > Settings.LinearSlop * Settings.LinearSlop);
			}
			this.Vertices = new Vertices(vertices);
			if (createLoop)
			{
				this.Vertices.Add(vertices[0]);
				this.PrevVertex = this.Vertices[this.Vertices.Count - 2];
				this.NextVertex = this.Vertices[1];
			}
		}

		internal void GetChildEdge(EdgeShape edge, int index)
		{
			Debug.Assert(0 <= index && index < this.Vertices.Count - 1);
			Debug.Assert(edge != null);
			edge.ShapeType = ShapeType.Edge;
			edge._radius = this._radius;
			edge.Vertex1 = this.Vertices[index];
			edge.Vertex2 = this.Vertices[index + 1];
			bool flag = index > 0;
			if (flag)
			{
				edge.Vertex0 = this.Vertices[index - 1];
				edge.HasVertex0 = true;
			}
			else
			{
				edge.Vertex0 = this._prevVertex;
				edge.HasVertex0 = this._hasPrevVertex;
			}
			bool flag2 = index < this.Vertices.Count - 2;
			if (flag2)
			{
				edge.Vertex3 = this.Vertices[index + 2];
				edge.HasVertex3 = true;
			}
			else
			{
				edge.Vertex3 = this._nextVertex;
				edge.HasVertex3 = this._hasNextVertex;
			}
		}

		public EdgeShape GetChildEdge(int index)
		{
			EdgeShape edgeShape = new EdgeShape();
			this.GetChildEdge(edgeShape, index);
			return edgeShape;
		}

		public override bool TestPoint(ref Transform transform, ref TSVector2 point)
		{
			return false;
		}

		public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
		{
			Debug.Assert(childIndex < this.Vertices.Count);
			int num = childIndex + 1;
			bool flag = num == this.Vertices.Count;
			if (flag)
			{
				num = 0;
			}
			ChainShape._edgeShape.Vertex1 = this.Vertices[childIndex];
			ChainShape._edgeShape.Vertex2 = this.Vertices[num];
			return ChainShape._edgeShape.RayCast(out output, ref input, ref transform, 0);
		}

		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			Debug.Assert(childIndex < this.Vertices.Count);
			int num = childIndex + 1;
			bool flag = num == this.Vertices.Count;
			if (flag)
			{
				num = 0;
			}
			TSVector2 value = MathUtils.Mul(ref transform, this.Vertices[childIndex]);
			TSVector2 value2 = MathUtils.Mul(ref transform, this.Vertices[num]);
			aabb.LowerBound = TSVector2.Min(value, value2);
			aabb.UpperBound = TSVector2.Max(value, value2);
		}

		protected override void ComputeProperties()
		{
		}

		public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
		{
			sc = TSVector2.zero;
			return 0;
		}

		public bool CompareTo(ChainShape shape)
		{
			bool flag = this.Vertices.Count != shape.Vertices.Count;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < this.Vertices.Count; i++)
				{
					bool flag2 = this.Vertices[i] != shape.Vertices[i];
					if (flag2)
					{
						result = false;
						return result;
					}
				}
				result = (this.PrevVertex == shape.PrevVertex && this.NextVertex == shape.NextVertex);
			}
			return result;
		}

		public override Shape Clone()
		{
			return new ChainShape
			{
				ShapeType = base.ShapeType,
				_density = this._density,
				_radius = this._radius,
				PrevVertex = this._prevVertex,
				NextVertex = this._nextVertex,
				_hasNextVertex = this._hasNextVertex,
				_hasPrevVertex = this._hasPrevVertex,
				Vertices = new Vertices(this.Vertices),
				MassData = this.MassData
			};
		}
	}
}
