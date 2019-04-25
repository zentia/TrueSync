namespace TrueSync
{
    public interface IBody3D : IBody
    {
        void TSApplyForce(TSVector force);
        void TSApplyForce(TSVector force, TSVector position);
        void TSApplyImpulse(TSVector force);
        void TSApplyImpulse(TSVector force, TSVector position);
        void TSApplyTorque(TSVector force);

        bool TSAffectedByGravity { get; set; }

        TSVector TSAngularVelocity { get; set; }

        bool TSIsKinematic { get; set; }

        TSVector TSLinearVelocity { get; set; }

        TSMatrix TSOrientation { get; set; }

        TSVector TSPosition { get; set; }
    }
}

