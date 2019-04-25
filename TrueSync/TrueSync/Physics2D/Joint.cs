namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using TrueSync;

    public abstract class Joint
    {
        private FP _breakpoint;
        private FP _breakpointSquared;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Body <BodyA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Body <BodyB>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <CollideConnected>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.JointType <JointType>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object <UserData>k__BackingField;
        internal JointEdge EdgeA;
        internal JointEdge EdgeB;
        public bool Enabled;
        internal bool IslandFlag;

        [field: CompilerGenerated, DebuggerBrowsable(0)]
        public event Action<TrueSync.Physics2D.Joint, FP> Broke;

        protected Joint()
        {
            this.Enabled = true;
            this.EdgeA = new JointEdge();
            this.EdgeB = new JointEdge();
            this.Breakpoint = FP.MaxValue;
            this.CollideConnected = false;
        }

        protected Joint(Body body) : this()
        {
            this.BodyA = body;
        }

        protected Joint(Body bodyA, Body bodyB) : this()
        {
            Debug.Assert(bodyA != bodyB);
            this.BodyA = bodyA;
            this.BodyB = bodyB;
        }

        public abstract TSVector2 GetReactionForce(FP invDt);
        public abstract FP GetReactionTorque(FP invDt);
        internal abstract void InitVelocityConstraints(ref SolverData data);
        public bool IsFixedType()
        {
            return (((((this.JointType == TrueSync.Physics2D.JointType.FixedRevolute) || (this.JointType == TrueSync.Physics2D.JointType.FixedDistance)) || ((this.JointType == TrueSync.Physics2D.JointType.FixedPrismatic) || (this.JointType == TrueSync.Physics2D.JointType.FixedLine))) || ((this.JointType == TrueSync.Physics2D.JointType.FixedMouse) || (this.JointType == TrueSync.Physics2D.JointType.FixedAngle))) || (this.JointType == TrueSync.Physics2D.JointType.FixedFriction));
        }

        internal abstract bool SolvePositionConstraints(ref SolverData data);
        internal abstract void SolveVelocityConstraints(ref SolverData data);
        internal void Validate(FP invDt)
        {
            if (this.Enabled)
            {
                FP fp = this.GetReactionForce(invDt).LengthSquared();
                if (FP.Abs(fp) > this._breakpointSquared)
                {
                    this.Enabled = false;
                    if (this.Broke > null)
                    {
                        this.Broke(this, FP.Sqrt(fp));
                    }
                }
            }
        }

        protected void WakeBodies()
        {
            if (this.BodyA > null)
            {
                this.BodyA.Awake = true;
            }
            if (this.BodyB > null)
            {
                this.BodyB.Awake = true;
            }
        }

        public Body BodyA { get; internal set; }

        public Body BodyB { get; internal set; }

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

        public bool CollideConnected { get; set; }

        public TrueSync.Physics2D.JointType JointType { get; protected set; }

        public object UserData { get; set; }

        public abstract TSVector2 WorldAnchorA { get; set; }

        public abstract TSVector2 WorldAnchorB { get; set; }
    }
}

