using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TrueSync.Physics2D
{
	public abstract class Joint
	{
		private FP _breakpoint;

		private FP _breakpointSquared;

		public bool Enabled = true;

		internal JointEdge EdgeA = new JointEdge();

		internal JointEdge EdgeB = new JointEdge();

		internal bool IslandFlag;

		[method: CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
		public event Action<Joint, FP> Broke;

		public JointType JointType
		{
			get;
			protected set;
		}

		public Body BodyA
		{
			get;
			internal set;
		}

		public Body BodyB
		{
			get;
			internal set;
		}

		public abstract TSVector2 WorldAnchorA
		{
			get;
			set;
		}

		public abstract TSVector2 WorldAnchorB
		{
			get;
			set;
		}

		public object UserData
		{
			get;
			set;
		}

		public bool CollideConnected
		{
			get;
			set;
		}

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

		protected Joint()
		{
			this.Breakpoint = FP.MaxValue;
			this.CollideConnected = false;
		}

		protected Joint(Body bodyA, Body bodyB) : this()
		{
			Debug.Assert(bodyA != bodyB);
			this.BodyA = bodyA;
			this.BodyB = bodyB;
		}

		protected Joint(Body body) : this()
		{
			this.BodyA = body;
		}

		public abstract TSVector2 GetReactionForce(FP invDt);

		public abstract FP GetReactionTorque(FP invDt);

		protected void WakeBodies()
		{
			bool flag = this.BodyA != null;
			if (flag)
			{
				this.BodyA.Awake = true;
			}
			bool flag2 = this.BodyB != null;
			if (flag2)
			{
				this.BodyB.Awake = true;
			}
		}

		public bool IsFixedType()
		{
			return this.JointType == JointType.FixedRevolute || this.JointType == JointType.FixedDistance || this.JointType == JointType.FixedPrismatic || this.JointType == JointType.FixedLine || this.JointType == JointType.FixedMouse || this.JointType == JointType.FixedAngle || this.JointType == JointType.FixedFriction;
		}

		internal abstract void InitVelocityConstraints(ref SolverData data);

		internal void Validate(FP invDt)
		{
			bool flag = !this.Enabled;
			if (!flag)
			{
				FP fP = this.GetReactionForce(invDt).LengthSquared();
				bool flag2 = FP.Abs(fP) <= this._breakpointSquared;
				if (!flag2)
				{
					this.Enabled = false;
					bool flag3 = this.Broke != null;
					if (flag3)
					{
						this.Broke(this, FP.Sqrt(fP));
					}
				}
			}
		}

		internal abstract void SolveVelocityConstraints(ref SolverData data);

		internal abstract bool SolvePositionConstraints(ref SolverData data);
	}
}
