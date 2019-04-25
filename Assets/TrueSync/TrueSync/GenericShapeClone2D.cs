namespace TrueSync
{
    using System;
    using TrueSync.Physics2D;

    public class GenericShapeClone2D
    {
        private FP _2radius;
        private FP _density;
        private Vertices _normals;
        private TSVector2 _position;
        private FP _radius;
        private Vertices _vertices;
        private MassData massData;

        public void Clone(TrueSync.Physics2D.Shape sh)
        {
            this._density = sh._density;
            this._radius = sh._radius;
            this._2radius = sh._2radius;
            this.massData = sh.MassData;
            if (sh is PolygonShape)
            {
                this.ClonePolygon((PolygonShape) sh);
            }
            else if (sh is CircleShape)
            {
                this.CloneCircle((CircleShape) sh);
            }
        }

        private void CloneCircle(CircleShape sh)
        {
            this._position = sh._position;
        }

        private void ClonePolygon(PolygonShape sh)
        {
            if (this._vertices == null)
            {
                this._vertices = new Vertices();
            }
            else
            {
                this._vertices.Clear();
            }
            if (this._normals == null)
            {
                this._normals = new Vertices();
            }
            else
            {
                this._normals.Clear();
            }
            this._vertices.AddRange(sh._vertices);
            this._normals.AddRange(sh._normals);
        }

        public void Restore(TrueSync.Physics2D.Shape sh)
        {
            sh._density = this._density;
            sh._radius = this._radius;
            sh._2radius = this._2radius;
            sh.MassData = this.massData;
            if (sh is PolygonShape)
            {
                this.RestorePolygon((PolygonShape) sh);
            }
            else if (sh is CircleShape)
            {
                this.RestoreCircle((CircleShape) sh);
            }
        }

        private void RestoreCircle(CircleShape sh)
        {
            sh._position = this._position;
        }

        private void RestorePolygon(PolygonShape sh)
        {
            sh._vertices.Clear();
            sh._vertices.AddRange(this._vertices);
            sh._normals.Clear();
            sh._normals.AddRange(this._normals);
        }
    }
}

