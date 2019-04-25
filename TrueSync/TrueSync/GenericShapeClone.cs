namespace TrueSync
{
    using System;

    public class GenericShapeClone
    {
        public TSBBox boundingBox;
        public FP fp1;
        public FP fp2;
        public FP fp3;
        public TSVector geomCen;
        public TSMatrix inertia;
        public FP mass;
        public TSVector vector1;
        public TSVector vector2;

        public void Clone(Shape sh)
        {
            this.inertia = sh.inertia;
            this.mass = sh.mass;
            this.boundingBox = sh.boundingBox;
            this.geomCen = sh.geomCen;
            if (sh is BoxShape)
            {
                this.CloneBox((BoxShape) sh);
            }
            else if (sh is SphereShape)
            {
                this.CloneSphere((SphereShape) sh);
            }
            else if (sh is ConeShape)
            {
                this.CloneCone((ConeShape) sh);
            }
            else if (sh is CylinderShape)
            {
                this.CloneCylinder((CylinderShape) sh);
            }
            else if (sh is CapsuleShape)
            {
                this.CloneCapsule((CapsuleShape) sh);
            }
        }

        private void CloneBox(BoxShape sh)
        {
            this.vector1 = sh.size;
            this.vector2 = sh.halfSize;
        }

        private void CloneCapsule(CapsuleShape sh)
        {
            this.fp1 = sh.radius;
            this.fp2 = sh.length;
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

        private void CloneSphere(SphereShape sh)
        {
            this.fp1 = sh.radius;
        }

        public void Restore(Shape sh)
        {
            sh.inertia = this.inertia;
            sh.mass = this.mass;
            sh.boundingBox = this.boundingBox;
            sh.geomCen = this.geomCen;
            if (sh is BoxShape)
            {
                this.RestoreBox((BoxShape) sh);
            }
            else if (sh is SphereShape)
            {
                this.RestoreSphere((SphereShape) sh);
            }
            else if (sh is ConeShape)
            {
                this.RestoreCone((ConeShape) sh);
            }
            else if (sh is CylinderShape)
            {
                this.RestoreCylinder((CylinderShape) sh);
            }
            else if (sh is CapsuleShape)
            {
                this.RestoreCapsule((CapsuleShape) sh);
            }
        }

        private void RestoreBox(BoxShape sh)
        {
            sh.size = this.vector1;
            sh.halfSize = this.vector2;
        }

        private void RestoreCapsule(CapsuleShape sh)
        {
            sh.radius = this.fp1;
            sh.length = this.fp2;
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

        private void RestoreSphere(SphereShape sh)
        {
            sh.radius = this.fp1;
        }
    }
}

