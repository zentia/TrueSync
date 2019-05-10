using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class TriangleMeshShape : Multishape
	{
		private List<int> potentialTriangles = new List<int>();

		private Octree octree = null;

		private FP sphericalExpansion = FP.EN2;

		private TSVector[] vecs = new TSVector[3];

		private bool flipNormal = false;

		private TSVector normal = TSVector.up;

		public FP SphericalExpansion
		{
			get
			{
				return this.sphericalExpansion;
			}
			set
			{
				this.sphericalExpansion = value;
			}
		}

		public bool FlipNormals
		{
			get
			{
				return this.flipNormal;
			}
			set
			{
				this.flipNormal = value;
			}
		}

		public TriangleMeshShape(Octree octree)
		{
			this.octree = octree;
			this.UpdateShape();
		}

		internal TriangleMeshShape()
		{
		}

		protected override Multishape CreateWorkingClone()
		{
			return new TriangleMeshShape(this.octree)
			{
				sphericalExpansion = this.sphericalExpansion
			};
		}

		public override int Prepare(ref TSBBox box)
		{
			this.potentialTriangles.Clear();
			TSBBox tSBBox = box;
			tSBBox.min.x = tSBBox.min.x - this.sphericalExpansion;
			tSBBox.min.y = tSBBox.min.y - this.sphericalExpansion;
			tSBBox.min.z = tSBBox.min.z - this.sphericalExpansion;
			tSBBox.max.x = tSBBox.max.x + this.sphericalExpansion;
			tSBBox.max.y = tSBBox.max.y + this.sphericalExpansion;
			tSBBox.max.z = tSBBox.max.z + this.sphericalExpansion;
			this.octree.GetTrianglesIntersectingtAABox(this.potentialTriangles, ref tSBBox);
			return this.potentialTriangles.Count;
		}

		public override void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
		{
			TSBBox largeBox = TSBBox.LargeBox;
			List<int> list = new List<int>();
			this.octree.GetTrianglesIntersectingtAABox(list, ref largeBox);
			for (int i = 0; i < list.Count; i++)
			{
				triangleList.Add(this.octree.GetVertex(this.octree.GetTriangleVertexIndex(i).I0));
				triangleList.Add(this.octree.GetVertex(this.octree.GetTriangleVertexIndex(i).I1));
				triangleList.Add(this.octree.GetVertex(this.octree.GetTriangleVertexIndex(i).I2));
			}
		}

		public override int Prepare(ref TSVector rayOrigin, ref TSVector rayDelta)
		{
			this.potentialTriangles.Clear();
			TSVector tSVector;
			TSVector.Normalize(ref rayDelta, out tSVector);
			tSVector = rayDelta + tSVector * this.sphericalExpansion;
			this.octree.GetTrianglesIntersectingRay(this.potentialTriangles, rayOrigin, tSVector);
			return this.potentialTriangles.Count;
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			TSVector tSVector;
			TSVector.Normalize(ref direction, out tSVector);
			tSVector *= this.sphericalExpansion;
			FP y = TSVector.Dot(ref this.vecs[0], ref direction);
			int num = 0;
			FP fP = TSVector.Dot(ref this.vecs[1], ref direction);
			bool flag = fP > y;
			if (flag)
			{
				y = fP;
				num = 1;
			}
			fP = TSVector.Dot(ref this.vecs[2], ref direction);
			bool flag2 = fP > y;
			if (flag2)
			{
				num = 2;
			}
			result = this.vecs[num] + tSVector;
		}

		public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
		{
			box = this.octree.rootNodeBox;
			box.min.x = box.min.x - this.sphericalExpansion;
			box.min.y = box.min.y - this.sphericalExpansion;
			box.min.z = box.min.z - this.sphericalExpansion;
			box.max.x = box.max.x + this.sphericalExpansion;
			box.max.y = box.max.y + this.sphericalExpansion;
			box.max.z = box.max.z + this.sphericalExpansion;
			box.Transform(ref orientation);
		}

		public override void SetCurrentShape(int index)
		{
			this.vecs[0] = this.octree.GetVertex(this.octree.tris[this.potentialTriangles[index]].I0);
			this.vecs[1] = this.octree.GetVertex(this.octree.tris[this.potentialTriangles[index]].I1);
			this.vecs[2] = this.octree.GetVertex(this.octree.tris[this.potentialTriangles[index]].I2);
			TSVector geomCen = this.vecs[0];
			TSVector.Add(ref geomCen, ref this.vecs[1], out geomCen);
			TSVector.Add(ref geomCen, ref this.vecs[2], out geomCen);
			TSVector.Multiply(ref geomCen, FP.One / (3 * FP.One), out geomCen);
			this.geomCen = geomCen;
			TSVector.Subtract(ref this.vecs[1], ref this.vecs[0], out geomCen);
			TSVector.Subtract(ref this.vecs[2], ref this.vecs[0], out this.normal);
			TSVector.Cross(ref geomCen, ref this.normal, out this.normal);
			bool flag = this.flipNormal;
			if (flag)
			{
				this.normal.Negate();
			}
		}

		public void CollisionNormal(out TSVector normal)
		{
			normal = this.normal;
		}
	}
}
