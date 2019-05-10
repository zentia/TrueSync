using System;

namespace TrueSync
{
	public class GenericShapeClone
	{
		public TSMatrix inertia;

		public FP mass;

		public TSBBox boundingBox;

		public TSVector geomCen;

		public TSVector vector1;

		public TSVector vector2;

		public FP fp1;

		public FP fp2;

		public FP fp3;

		public void Clone(Shape sh)
		{
			this.inertia = sh.inertia;
			this.mass = sh.mass;
			this.boundingBox = sh.boundingBox;
			this.geomCen = sh.geomCen;
			bool flag = sh is BoxShape;
			if (flag)
			{
				this.CloneBox((BoxShape)sh);
			}
			else
			{
				bool flag2 = sh is SphereShape;
				if (flag2)
				{
					this.CloneSphere((SphereShape)sh);
				}
				else
				{
					bool flag3 = sh is ConeShape;
					if (flag3)
					{
						this.CloneCone((ConeShape)sh);
					}
					else
					{
						bool flag4 = sh is CylinderShape;
						if (flag4)
						{
							this.CloneCylinder((CylinderShape)sh);
						}
						else
						{
							bool flag5 = sh is CapsuleShape;
							if (flag5)
							{
								this.CloneCapsule((CapsuleShape)sh);
							}
						}
					}
				}
			}
		}

		private void CloneBox(BoxShape sh)
		{
			this.vector1 = sh.size;
			this.vector2 = sh.halfSize;
		}

		private void CloneSphere(SphereShape sh)
		{
			this.fp1 = sh.radius;
		}

		private void CloneCone(ConeShape sh)
		{
			this.fp1 = sh.radius;
			this.fp2 = sh.height;
			this.fp3 = sh.sina;
		}

		private void CloneCylinder(CylinderShape sh)
		{
			this.fp1 = sh.radius;
			this.fp2 = sh.height;
		}

		private void CloneCapsule(CapsuleShape sh)
		{
			this.fp1 = sh.radius;
			this.fp2 = sh.length;
		}

		public void Restore(Shape sh)
		{
			sh.inertia = this.inertia;
			sh.mass = this.mass;
			sh.boundingBox = this.boundingBox;
			sh.geomCen = this.geomCen;
			bool flag = sh is BoxShape;
			if (flag)
			{
				this.RestoreBox((BoxShape)sh);
			}
			else
			{
				bool flag2 = sh is SphereShape;
				if (flag2)
				{
					this.RestoreSphere((SphereShape)sh);
				}
				else
				{
					bool flag3 = sh is ConeShape;
					if (flag3)
					{
						this.RestoreCone((ConeShape)sh);
					}
					else
					{
						bool flag4 = sh is CylinderShape;
						if (flag4)
						{
							this.RestoreCylinder((CylinderShape)sh);
						}
						else
						{
							bool flag5 = sh is CapsuleShape;
							if (flag5)
							{
								this.RestoreCapsule((CapsuleShape)sh);
							}
						}
					}
				}
			}
		}

		private void RestoreBox(BoxShape sh)
		{
			sh.size = this.vector1;
			sh.halfSize = this.vector2;
		}

		private void RestoreSphere(SphereShape sh)
		{
			sh.radius = this.fp1;
		}

		private void RestoreCone(ConeShape sh)
		{
			sh.radius = this.fp1;
			sh.height = this.fp2;
			sh.sina = this.fp3;
		}

		private void RestoreCylinder(CylinderShape sh)
		{
			sh.radius = this.fp1;
			sh.height = this.fp2;
		}

		private void RestoreCapsule(CapsuleShape sh)
		{
			sh.radius = this.fp1;
			sh.length = this.fp2;
		}
	}
}
