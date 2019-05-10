using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync
{
	public class Octree
	{
		[Flags]
		private enum EChild
		{
			XP = 1,
			YP = 2,
			ZP = 4,
			PPP = 7,
			PPM = 3,
			PMP = 5,
			PMM = 1,
			MPP = 6,
			MPM = 2,
			MMP = 4,
			MMM = 0
		}

		private struct Node
		{
			public ushort[] nodeIndices;

			public int[] triIndices;

			public TSBBox box;
		}

		private class BuildNode
		{
			public int childType;

			public List<int> nodeIndices = new List<int>();

			public List<int> triIndices = new List<int>();

			public TSBBox box;
		}

		private TSVector[] positions;

		private TSBBox[] triBoxes;

		private Octree.Node[] nodes;

		internal TriangleVertexIndices[] tris;

		internal TSBBox rootNodeBox;

		private ArrayResourcePool<ushort> nodeStackPool;

		public TSBBox RootNodeBox
		{
			get
			{
				return this.rootNodeBox;
			}
		}

		public int NumTriangles
		{
			get
			{
				return this.tris.Length;
			}
		}

		public void Clear()
		{
			this.positions = null;
			this.triBoxes = null;
			this.tris = null;
			this.nodes = null;
			this.nodeStackPool.ResetResourcePool();
		}

		public void SetTriangles(List<TSVector> positions, List<TriangleVertexIndices> tris)
		{
			this.positions = new TSVector[positions.Count];
			positions.CopyTo(this.positions);
			this.tris = new TriangleVertexIndices[tris.Count];
			tris.CopyTo(this.tris);
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
			List<Octree.BuildNode> list = new List<Octree.BuildNode>();
			list.Add(new Octree.BuildNode());
			list[0].box = this.rootNodeBox;
			TSBBox[] array = new TSBBox[8];
			for (int j = 0; j < this.tris.Length; j++)
			{
				int num = 0;
				TSBBox tSBBox = this.rootNodeBox;
				while (tSBBox.Contains(ref this.triBoxes[j]) == TSBBox.ContainmentType.Contains)
				{
					int num2 = -1;
					for (int k = 0; k < 8; k++)
					{
						this.CreateAABox(ref tSBBox, (Octree.EChild)k, out array[k]);
						bool flag = array[k].Contains(ref this.triBoxes[j]) == TSBBox.ContainmentType.Contains;
						if (flag)
						{
							num2 = k;
							break;
						}
					}
					bool flag2 = num2 == -1;
					if (flag2)
					{
						list[num].triIndices.Add(j);
						break;
					}
					int num3 = -1;
					for (int l = 0; l < list[num].nodeIndices.Count; l++)
					{
						bool flag3 = list[list[num].nodeIndices[l]].childType == num2;
						if (flag3)
						{
							num3 = l;
							break;
						}
					}
					bool flag4 = num3 == -1;
					if (flag4)
					{
						Octree.BuildNode buildNode = list[num];
						list.Add(new Octree.BuildNode
						{
							childType = num2,
							box = array[num2]
						});
						num = list.Count - 1;
						tSBBox = array[num2];
						buildNode.nodeIndices.Add(num);
					}
					else
					{
						num = list[num].nodeIndices[num3];
						tSBBox = array[num2];
					}
				}
			}
			this.nodes = new Octree.Node[list.Count];
			this.nodeStackPool = new ArrayResourcePool<ushort>(list.Count);
			for (int m = 0; m < this.nodes.Length; m++)
			{
				this.nodes[m].nodeIndices = new ushort[list[m].nodeIndices.Count];
				for (int n = 0; n < this.nodes[m].nodeIndices.Length; n++)
				{
					this.nodes[m].nodeIndices[n] = (ushort)list[m].nodeIndices[n];
				}
				this.nodes[m].triIndices = new int[list[m].triIndices.Count];
				list[m].triIndices.CopyTo(this.nodes[m].triIndices);
				this.nodes[m].box = list[m].box;
			}
			list.Clear();
		}

		public Octree(List<TSVector> positions, List<TriangleVertexIndices> tris)
		{
			this.SetTriangles(positions, tris);
			this.BuildOctree();
		}

		private void CreateAABox(ref TSBBox aabb, Octree.EChild child, out TSBBox result)
		{
			TSVector tSVector;
			TSVector.Subtract(ref aabb.max, ref aabb.min, out tSVector);
			TSVector.Multiply(ref tSVector, FP.Half, out tSVector);
			TSVector zero = TSVector.zero;
			switch (child)
			{
			case Octree.EChild.MMM:
				zero = new TSVector(0, 0, 0);
				break;
			case Octree.EChild.XP:
				zero = new TSVector(1, 0, 0);
				break;
			case Octree.EChild.YP:
				zero = new TSVector(0, 1, 0);
				break;
			case Octree.EChild.PPM:
				zero = new TSVector(1, 1, 0);
				break;
			case Octree.EChild.ZP:
				zero = new TSVector(0, 0, 1);
				break;
			case Octree.EChild.PMP:
				zero = new TSVector(1, 0, 1);
				break;
			case Octree.EChild.MPP:
				zero = new TSVector(0, 1, 1);
				break;
			case Octree.EChild.PPP:
				zero = new TSVector(1, 1, 1);
				break;
			default:
				Debug.WriteLine("Octree.CreateAABox  got impossible child");
				break;
			}
			result = default(TSBBox);
			result.min = new TSVector(zero.x * tSVector.x, zero.y * tSVector.y, zero.z * tSVector.z);
			TSVector.Add(ref result.min, ref aabb.min, out result.min);
			TSVector.Add(ref result.min, ref tSVector, out result.max);
			FP eN = FP.EN5;
			TSVector tSVector2;
			TSVector.Multiply(ref tSVector, eN, out tSVector2);
			TSVector.Subtract(ref result.min, ref tSVector2, out result.min);
			TSVector.Add(ref result.max, ref tSVector2, out result.max);
		}

		private void GatherTriangles(int nodeIndex, ref List<int> tris)
		{
			tris.AddRange(this.nodes[nodeIndex].triIndices);
			int num = this.nodes[nodeIndex].nodeIndices.Length;
			for (int i = 0; i < num; i++)
			{
				int nodeIndex2 = (int)this.nodes[nodeIndex].nodeIndices[i];
				this.GatherTriangles(nodeIndex2, ref tris);
			}
		}

		public int GetTrianglesIntersectingtAABox(List<int> triangles, ref TSBBox testBox)
		{
			bool flag = this.nodes.Length == 0;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				int i = 0;
				int num = 1;
				ushort[] @new = this.nodeStackPool.GetNew();
				@new[0] = 0;
				int num2 = 0;
				while (i < num)
				{
					ushort num3 = @new[i];
					i++;
					bool flag2 = this.nodes[(int)num3].box.Contains(ref testBox) > TSBBox.ContainmentType.Disjoint;
					if (flag2)
					{
						for (int j = 0; j < this.nodes[(int)num3].triIndices.Length; j++)
						{
							bool flag3 = this.triBoxes[this.nodes[(int)num3].triIndices[j]].Contains(ref testBox) > TSBBox.ContainmentType.Disjoint;
							if (flag3)
							{
								triangles.Add(this.nodes[(int)num3].triIndices[j]);
								num2++;
							}
						}
						int num4 = this.nodes[(int)num3].nodeIndices.Length;
						for (int k = 0; k < num4; k++)
						{
							@new[num++] = this.nodes[(int)num3].nodeIndices[k];
						}
					}
				}
				this.nodeStackPool.GiveBack(@new);
				result = num2;
			}
			return result;
		}

		public int GetTrianglesIntersectingRay(List<int> triangles, TSVector rayOrigin, TSVector rayDelta)
		{
			bool flag = this.nodes.Length == 0;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				int i = 0;
				int num = 1;
				ushort[] @new = this.nodeStackPool.GetNew();
				@new[0] = 0;
				int num2 = 0;
				while (i < num)
				{
					ushort num3 = @new[i];
					i++;
					bool flag2 = this.nodes[(int)num3].box.SegmentIntersect(ref rayOrigin, ref rayDelta);
					if (flag2)
					{
						for (int j = 0; j < this.nodes[(int)num3].triIndices.Length; j++)
						{
							bool flag3 = this.triBoxes[this.nodes[(int)num3].triIndices[j]].SegmentIntersect(ref rayOrigin, ref rayDelta);
							if (flag3)
							{
								triangles.Add(this.nodes[(int)num3].triIndices[j]);
								num2++;
							}
						}
						int num4 = this.nodes[(int)num3].nodeIndices.Length;
						for (int k = 0; k < num4; k++)
						{
							@new[num++] = this.nodes[(int)num3].nodeIndices[k];
						}
					}
				}
				this.nodeStackPool.GiveBack(@new);
				result = num2;
			}
			return result;
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
	}
}
