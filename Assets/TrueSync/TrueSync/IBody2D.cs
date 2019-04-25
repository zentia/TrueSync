namespace TrueSync
{
    // 表示到2D实体的接口
    public interface IBody2D : IBody
    {
        //对物体的中心施加一个力。
        void TSApplyForce(TSVector2 force);
        //在物体的特定位置施加一个力。
        void TSApplyForce(TSVector2 force, TSVector2 position);
        //对身体的中心施加一个冲量。
        void TSApplyImpulse(TSVector2 force);
        //在身体的特定位置施加一个脉冲。
        void TSApplyImpulse(TSVector2 force, TSVector2 position);
        // 对物体施加扭矩。
        void TSApplyTorque(TSVector2 force);
        //如果是真的，身体会受到重力的影响。
        bool TSAffectedByGravity { get; set; }

        FP TSAngularDamping { get; set; }

        FP TSAngularVelocity { get; set; }
        //如果为真，则以运动学的形式管理身体
        bool TSIsKinematic { get; set; }
        //设置/得到物体的线性阻尼(0到1)。
        FP TSLinearDamping { get; set; }

        TSVector2 TSLinearVelocity { get; set; }
        //设置/获取身体的方向。
        FP TSOrientation { get; set; }

        TSVector2 TSPosition { get; set; }
    }
}

