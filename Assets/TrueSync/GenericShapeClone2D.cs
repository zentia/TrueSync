using System;
using TrueSync.Physics2D;

namespace TrueSync
{
	public class GenericShapeClone2D
	{
		private FP _density;

		private FP _radius;

		private FP _2radius;

		private MassData massData;

		private TSVector2 _position;

		private Vertices _vertices;

		private Vertices _normals;

		public void Clone(TrueSync.Physics2D.Shape sh)
		{
			this._density = sh._density;
			this._radius = sh._radius;
			this._2radius = sh._2radius;
			this.massData = sh.MassData;
			bool flag = sh is PolygonShape;
			if (flag)
			{
				this.ClonePolygon((PolygonShape)sh);
			}
			else
			{
				bool flag2 = sh is CircleShape;
				if (flag2)
				{
					this.CloneCircle((CircleShape)sh);
				}
			}
		}

		private void ClonePolygon(PolygonShape sh)
		{
			bool flag = this._vertices == null;
			if (flag)
			{
				this._vertices = new Vertices();
			}
			else
			{
				this._vertices.Clear();
			}
			bool flag2 = this._normals == null;
			if (flag2)
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

		private void CloneCircle(CircleShape sh)
		{
			this._position = sh._position;
		}

		public void Restore(TrueSync.Physics2D.Shape sh)
		{
			sh._density = this._density;
			sh._radius = this._radius;
			sh._2radius = this._2radius;
			sh.MassData = this.massData;
			bool flag = sh is PolygonShape;
			if (flag)
			{
				this.RestorePolygon((PolygonShape)sh);
			}
			else
			{
				bool flag2 = sh is CircleShape;
				if (flag2)
				{
					this.RestoreCircle((CircleShape)sh);
				}
			}
		}

		private void RestorePolygon(PolygonShape sh)
		{
			sh._vertices.Clear();
			sh._vertices.AddRange(this._vertices);
			sh._normals.Clear();
			sh._normals.AddRange(this._normals);
		}

		private void RestoreCircle(CircleShape sh)
		{
			sh._position = this._position;
		}
	}
}
