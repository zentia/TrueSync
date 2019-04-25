namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class Terrain
    {
        private List<Body>[,] _bodyMap;
        private AABB _dirtyArea;
        private FP _localHeight;
        private FP _localWidth;
        private sbyte[,] _terrainMap;
        private TSVector2 _topLeft;
        private int _xnum;
        private int _ynum;
        public int CellSize;
        public TSVector2 Center;
        public TriangulationAlgorithm Decomposer;
        public FP Height;
        public int Iterations;
        public int PointsPerUnit;
        public int SubCellSize;
        public FP Width;
        public TrueSync.Physics2D.World World;

        public Terrain(TrueSync.Physics2D.World world, AABB area)
        {
            this.Iterations = 2;
            this.World = world;
            this.Width = area.Width;
            this.Height = area.Height;
            this.Center = area.Center;
        }

        public Terrain(TrueSync.Physics2D.World world, TSVector2 position, FP width, FP height)
        {
            this.Iterations = 2;
            this.World = world;
            this.Width = width;
            this.Height = height;
            this.Center = position;
        }

        public void ApplyData(sbyte[,] data, TSVector2 offset = new TSVector2())
        {
            for (int i = 0; i < data.GetUpperBound(0); i++)
            {
                for (int j = 0; j < data.GetUpperBound(1); j++)
                {
                    if (((((i + offset.x) >= 0) && ((i + offset.x) < this._localWidth)) && ((j + offset.y) >= 0)) && ((j + offset.y) < this._localHeight))
                    {
                        this._terrainMap[i + offset.x, j + offset.y] = data[i, j];
                    }
                }
            }
            this.RemoveOldData(0, this._xnum, 0, this._ynum);
        }

        private void GenerateTerrain(int gx, int gy)
        {
            FP x = gx * this.CellSize;
            FP y = gy * this.CellSize;
            List<Vertices> list = MarchingSquares.DetectSquares(new AABB(new TSVector2(x, y), new TSVector2(x + this.CellSize, y + this.CellSize)), this.SubCellSize, this.SubCellSize, this._terrainMap, this.Iterations, true);
            if (list.Count != 0)
            {
                this._bodyMap[gx, gy] = new List<Body>();
                TSVector2 vector = new TSVector2(1f / ((float) this.PointsPerUnit), 1f / ((float) -this.PointsPerUnit));
                foreach (Vertices vertices in list)
                {
                    vertices.Scale(ref vector);
                    vertices.Translate(ref this._topLeft);
                    List<Vertices> list2 = Triangulate.ConvexPartition(SimplifyTools.CollinearSimplify(vertices, FP.Zero), this.Decomposer, true, FP.EN3);
                    foreach (Vertices vertices3 in list2)
                    {
                        if (vertices3.Count > 2)
                        {
                            this._bodyMap[gx, gy].Add(BodyFactory.CreatePolygon(this.World, vertices3, 1, null));
                        }
                    }
                }
            }
        }

        public void Initialize()
        {
            this._topLeft = new TSVector2(this.Center.x - (this.Width * 0.5f), this.Center.y - (-this.Height * 0.5f));
            this._localWidth = this.Width * this.PointsPerUnit;
            this._localHeight = this.Height * this.PointsPerUnit;
            this._terrainMap = new sbyte[((int) ((long) this._localWidth)) + 1, ((int) ((long) this._localHeight)) + 1];
            for (int i = 0; i < this._localWidth; i++)
            {
                for (int j = 0; j < this._localHeight; j++)
                {
                    this._terrainMap[i, j] = 1;
                }
            }
            this._xnum = (int) ((long) (this._localWidth / this.CellSize));
            this._ynum = (int) ((long) (this._localHeight / this.CellSize));
            this._bodyMap = new List<Body>[this._xnum, this._ynum];
            this._dirtyArea = new AABB(new TSVector2(FP.MaxValue, FP.MaxValue), new TSVector2(FP.MinValue, FP.MinValue));
        }

        public void ModifyTerrain(TSVector2 location, sbyte value)
        {
            TSVector2 vector = location - this._topLeft;
            vector.x = (vector.x * this._localWidth) / this.Width;
            vector.y = (vector.y * -this._localHeight) / this.Height;
            if ((((vector.x >= 0) && (vector.x < this._localWidth)) && (vector.y >= 0)) && (vector.y < this._localHeight))
            {
                this._terrainMap[(int) ((long) vector.x), (int) ((long) vector.y)] = value;
                if (vector.x < this._dirtyArea.LowerBound.x)
                {
                    this._dirtyArea.LowerBound.x = vector.x;
                }
                if (vector.x > this._dirtyArea.UpperBound.x)
                {
                    this._dirtyArea.UpperBound.x = vector.x;
                }
                if (vector.y < this._dirtyArea.LowerBound.y)
                {
                    this._dirtyArea.LowerBound.y = vector.y;
                }
                if (vector.y > this._dirtyArea.UpperBound.y)
                {
                    this._dirtyArea.UpperBound.y = vector.y;
                }
            }
        }

        public void RegenerateTerrain()
        {
            int xStart = (int) ((long) (this._dirtyArea.LowerBound.x / this.CellSize));
            if (xStart < 0)
            {
                xStart = 0;
            }
            int xEnd = ((int) ((long) (this._dirtyArea.UpperBound.x / this.CellSize))) + 1;
            if (xEnd > this._xnum)
            {
                xEnd = this._xnum;
            }
            int yStart = (int) ((long) (this._dirtyArea.LowerBound.y / this.CellSize));
            if (yStart < 0)
            {
                yStart = 0;
            }
            int yEnd = ((int) ((long) (this._dirtyArea.UpperBound.y / this.CellSize))) + 1;
            if (yEnd > this._ynum)
            {
                yEnd = this._ynum;
            }
            this.RemoveOldData(xStart, xEnd, yStart, yEnd);
            this._dirtyArea = new AABB(new TSVector2(FP.MaxValue, FP.MaxValue), new TSVector2(FP.MinValue, FP.MinValue));
        }

        private void RemoveOldData(int xStart, int xEnd, int yStart, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    if (this._bodyMap[i, j] > null)
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
    }
}

