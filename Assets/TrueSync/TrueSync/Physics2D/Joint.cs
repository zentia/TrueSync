// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.Joint
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
    public abstract class Joint
    {
        public bool Enabled = true;
        internal JointEdge EdgeA = new JointEdge();
        internal JointEdge EdgeB = new JointEdge();
        private FP _breakpoint;
        private FP _breakpointSquared;
        internal bool IslandFlag;

        protected Joint()
        {
            this.Breakpoint = FP.MaxValue;
            this.CollideConnected = false;
        }

        protected Joint(Body bodyA, Body bodyB)
          : this()
        {
            Debug.Assert(bodyA != bodyB);
            this.BodyA = bodyA;
            this.BodyB = bodyB;
        }

        protected Joint(Body body)
          : this()
        {
            this.BodyA = body;
        }

        public JointType JointType { get; protected set; }

        public Body BodyA { get; internal set; }

        public Body BodyB { get; internal set; }

        public abstract TSVector2 WorldAnchorA { get; set; }

        public abstract TSVector2 WorldAnchorB { get; set; }

        public object UserData { get; set; }

        public bool CollideConnected { get; set; }

        public FP Breakpoint
        {
            get
            {
                return this._breakpoint;
            }
            set
            {
                this._breakpoint = value;
                this._breakpointSquared = this._breakpoint * this._breakpoint;
            }
        }

        public event Action<Joint, FP> Broke;

        public abstract TSVector2 GetReactionForce(FP invDt);

        public abstract FP GetReactionTorque(FP invDt);

        protected void WakeBodies()
        {
            if (this.BodyA != null)
                this.BodyA.Awake = true;
            if (this.BodyB == null)
                return;
            this.BodyB.Awake = true;
        }

        public bool IsFixedType()
        {
            return this.JointType == JointType.FixedRevolute || this.JointType == JointType.FixedDistance || (this.JointType == JointType.FixedPrismatic || this.JointType == JointType.FixedLine) || (this.JointType == JointType.FixedMouse || this.JointType == JointType.FixedAngle) || this.JointType == JointType.FixedFriction;
        }

        internal abstract void InitVelocityConstraints(ref SolverData data);

        internal void Validate(FP invDt)
        {
            if (!this.Enabled)
                return;
            FP x = this.GetReactionForce(invDt).LengthSquared();
            if (FP.Abs(x) <= this._breakpointSquared)
                return;
            this.Enabled = false;
            // ISSUE: reference to a compiler-generated field
            if (this.Broke == null)
                return;
            // ISSUE: reference to a compiler-generated field
            this.Broke(this, FP.Sqrt(x));
        }

        internal abstract void SolveVelocityConstraints(ref SolverData data);

        internal abstract bool SolvePositionConstraints(ref SolverData data);
    }
}
