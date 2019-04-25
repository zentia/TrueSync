namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class Octree
    {
        private Node[] nodes;
        private ArrayResourcePool<ushort> nodeStackPool;
        private TSVector[] positions;
        internal TSBBox rootNodeBox;
        private TSBBox[] triBoxes;
        internal TriangleVertexIndices[] tris;

        public Octree(List<TSVector> positions, List<TriangleVertexIndices> tris)
        {
            this.SetTriangles(positions, tris);
            this.BuildOctree();
        }

        public void BuildOctree()
        {
            this.triBoxes = new TSBBox[this.tris.Length];
            this.rootNodeBox = new TSBBox(new TSVector(FP.PositiveInfinity, FP.PositiveInfinity, FP.PositiveInfinity), new TSVector(FP.NegativeInfinity, FP.NegativeInfinity, FP.NegativeInfinity));
            for (int i = 0; i < this.tris.Length; i++)
            {
                TSVector.Min(ref this.positions[this.tris[i].I1], ref this.positions[this.tris[i].I2], out this.triBoxes[i].min);
                TSVector.Min(ref this.positions[this.tris[i].I0], ref this.triBoxes[i].min, out this.triBoxes[i].min);
                TSVector.Max(ref this.positions[this.tris[i].I1], ref this.positions[this.tris[i].I2], out this.triBoxes[i].max);
                TSVector.Max(ref this.positions[this.tris[i].I0], ref this.triBoxes[i].max, out this.triBoxes[i].max);
                TSVector.Min(ref this.rootNodeBox.min, ref this.triBoxes[i].min, out this.rootNodeBox.min);
                TSVector.Max(ref this.rootNodeBox.max, ref this.triBoxes[i].max, out this.rootNodeBox.max);
            }
            List<BuildNode> list = new List<BuildNode> {
                new BuildNode()
            };
            list[0].box = this.rootNodeBox;
            TSBBox[] boxArray = new TSBBox[8];
            for (int j = 0; j < this.tris.Length; j++)
            {
                int item = 0;
                TSBBox rootNodeBox = this.rootNodeBox;
                while (rootNodeBox.Contains(ref this.triBoxes[j]) == TSBBox.ContainmentType.Contains)
                {
                    int index = -1;
                    for (int m = 0; m < 8; m++)
                    {
                        this.CreateAABox(ref rootNodeBox, (EChild) m, out boxArray[m]);
                        if (boxArray[m].Contains(ref this.triBoxes[j]) == TSBBox.ContainmentType.Contains)
                        {
                            index = m;
                            break;
                        }
                    }
                    if (index == -1)
                    {
                        list[item].triIndices.Add(j);
                        break;
                    }
                    int num6 = -1;
                    for (int n = 0; n < list[item].nodeIndices.Count; n++)
                    {
                        if (list[list[item].nodeIndices[n]].childType == index)
                        {
                            num6 = n;
                            break;
                        }
                    }
                    if (num6 == -1)
                    {
                        BuildNode node = list[item];
                        BuildNode node2 = new BuildNode {
                            childType = index,
                            box = boxArray[index]
                        };
                        list.Add(node2);
                        item = list.Count - 1;
                        rootNodeBox = boxArray[index];
                        node.nodeIndices.Add(item);
                    }
                    else
                    {
                        item = list[item].nodeIndices[num6];
                        rootNodeBox = boxArray[index];
                    }
                }
            }
            this.nodes = new Node[list.Count];
            this.nodeStackPool = new ArrayResourcePool<ushort>(list.Count);
            for (int k = 0; k < this.nodes.Length; k++)
            {
                this.nodes[k].nodeIndices = new ushort[list[k].nodeIndices.Count];
                for (int num9 = 0; num9 < this.nodes[k].nodeIndices.Length; num9++)
                {
                    this.nodes[k].nodeIndices[num9] = (ushort) list[k].nodeIndices[num9];
                }
                this.nodes[k].triIndices = new int[list[k].triIndices.Count];
                list[k].triIndices.CopyTo(this.nodes[k].triIndices);
                this.nodes[k].box = list[k].box;
            }
            list.Clear();
            list = null;
        }

        public void Clear()
        {
            this.positions = null;
            this.triBoxes = null;
            this.tris = null;
            this.nodes = null;
            this.nodeStackPool.ResetResourcePool();
        }

        private void CreateAABox(ref TSBBox aabb, EChild child, out TSBBox result)
        {
            TSVector vector;
            TSVector vector3;
            TSVector.Subtract(ref aabb.max, ref aabb.min, out vector);
            TSVector.Multiply(ref vector, FP.Half, out vector);
            TSVector zero = TSVector.zero;
            switch (child)
            {
                case EChild.MMM:
                    zero = new TSVector(0, 0, 0);
                    break;

                case EChild.PMM:
                    zero = new TSVector(1, 0, 0);
                    break;

                case EChild.MPM:
                    zero = new TSVector(0, 1, 0);
                    break;

                case EChild.PPM:
                    zero = new TSVector(1, 1, 0);
                    break;

                case EChild.MMP:
                    zero = new TSVector(0, 0, 1);
                    break;

                case EChild.PMP:
                    zero = new TSVector(1, 0, 1);
                    break;

                case EChild.MPP:
                    zero = new TSVector(0, 1, 1);
                    break;

                case EChild.PPP:
                    zero = new TSVector(1, 1, 1);
                    break;

                default:
                    Debug.WriteLine("Octree.CreateAABox  got impossible child");
                    break;
            }
            result = new TSBBox();
            result.min = new TSVector(zero.x * vector.x, zero.y * vector.y, zero.z * vector.z);
            TSVector.Add(ref result.min, ref aabb.min, out result.min);
            TSVector.Add(ref result.min, ref vector, out result.max);
            FP scaleFactor = FP.EN5;
            TSVector.Multiply(ref vector, scaleFactor, out vector3);
            TSVector.Subtract(ref result.min, ref vector3, out result.min);
            TSVector.Add(ref result.max, ref vector3, out result.max);
        }

        private void GatherTriangles(int nodeIndex, ref List<int> tris)
        {
            tris.AddRange(this.nodes[nodeIndex].triIndices);
            int length = this.nodes[nodeIndex].nodeIndices.Length;
            for (int i = 0; i < length; i++)
            {
                int num3 = this.nodes[nodeIndex].nodeIndices[i];
                this.GatherTriangles(num3, ref tris);
            }
        }

        public int GetTrianglesIntersectingRay(List<int> triangles, TSVector rayOrigin, TSVector rayDelta)
        {
            if (this.nodes.Length == 0)
            {
                return 0;
            }
            int index = 0;
            int num2 = 1;
            ushort[] numArray = this.nodeStackPool.GetNew();
            numArray[0] = 0;
            int num3 = 0;
            while (index < num2)
            {
                ushort num5 = numArray[index];
                index++;
                if (this.nodes[num5].box.SegmentIntersect(ref rayOrigin, ref rayDelta))
                {
                    for (int i = 0; i < this.nodes[num5].triIndices.Length; i++)
                    {
                        if (this.triBoxes[this.nodes[num5].triIndices[i]].SegmentIntersect(ref rayOrigin, ref rayDelta))
                        {
                            triangles.Add(this.nodes[num5].triIndices[i]);
                            num3++;
                        }
                    }
                    int length = this.nodes[num5].nodeIndices.Length;
                    for (int j = 0; j < length; j++)
                    {
                        numArray[num2++] = this.nodes[num5].nodeIndices[j];
                    }
                }
            }
            this.nodeStackPool.GiveBack(numArray);
            return num3;
        }

        public int GetTrianglesIntersectingtAABox(List<int> triangles, ref TSBBox testBox)
        {
            if (this.nodes.Length == 0)
            {
                return 0;
            }
            int index = 0;
            int num2 = 1;
            ushort[] numArray = this.nodeStackPool.GetNew();
            numArray[0] = 0;
            int num3 = 0;
            while (index < num2)
            {
                ushort num5 = numArray[index];
                index++;
                if (this.nodes[num5].box.Contains(ref testBox) > TSBBox.ContainmentType.Disjoint)
                {
                    for (int i = 0; i < this.nodes[num5].triIndices.Length; i++)
                    {
                        if (this.triBoxes[this.nodes[num5].triIndices[i]].Contains(ref testBox) > TSBBox.ContainmentType.Disjoint)
                        {
                            triangles.Add(this.nodes[num5].triIndices[i]);
                            num3++;
                        }
                    }
                    int length = this.nodes[num5].nodeIndices.Length;
                    for (int j = 0; j < length; j++)
                    {
                        numArray[num2++] = this.nodes[num5].nodeIndices[j];
                    }
                }
            }
            this.nodeStackPool.GiveBack(numArray);
            return num3;
        }

        public TriangleVertexIndices GetTriangleVertexIndex(int index)
        {
            return this.tris[index];
        }

        public TSVector GetVertex(int vertex)
        {
            return this.positions[vertex];
        }

        public void GetVertex(int vertex, out TSVector result)
        {
            result = this.positions[vertex];
        }

        public void SetTriangles(List<TSVector> positions, List<TriangleVertexIndices> tris)
        {
            this.positions = new TSVector[positions.Count];
            positions.CopyTo(this.positions);
            this.tris = new TriangleVertexIndices[tris.Count];
            tris.CopyTo(this.tris);
        }

        public int NumTriangles
        {
            get
            {
                return this.tris.Length;
            }
        }

        public TSBBox RootNodeBox
        {
            get
            {
                return this.rootNodeBox;
            }
        }

        private class BuildNode
        {
            public TSBBox box;
            public int childType;
            public List<int> nodeIndices = new List<int>();
            public List<int> triIndices = new List<int>();
        }

        [Flags]
        private enum EChild
        {
            MMM = 0,
            MMP = 4,
            MPM = 2,
            MPP = 6,
            PMM = 1,
            PMP = 5,
            PPM = 3,
            PPP = 7,
            XP = 1,
            YP = 2,
            ZP = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Node
        {
            public ushort[] nodeIndices;
            public int[] triIndices;
            public TSBBox box;
        }
    }
}

