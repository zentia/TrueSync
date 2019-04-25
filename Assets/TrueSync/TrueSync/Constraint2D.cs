namespace TrueSync
{
    using System;

    public class Constraint2D : Constraint
    {
        private bool freezeZAxis;

        public Constraint2D(RigidBody body, bool freezeZAxis) : base(body, null)
        {
            this.freezeZAxis = freezeZAxis;
        }

        public override void PostStep()
        {
            TSVector position = base.Body1.Position;
            position.z = 0;
            base.Body1.Position = position;
            TSQuaternion quaternion = TSQuaternion.CreateFromMatrix(base.Body1.Orientation);
            quaternion.Normalize();
            quaternion.x = 0;
            quaternion.y = 0;
            if (this.freezeZAxis)
            {
                quaternion.z = 0;
            }
            base.Body1.Orientation = TSMatrix.CreateFromQuaternion(quaternion);
            if (!base.Body1.isStatic)
            {
                TSVector linearVelocity = base.Body1.LinearVelocity;
                linearVelocity.z = 0;
                base.Body1.LinearVelocity = linearVelocity;
                TSVector angularVelocity = base.Body1.AngularVelocity;
                angularVelocity.x = 0;
                angularVelocity.y = 0;
                if (this.freezeZAxis)
                {
                    angularVelocity.z = 0;
                }
                base.Body1.AngularVelocity = angularVelocity;
            }
        }
    }
}

