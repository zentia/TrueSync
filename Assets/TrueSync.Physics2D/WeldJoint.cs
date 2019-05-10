using Microsoft.Xna.Framework;
using System;

namespace TrueSync.Physics2D
{
	public class WeldJoint : Joint
	{
		private Vector3 _impulse;

		private FP _gamma;

		private FP _bias;

		private int _indexA;

		private int _indexB;

		private TSVector2 _rA;

		private TSVector2 _rB;

		private TSVector2 _localCenterA;

		private TSVector2 _localCenterB;

		private FP _invMassA;

		private FP _invMassB;

		private FP _invIA;

		private FP _invIB;

		private Mat33 _mass;

		public TSVector2 LocalAnchorA
		{
			get;
			set;
		}

		public TSVector2 LocalAnchorB
		{
			get;
			set;
		}

		public override TSVector2 WorldAnchorA
		{
			get
			{
				return base.BodyA.GetWorldPoint(this.LocalAnchorA);
			}
			set
			{
				this.LocalAnchorA = base.BodyA.GetLocalPoint(value);
			}
		}

		public override TSVector2 WorldAnchorB
		{
			get
			{
				return base.BodyB.GetWorldPoint(this.LocalAnchorB);
			}
			set
			{
				this.LocalAnchorB = base.BodyB.GetLocalPoint(value);
			}
		}

		public FP ReferenceAngle
		{
			get;
			set;
		}

		public FP FrequencyHz
		{
			get;
			set;
		}

		public FP DampingRatio
		{
			get;
			set;
		}

		internal WeldJoint()
		{
			base.JointType = JointType.Weld;
		}

		public WeldJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Weld;
			if (useWorldCoordinates)
			{
				this.LocalAnchorA = bodyA.GetLocalPoint(anchorA);
				this.LocalAnchorB = bodyB.GetLocalPoint(anchorB);
			}
			else
			{
				this.LocalAnchorA = anchorA;
				this.LocalAnchorB = anchorB;
			}
			this.ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			return invDt * new TSVector2(this._impulse.X, this._impulse.Y);
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return invDt * this._impulse.Z;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			this._indexA = base.BodyA.IslandIndex;
			this._indexB = base.BodyB.IslandIndex;
			this._localCenterA = base.BodyA._sweep.LocalCenter;
			this._localCenterB = base.BodyB._sweep.LocalCenter;
			this._invMassA = base.BodyA._invMass;
			this._invMassB = base.BodyB._invMass;
			this._invIA = base.BodyA._invI;
			this._invIB = base.BodyB._invI;
			FP a = data.positions[this._indexA].a;
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			FP a2 = data.positions[this._indexB].a;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			this._rA = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			this._rB = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			Mat33 mat = default(Mat33);
			mat.ex.X = invMassA + invMassB + this._rA.y * this._rA.y * invIA + this._rB.y * this._rB.y * invIB;
			mat.ey.X = -this._rA.y * this._rA.x * invIA - this._rB.y * this._rB.x * invIB;
			mat.ez.X = -this._rA.y * invIA - this._rB.y * invIB;
			mat.ex.Y = mat.ey.X;
			mat.ey.Y = invMassA + invMassB + this._rA.x * this._rA.x * invIA + this._rB.x * this._rB.x * invIB;
			mat.ez.Y = this._rA.x * invIA + this._rB.x * invIB;
			mat.ex.Z = mat.ez.X;
			mat.ey.Z = mat.ez.Y;
			mat.ez.Z = invIA + invIB;
			bool flag = this.FrequencyHz > 0f;
			if (flag)
			{
				mat.GetInverse22(ref this._mass);
				FP fP3 = invIA + invIB;
				FP fP4 = (fP3 > 0f) ? (1f / fP3) : 0f;
				FP x = a2 - a - this.ReferenceAngle;
				FP y = 2f * Settings.Pi * this.FrequencyHz;
				FP x2 = 2f * fP4 * this.DampingRatio * y;
				FP y2 = fP4 * y * y;
				FP dt = data.step.dt;
				this._gamma = dt * (x2 + dt * y2);
				this._gamma = ((this._gamma != 0f) ? (1f / this._gamma) : 0f);
				this._bias = x * dt * y2 * this._gamma;
				fP3 += this._gamma;
				this._mass.ez.Z = ((fP3 != 0f) ? (1f / fP3) : 0f);
			}
			else
			{
				mat.GetSymInverse33(ref this._mass);
				this._gamma = 0f;
				this._bias = 0f;
			}
			this._impulse *= data.step.dtRatio;
			TSVector2 tSVector3 = new TSVector2(this._impulse.X, this._impulse.Y);
			tSVector -= invMassA * tSVector3;
			fP -= invIA * (MathUtils.Cross(this._rA, tSVector3) + this._impulse.Z);
			tSVector2 += invMassB * tSVector3;
			fP2 += invIB * (MathUtils.Cross(this._rB, tSVector3) + this._impulse.Z);
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
			data.velocities[this._indexB].v = tSVector2;
			data.velocities[this._indexB].w = fP2;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			bool flag = this.FrequencyHz > 0f;
			if (flag)
			{
				FP x = fP2 - fP;
				FP y = -this._mass.ez.Z * (x + this._bias + this._gamma * this._impulse.Z);
				this._impulse.Z = this._impulse.Z + y;
				fP -= invIA * y;
				fP2 += invIB * y;
				TSVector2 v = tSVector2 + MathUtils.Cross(fP2, this._rB) - tSVector - MathUtils.Cross(fP, this._rA);
				TSVector2 tSVector3 = -MathUtils.Mul22(this._mass, v);
				this._impulse.X = this._impulse.X + tSVector3.x;
				this._impulse.Y = this._impulse.Y + tSVector3.y;
				TSVector2 tSVector4 = tSVector3;
				tSVector -= invMassA * tSVector4;
				fP -= invIA * MathUtils.Cross(this._rA, tSVector4);
				tSVector2 += invMassB * tSVector4;
				fP2 += invIB * MathUtils.Cross(this._rB, tSVector4);
			}
			else
			{
				TSVector2 tSVector5 = tSVector2 + MathUtils.Cross(fP2, this._rB) - tSVector - MathUtils.Cross(fP, this._rA);
				FP z = fP2 - fP;
				Vector3 v2 = new Vector3(tSVector5.x, tSVector5.y, z);
				Vector3 vector = -MathUtils.Mul(this._mass, v2);
				this._impulse += vector;
				TSVector2 tSVector6 = new TSVector2(vector.X, vector.Y);
				tSVector -= invMassA * tSVector6;
				fP -= invIA * (MathUtils.Cross(this._rA, tSVector6) + vector.Z);
				tSVector2 += invMassB * tSVector6;
				fP2 += invIB * (MathUtils.Cross(this._rB, tSVector6) + vector.Z);
			}
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
			data.velocities[this._indexB].v = tSVector2;
			data.velocities[this._indexB].w = fP2;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.positions[this._indexA].c;
			FP fP = data.positions[this._indexA].a;
			TSVector2 tSVector2 = data.positions[this._indexB].c;
			FP fP2 = data.positions[this._indexB].a;
			Rot q = new Rot(fP);
			Rot q2 = new Rot(fP2);
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			TSVector2 tSVector3 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			TSVector2 tSVector4 = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			Mat33 mat = default(Mat33);
			mat.ex.X = invMassA + invMassB + tSVector3.y * tSVector3.y * invIA + tSVector4.y * tSVector4.y * invIB;
			mat.ey.X = -tSVector3.y * tSVector3.x * invIA - tSVector4.y * tSVector4.x * invIB;
			mat.ez.X = -tSVector3.y * invIA - tSVector4.y * invIB;
			mat.ex.Y = mat.ey.X;
			mat.ey.Y = invMassA + invMassB + tSVector3.x * tSVector3.x * invIA + tSVector4.x * tSVector4.x * invIB;
			mat.ez.Y = tSVector3.x * invIA + tSVector4.x * invIB;
			mat.ex.Z = mat.ez.X;
			mat.ey.Z = mat.ez.Y;
			mat.ez.Z = invIA + invIB;
			bool flag = this.FrequencyHz > 0f;
			FP magnitude;
			FP x;
			if (flag)
			{
				TSVector2 b = tSVector2 + tSVector4 - tSVector - tSVector3;
				magnitude = b.magnitude;
				x = 0f;
				TSVector2 tSVector5 = -mat.Solve22(b);
				tSVector -= invMassA * tSVector5;
				fP -= invIA * MathUtils.Cross(tSVector3, tSVector5);
				tSVector2 += invMassB * tSVector5;
				fP2 += invIB * MathUtils.Cross(tSVector4, tSVector5);
			}
			else
			{
				TSVector2 tSVector6 = tSVector2 + tSVector4 - tSVector - tSVector3;
				FP fP3 = fP2 - fP - this.ReferenceAngle;
				magnitude = tSVector6.magnitude;
				x = FP.Abs(fP3);
				Vector3 b2 = new Vector3(tSVector6.x, tSVector6.y, fP3);
				Vector3 vector = -mat.Solve33(b2);
				TSVector2 tSVector7 = new TSVector2(vector.X, vector.Y);
				tSVector -= invMassA * tSVector7;
				fP -= invIA * (MathUtils.Cross(tSVector3, tSVector7) + vector.Z);
				tSVector2 += invMassB * tSVector7;
				fP2 += invIB * (MathUtils.Cross(tSVector4, tSVector7) + vector.Z);
			}
			data.positions[this._indexA].c = tSVector;
			data.positions[this._indexA].a = fP;
			data.positions[this._indexB].c = tSVector2;
			data.positions[this._indexB].a = fP2;
			return magnitude <= Settings.LinearSlop && x <= Settings.AngularSlop;
		}
	}
}
