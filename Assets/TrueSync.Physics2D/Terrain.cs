using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public class Terrain
	{
		public World World;

		public TSVector2 Center;

		public FP Width;

		public FP Height;

		public int PointsPerUnit;

		public int CellSize;

		public int SubCellSize;

		public int Iterations = 2;

		public TriangulationAlgorithm Decomposer;

		private sbyte[,] _terrainMap;

		private List<Body>[,] _bodyMap;

		private FP _localWidth;

		private FP _localHeight;

		private int _xnum;

		private int _ynum;

		private AABB _dirtyArea;

		private TSVector2 _topLeft;

		public Terrain(World world, AABB area)
		{
			this.World = world;
			this.Width = area.Width;
			this.Height = area.Height;
			this.Center = area.Center;
		}

		public Terrain(World world, TSVector2 position, FP width, FP height)
		{
			this.World = world;
			this.Width = width;
			this.Height = height;
			this.Center = position;
		}

		public void Initialize()
		{
			this._topLeft = new TSVector2(this.Center.x - this.Width * 0.5f, this.Center.y - -this.Height * 0.5f);
			this._localWidth = this.Width * this.PointsPerUnit;
			this._localHeight = this.Height * this.PointsPerUnit;
			this._terrainMap = new sbyte[(int)((long)this._localWidth) + 1, (int)((long)this._localHeight) + 1];
			int num = 0;
			while (num < this._localWidth)
			{
				int num2 = 0;
				while (num2 < this._localHeight)
				{
					this._terrainMap[num, num2] = 1;
					num2++;
				}
				num++;
			}
			this._xnum = (int)((long)(this._localWidth / this.CellSize));
			this._ynum = (int)((long)(this._localHeight / this.CellSize));
			this._bodyMap = new List<Body>[this._xnum, this._ynum];
			this._dirtyArea = new AABB(new TSVector2(FP.MaxValue, FP.MaxValue), new TSVector2(FP.MinValue, FP.MinValue));
		}

		public void ApplyData(sbyte[,] data, TSVector2 offset = default(TSVector2))
		{
			for (int i = 0; i < data.GetUpperBound(0); i++)
			{
				for (int j = 0; j < data.GetUpperBound(1); j++)
				{
					bool flag = i + offset.x >= 0 && i + offset.x < this._localWidth && j + offset.y >= 0 && j + offset.y < this._localHeight;
					if (flag)
					{
						this._terrainMap[(int)((long)(i + offset.x)), (int)((long)(j + offset.y))] = data[i, j];
					}
				}
			}
			this.RemoveOldData(0, this._xnum, 0, this._ynum);
		}

		public void ModifyTerrain(TSVector2 location, sbyte value)
		{
			TSVector2 tSVector = location - this._topLeft;
			tSVector.x = tSVector.x * this._localWidth / this.Width;
			tSVector.y = tSVector.y * -this._localHeight / this.Height;
			bool flag = tSVector.x >= 0 && tSVector.x < this._localWidth && tSVector.y >= 0 && tSVector.y < this._localHeight;
			if (flag)
			{
				this._terrainMap[(int)((long)tSVector.x), (int)((long)tSVector.y)] = value;
				bool flag2 = tSVector.x < this._dirtyArea.LowerBound.x;
				if (flag2)
				{
					this._dirtyArea.LowerBound.x = tSVector.x;
				}
				bool flag3 = tSVector.x > this._dirtyArea.UpperBound.x;
				if (flag3)
				{
					this._dirtyArea.UpperBound.x = tSVector.x;
				}
				bool flag4 = tSVector.y < this._dirtyArea.LowerBound.y;
				if (flag4)
				{
					this._dirtyArea.LowerBound.y = tSVector.y;
				}
				bool flag5 = tSVector.y > this._dirtyArea.UpperBound.y;
				if (flag5)
				{
					this._dirtyArea.UpperBound.y = tSVector.y;
				}
			}
		}

		public void RegenerateTerrain()
		{
			int num = (int)((long)(this._dirtyArea.LowerBound.x / this.CellSize));
			bool flag = num < 0;
			if (flag)
			{
				num = 0;
			}
			int num2 = (int)((long)(this._dirtyArea.UpperBound.x / this.CellSize)) + 1;
			bool flag2 = num2 > this._xnum;
			if (flag2)
			{
				num2 = this._xnum;
			}
			int num3 = (int)((long)(this._dirtyArea.LowerBound.y / this.CellSize));
			bool flag3 = num3 < 0;
			if (flag3)
			{
				num3 = 0;
			}
			int num4 = (int)((long)(this._dirtyArea.UpperBound.y / this.CellSize)) + 1;
			bool flag4 = num4 > this._ynum;
			if (flag4)
			{
				num4 = this._ynum;
			}
			this.RemoveOldData(num, num2, num3, num4);
			this._dirtyArea = new AABB(new TSVector2(FP.MaxValue, FP.MaxValue), new TSVector2(FP.MinValue, FP.MinValue));
		}

		private void RemoveOldData(int xStart, int xEnd, int yStart, int yEnd)
		{
			for (int i = xStart; i < xEnd; i++)
			{
				for (int j = yStart; j < yEnd; j++)
				{
					bool flag = this._bodyMap[i, j] != null;
					if (flag)
					{
						for (int k = 0; k < this._bodyMap[i, j].Count; k++)
						{
							this.World.RemoveBody(this._bodyMap[i, j][k]);
						}
					}
					this._bodyMap[i, j] = null;
					this.GenerateTerrain(i, j);
				}
			}
		}

		private void GenerateTerrain(int gx, int gy)
		{
			FP x = gx * this.CellSize;
			FP fP = gy * this.CellSize;
			List<Vertices> list = MarchingSquares.DetectSquares(new AABB(new TSVector2(x, fP), new TSVector2(x + this.CellSize, fP + this.CellSize)), this.SubCellSize, this.SubCellSize, this._terrainMap, this.Iterations, true);
			bool flag = list.Count == 0;
			if (!flag)
			{
				this._bodyMap[gx, gy] = new List<Body>();
				TSVector2 tSVector = new TSVector2(1f / (float)this.PointsPerUnit, 1f / (float)(-(float)this.PointsPerUnit));
				foreach (Vertices current in list)
				{
					current.Scale(ref tSVector);
					current.Translate(ref this._topLeft);
					Vertices vertices = SimplifyTools.CollinearSimplify(current, FP.Zero);
					List<Vertices> list2 = Triangulate.ConvexPartition(vertices, this.Decomposer, true, FP.EN3);
					foreach (Vertices current2 in list2)
					{
						bool flag2 = current2.Count > 2;
						if (flag2)
						{
							this._bodyMap[gx, gy].Add(BodyFactory.CreatePolygon(this.World, current2, 1, null));
						}
					}
				}
			}
		}
	}
}
