namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class TriangleMeshShape : Multishape
    {
        private bool flipNormal;
        private TSVector normal;
        private Octree octree;
        private List<int> potentialTriangles;
        private FP sphericalExpansion;
        private TSVector[] vecs;

        internal TriangleMeshShape()
        {
            this.potentialTriangles = new List<int>();
            this.octree = null;
            this.sphericalExpansion = FP.EN2;
            this.vecs = new TSVector[3];
            this.flipNormal = false;
            this.normal = TSVector.up;
        }

        public TriangleMeshShape(Octree octree)
        {
            this.potentialTriangles = new List<int>();
            this.octree = null;
            this.sphericalExpansion = FP.EN2;
            this.vecs = new TSVector[3];
            this.flipNormal = false;
            this.normal = TSVector.up;
            this.octree = octree;
            this.UpdateShape();
        }

        public void CollisionNormal(out TSVector normal)
        {
            normal = this.normal;
        }

        protected override Multishape CreateWorkingClone()
        {
            return new TriangleMeshShape(this.octree) { sphericalExpansion = this.sphericalExpansion };
        }

        public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
        {
            box = this.octree.rootNodeBox;
            box.min.x -= this.sphericalExpansion;
            box.min.y -= this.sphericalExpansion;
            box.min.z -= this.sphericalExpansion;
            box.max.x += this.sphericalExpansion;
            box.max.y += this.sphericalExpansion;
            box.max.z += this.sphericalExpansion;
            box.Transform(ref orientation);
        }

        public override void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
        {
            TSBBox largeBox = TSBBox.LargeBox;
            List<int> triangles = new List<int>();
            this.octree.GetTrianglesIntersectingtAABox(triangles, ref largeBox);
            for (int i = 0; i < triangles.Count; i++)
            {
                triangleList.Add(this.octree.GetVertex(this.octree.GetTriangleVertexIndex(i).I0));
                triangleList.Add(this.octree.GetVertex(this.octree.GetTriangleVertexIndex(i).I1));
                triangleList.Add(this.octree.GetVertex(this.octree.GetTriangleVertexIndex(i).I2));
            }
        }

        public override int Prepare(ref TSBBox box)
        {
            this.potentialTriangles.Clear();
            TSBBox testBox = box;
            testBox.min.x -= this.sphericalExpansion;
            testBox.min.y -= this.sphericalExpansion;
            testBox.min.z -= this.sphericalExpansion;
            testBox.max.x += this.sphericalExpansion;
            testBox.max.y += this.sphericalExpansion;
            testBox.max.z += this.sphericalExpansion;
            this.octree.GetTrianglesIntersectingtAABox(this.potentialTriangles, ref testBox);
            return this.potentialTriangles.Count;
        }

        public override int Prepare(ref TSVector rayOrigin, ref TSVector rayDelta)
        {
            TSVector vector;
            this.potentialTriangles.Clear();
            TSVector.Normalize(ref rayDelta, out vector);
            vector = rayDelta + (vector * this.sphericalExpansion);
            this.octree.GetTrianglesIntersectingRay(this.potentialTriangles, rayOrigin, vector);
            return this.potentialTriangles.Count;
        }

        public override void SetCurrentShape(int index)
        {
            this.vecs[0] = this.octree.GetVertex(this.octree.tris[this.potentialTriangles[index]].I0);
            this.vecs[1] = this.octree.GetVertex(this.octree.tris[this.potentialTriangles[index]].I1);
            this.vecs[2] = this.octree.GetVertex(this.octree.tris[this.potentialTriangles[index]].I2);
            TSVector vector = this.vecs[0];
            TSVector.Add(ref vector, ref this.vecs[1], out vector);
            TSVector.Add(ref vector, ref this.vecs[2], out vector);
            TSVector.Multiply(ref vector, FP.One / (3 * FP.One), out vector);
            base.geomCen = vector;
            TSVector.Subtract(ref this.vecs[1], ref this.vecs[0], out vector);
            TSVector.Subtract(ref this.vecs[2], ref this.vecs[0], out this.normal);
            TSVector.Cross(ref vector, ref this.normal, out this.normal);
            if (this.flipNormal)
            {
                this.normal.Negate();
            }
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            TSVector vector;
            TSVector.Normalize(ref direction, out vector);
            vector *= this.sphericalExpansion;
            FP fp = TSVector.Dot(ref this.vecs[0], ref direction);
            int index = 0;
            FP fp2 = TSVector.Dot(ref this.vecs[1], ref direction);
            if (fp2 > fp)
            {
                fp = fp2;
                index = 1;
            }
            fp2 = TSVector.Dot(ref this.vecs[2], ref direction);
            if (fp2 > fp)
            {
                fp = fp2;
                index = 2;
            }
            result = this.vecs[index] + vector;
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
    }
}

