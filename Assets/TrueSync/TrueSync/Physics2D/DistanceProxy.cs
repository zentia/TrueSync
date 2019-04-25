namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using TrueSync;

    public class DistanceProxy
    {
        internal FP Radius;
        internal TrueSync.Physics2D.Vertices Vertices = new TrueSync.Physics2D.Vertices();

        public int GetSupport(TSVector2 direction)
        {
            int num = 0;
            FP fp = TSVector2.Dot(this.Vertices[0], direction);
            for (int i = 1; i < this.Vertices.Count; i++)
            {
                FP fp2 = TSVector2.Dot(this.Vertices[i], direction);
                if (fp2 > fp)
                {
                    num = i;
                    fp = fp2;
                }
            }
            return num;
        }

        public TSVector2 GetSupportVertex(TSVector2 direction)
        {
            int num = 0;
            FP fp = TSVector2.Dot(this.Vertices[0], direction);
            for (int i = 1; i < this.Vertices.Count; i++)
            {
                FP fp2 = TSVector2.Dot(this.Vertices[i], direction);
                if (fp2 > fp)
                {
                    num = i;
                    fp = fp2;
                }
            }
            return this.Vertices[num];
        }

        public void Set(TrueSync.Physics2D.Shape shape, int index)
        {
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                {
                    CircleShape shape2 = (CircleShape) shape;
                    this.Vertices.Clear();
                    this.Vertices.Add(shape2.Position);
                    this.Radius = shape2.Radius;
                    break;
                }
                case ShapeType.Edge:
                {
                    EdgeShape shape5 = (EdgeShape) shape;
                    this.Vertices.Clear();
                    this.Vertices.Add(shape5.Vertex1);
                    this.Vertices.Add(shape5.Vertex2);
                    this.Radius = shape5.Radius;
                    break;
                }
                case ShapeType.Polygon:
                {
                    PolygonShape shape3 = (PolygonShape) shape;
                    this.Vertices.Clear();
                    for (int i = 0; i < shape3.Vertices.Count; i++)
                    {
                        this.Vertices.Add(shape3.Vertices[i]);
                    }
                    this.Radius = shape3.Radius;
                    break;
                }
                case ShapeType.Chain:
                {
                    ChainShape shape4 = (ChainShape) shape;
                    Debug.Assert((0 <= index) && (index < shape4.Vertices.Count));
                    this.Vertices.Clear();
                    this.Vertices.Add(shape4.Vertices[index]);
                    this.Vertices.Add(((index + 1) < shape4.Vertices.Count) ? shape4.Vertices[index + 1] : shape4.Vertices[0]);
                    this.Radius = shape4.Radius;
                    break;
                }
                default:
                    Debug.Assert(false);
                    break;
            }
        }
    }
}

