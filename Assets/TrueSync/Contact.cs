using System;

namespace TrueSync
{
	public class Contact : IConstraint
	{
		public ContactSettings settings;

		public RigidBody body1;

		public RigidBody body2;

		public TSVector normal;

		public TSVector tangent;

		public TSVector realRelPos1;

		public TSVector realRelPos2;

		public TSVector relativePos1;

		public TSVector relativePos2;

		public TSVector p1;

		public TSVector p2;

		public FP accumulatedNormalImpulse = FP.Zero;

		public FP accumulatedTangentImpulse = FP.Zero;

		public FP penetration = FP.Zero;

		public FP initialPen = FP.Zero;

		public FP staticFriction;

		public FP dynamicFriction;

		public FP restitution;

		public FP friction = FP.Zero;

		public FP massNormal = FP.Zero;

		public FP massTangent = FP.Zero;

		public FP restitutionBias = FP.Zero;

		public bool newContact = false;

		public bool treatBody1AsStatic = false;

		public bool treatBody2AsStatic = false;

		public bool body1IsMassPoint;

		public bool body2IsMassPoint;

		public FP lostSpeculativeBounce = FP.Zero;

		public FP speculativeVelocity = FP.Zero;

		public static readonly ResourcePool<Contact> Pool = new ResourcePool<Contact>();

		public FP lastTimeStep = FP.PositiveInfinity;

		public FP Restitution
		{
			get
			{
				return this.restitution;
			}
			set
			{
				this.restitution = value;
			}
		}

		public FP StaticFriction
		{
			get
			{
				return this.staticFriction;
			}
			set
			{
				this.staticFriction = value;
			}
		}

		public FP DynamicFriction
		{
			get
			{
				return this.dynamicFriction;
			}
			set
			{
				this.dynamicFriction = value;
			}
		}

		public RigidBody Body1
		{
			get
			{
				return this.body1;
			}
		}

		public RigidBody Body2
		{
			get
			{
				return this.body2;
			}
		}

		public FP Penetration
		{
			get
			{
				return this.penetration;
			}
		}

		public TSVector Position1
		{
			get
			{
				return this.p1;
			}
		}

		public TSVector Position2
		{
			get
			{
				return this.p2;
			}
		}

		public TSVector Tangent
		{
			get
			{
				return this.tangent;
			}
		}

		public TSVector Normal
		{
			get
			{
				return this.normal;
			}
		}

		public FP AppliedNormalImpulse
		{
			get
			{
				return this.accumulatedNormalImpulse;
			}
		}

		public FP AppliedTangentImpulse
		{
			get
			{
				return this.accumulatedTangentImpulse;
			}
		}

		public TSVector CalculateRelativeVelocity()
		{
			FP x = this.body2.angularVelocity.y * this.relativePos2.z - this.body2.angularVelocity.z * this.relativePos2.y + this.body2.linearVelocity.x;
			FP x2 = this.body2.angularVelocity.z * this.relativePos2.x - this.body2.angularVelocity.x * this.relativePos2.z + this.body2.linearVelocity.y;
			FP x3 = this.body2.angularVelocity.x * this.relativePos2.y - this.body2.angularVelocity.y * this.relativePos2.x + this.body2.linearVelocity.z;
			TSVector result;
			result.x = x - this.body1.angularVelocity.y * this.relativePos1.z + this.body1.angularVelocity.z * this.relativePos1.y - this.body1.linearVelocity.x;
			result.y = x2 - this.body1.angularVelocity.z * this.relativePos1.x + this.body1.angularVelocity.x * this.relativePos1.z - this.body1.linearVelocity.y;
			result.z = x3 - this.body1.angularVelocity.x * this.relativePos1.y + this.body1.angularVelocity.y * this.relativePos1.x - this.body1.linearVelocity.z;
			return result;
		}

		public void Iterate()
		{
			bool flag = this.treatBody1AsStatic && this.treatBody2AsStatic;
			if (!flag)
			{
				FP fP = this.body2.linearVelocity.x - this.body1.linearVelocity.x;
				FP fP2 = this.body2.linearVelocity.y - this.body1.linearVelocity.y;
				FP fP3 = this.body2.linearVelocity.z - this.body1.linearVelocity.z;
				bool flag2 = !this.body1IsMassPoint;
				if (flag2)
				{
					fP = fP - this.body1.angularVelocity.y * this.relativePos1.z + this.body1.angularVelocity.z * this.relativePos1.y;
					fP2 = fP2 - this.body1.angularVelocity.z * this.relativePos1.x + this.body1.angularVelocity.x * this.relativePos1.z;
					fP3 = fP3 - this.body1.angularVelocity.x * this.relativePos1.y + this.body1.angularVelocity.y * this.relativePos1.x;
				}
				bool flag3 = !this.body2IsMassPoint;
				if (flag3)
				{
					fP = fP + this.body2.angularVelocity.y * this.relativePos2.z - this.body2.angularVelocity.z * this.relativePos2.y;
					fP2 = fP2 + this.body2.angularVelocity.z * this.relativePos2.x - this.body2.angularVelocity.x * this.relativePos2.z;
					fP3 = fP3 + this.body2.angularVelocity.x * this.relativePos2.y - this.body2.angularVelocity.y * this.relativePos2.x;
				}
				bool flag4 = fP * fP + fP2 * fP2 + fP3 * fP3 < this.settings.minVelocity * this.settings.minVelocity;
				if (!flag4)
				{
					FP x = this.normal.x * fP + this.normal.y * fP2 + this.normal.z * fP3;
					FP y = this.massNormal * (-x + this.restitutionBias + this.speculativeVelocity);
					FP fP4 = this.accumulatedNormalImpulse;
					this.accumulatedNormalImpulse = fP4 + y;
					bool flag5 = this.accumulatedNormalImpulse < FP.Zero;
					if (flag5)
					{
						this.accumulatedNormalImpulse = FP.Zero;
					}
					y = this.accumulatedNormalImpulse - fP4;
					FP x2 = fP * this.tangent.x + fP2 * this.tangent.y + fP3 * this.tangent.z;
					FP fP5 = this.friction * this.accumulatedNormalImpulse;
					FP y2 = this.massTangent * -x2;
					FP fP6 = this.accumulatedTangentImpulse;
					this.accumulatedTangentImpulse = fP6 + y2;
					bool flag6 = this.accumulatedTangentImpulse < -fP5;
					if (flag6)
					{
						this.accumulatedTangentImpulse = -fP5;
					}
					else
					{
						bool flag7 = this.accumulatedTangentImpulse > fP5;
						if (flag7)
						{
							this.accumulatedTangentImpulse = fP5;
						}
					}
					y2 = this.accumulatedTangentImpulse - fP6;
					TSVector tSVector;
					tSVector.x = this.normal.x * y + this.tangent.x * y2;
					tSVector.y = this.normal.y * y + this.tangent.y * y2;
					tSVector.z = this.normal.z * y + this.tangent.z * y2;
					bool flag8 = !this.treatBody1AsStatic;
					if (flag8)
					{
						RigidBody expr_4F9_cp_0_cp_0 = this.body1;
						expr_4F9_cp_0_cp_0.linearVelocity.x = expr_4F9_cp_0_cp_0.linearVelocity.x - tSVector.x * this.body1.inverseMass;
						RigidBody expr_530_cp_0_cp_0 = this.body1;
						expr_530_cp_0_cp_0.linearVelocity.y = expr_530_cp_0_cp_0.linearVelocity.y - tSVector.y * this.body1.inverseMass;
						RigidBody expr_567_cp_0_cp_0 = this.body1;
						expr_567_cp_0_cp_0.linearVelocity.z = expr_567_cp_0_cp_0.linearVelocity.z - tSVector.z * this.body1.inverseMass;
						bool flag9 = !this.body1IsMassPoint;
						if (flag9)
						{
							FP x3 = this.relativePos1.y * tSVector.z - this.relativePos1.z * tSVector.y;
							FP x4 = this.relativePos1.z * tSVector.x - this.relativePos1.x * tSVector.z;
							FP x5 = this.relativePos1.x * tSVector.y - this.relativePos1.y * tSVector.x;
							FP y3 = x3 * this.body1.invInertiaWorld.M11 + x4 * this.body1.invInertiaWorld.M21 + x5 * this.body1.invInertiaWorld.M31;
							FP y4 = x3 * this.body1.invInertiaWorld.M12 + x4 * this.body1.invInertiaWorld.M22 + x5 * this.body1.invInertiaWorld.M32;
							FP y5 = x3 * this.body1.invInertiaWorld.M13 + x4 * this.body1.invInertiaWorld.M23 + x5 * this.body1.invInertiaWorld.M33;
							RigidBody expr_743_cp_0_cp_0 = this.body1;
							expr_743_cp_0_cp_0.angularVelocity.x = expr_743_cp_0_cp_0.angularVelocity.x - y3;
							RigidBody expr_765_cp_0_cp_0 = this.body1;
							expr_765_cp_0_cp_0.angularVelocity.y = expr_765_cp_0_cp_0.angularVelocity.y - y4;
							RigidBody expr_787_cp_0_cp_0 = this.body1;
							expr_787_cp_0_cp_0.angularVelocity.z = expr_787_cp_0_cp_0.angularVelocity.z - y5;
						}
					}
					bool flag10 = !this.treatBody2AsStatic;
					if (flag10)
					{
						RigidBody expr_7BE_cp_0_cp_0 = this.body2;
						expr_7BE_cp_0_cp_0.linearVelocity.x = expr_7BE_cp_0_cp_0.linearVelocity.x + tSVector.x * this.body2.inverseMass;
						RigidBody expr_7F5_cp_0_cp_0 = this.body2;
						expr_7F5_cp_0_cp_0.linearVelocity.y = expr_7F5_cp_0_cp_0.linearVelocity.y + tSVector.y * this.body2.inverseMass;
						RigidBody expr_82C_cp_0_cp_0 = this.body2;
						expr_82C_cp_0_cp_0.linearVelocity.z = expr_82C_cp_0_cp_0.linearVelocity.z + tSVector.z * this.body2.inverseMass;
						bool flag11 = !this.body2IsMassPoint;
						if (flag11)
						{
							FP x6 = this.relativePos2.y * tSVector.z - this.relativePos2.z * tSVector.y;
							FP x7 = this.relativePos2.z * tSVector.x - this.relativePos2.x * tSVector.z;
							FP x8 = this.relativePos2.x * tSVector.y - this.relativePos2.y * tSVector.x;
							FP y6 = x6 * this.body2.invInertiaWorld.M11 + x7 * this.body2.invInertiaWorld.M21 + x8 * this.body2.invInertiaWorld.M31;
							FP y7 = x6 * this.body2.invInertiaWorld.M12 + x7 * this.body2.invInertiaWorld.M22 + x8 * this.body2.invInertiaWorld.M32;
							FP y8 = x6 * this.body2.invInertiaWorld.M13 + x7 * this.body2.invInertiaWorld.M23 + x8 * this.body2.invInertiaWorld.M33;
							RigidBody expr_A08_cp_0_cp_0 = this.body2;
							expr_A08_cp_0_cp_0.angularVelocity.x = expr_A08_cp_0_cp_0.angularVelocity.x + y6;
							RigidBody expr_A2A_cp_0_cp_0 = this.body2;
							expr_A2A_cp_0_cp_0.angularVelocity.y = expr_A2A_cp_0_cp_0.angularVelocity.y + y7;
							RigidBody expr_A4C_cp_0_cp_0 = this.body2;
							expr_A4C_cp_0_cp_0.angularVelocity.z = expr_A4C_cp_0_cp_0.angularVelocity.z + y8;
						}
					}
				}
			}
		}

		public void UpdatePosition()
		{
			bool flag = this.body1IsMassPoint;
			if (flag)
			{
				TSVector.Add(ref this.realRelPos1, ref this.body1.position, out this.p1);
			}
			else
			{
				TSVector.Transform(ref this.realRelPos1, ref this.body1.orientation, out this.p1);
				TSVector.Add(ref this.p1, ref this.body1.position, out this.p1);
			}
			bool flag2 = this.body2IsMassPoint;
			if (flag2)
			{
				TSVector.Add(ref this.realRelPos2, ref this.body2.position, out this.p2);
			}
			else
			{
				TSVector.Transform(ref this.realRelPos2, ref this.body2.orientation, out this.p2);
				TSVector.Add(ref this.p2, ref this.body2.position, out this.p2);
			}
			TSVector tSVector;
			TSVector.Subtract(ref this.p1, ref this.p2, out tSVector);
			this.penetration = TSVector.Dot(ref tSVector, ref this.normal);
		}

		public void ApplyImpulse(ref TSVector impulse)
		{
			bool flag = !this.treatBody1AsStatic;
			if (flag)
			{
				RigidBody expr_22_cp_0_cp_0 = this.body1;
				expr_22_cp_0_cp_0.linearVelocity.x = expr_22_cp_0_cp_0.linearVelocity.x - impulse.x * this.body1.inverseMass;
				RigidBody expr_58_cp_0_cp_0 = this.body1;
				expr_58_cp_0_cp_0.linearVelocity.y = expr_58_cp_0_cp_0.linearVelocity.y - impulse.y * this.body1.inverseMass;
				RigidBody expr_8E_cp_0_cp_0 = this.body1;
				expr_8E_cp_0_cp_0.linearVelocity.z = expr_8E_cp_0_cp_0.linearVelocity.z - impulse.z * this.body1.inverseMass;
				FP x = this.relativePos1.y * impulse.z - this.relativePos1.z * impulse.y;
				FP x2 = this.relativePos1.z * impulse.x - this.relativePos1.x * impulse.z;
				FP x3 = this.relativePos1.x * impulse.y - this.relativePos1.y * impulse.x;
				FP y = x * this.body1.invInertiaWorld.M11 + x2 * this.body1.invInertiaWorld.M21 + x3 * this.body1.invInertiaWorld.M31;
				FP y2 = x * this.body1.invInertiaWorld.M12 + x2 * this.body1.invInertiaWorld.M22 + x3 * this.body1.invInertiaWorld.M32;
				FP y3 = x * this.body1.invInertiaWorld.M13 + x2 * this.body1.invInertiaWorld.M23 + x3 * this.body1.invInertiaWorld.M33;
				RigidBody expr_244_cp_0_cp_0 = this.body1;
				expr_244_cp_0_cp_0.angularVelocity.x = expr_244_cp_0_cp_0.angularVelocity.x - y;
				RigidBody expr_266_cp_0_cp_0 = this.body1;
				expr_266_cp_0_cp_0.angularVelocity.y = expr_266_cp_0_cp_0.angularVelocity.y - y2;
				RigidBody expr_288_cp_0_cp_0 = this.body1;
				expr_288_cp_0_cp_0.angularVelocity.z = expr_288_cp_0_cp_0.angularVelocity.z - y3;
			}
			bool flag2 = !this.treatBody2AsStatic;
			if (flag2)
			{
				RigidBody expr_2BE_cp_0_cp_0 = this.body2;
				expr_2BE_cp_0_cp_0.linearVelocity.x = expr_2BE_cp_0_cp_0.linearVelocity.x + impulse.x * this.body2.inverseMass;
				RigidBody expr_2F4_cp_0_cp_0 = this.body2;
				expr_2F4_cp_0_cp_0.linearVelocity.y = expr_2F4_cp_0_cp_0.linearVelocity.y + impulse.y * this.body2.inverseMass;
				RigidBody expr_32A_cp_0_cp_0 = this.body2;
				expr_32A_cp_0_cp_0.linearVelocity.z = expr_32A_cp_0_cp_0.linearVelocity.z + impulse.z * this.body2.inverseMass;
				FP x4 = this.relativePos2.y * impulse.z - this.relativePos2.z * impulse.y;
				FP x5 = this.relativePos2.z * impulse.x - this.relativePos2.x * impulse.z;
				FP x6 = this.relativePos2.x * impulse.y - this.relativePos2.y * impulse.x;
				FP y4 = x4 * this.body2.invInertiaWorld.M11 + x5 * this.body2.invInertiaWorld.M21 + x6 * this.body2.invInertiaWorld.M31;
				FP y5 = x4 * this.body2.invInertiaWorld.M12 + x5 * this.body2.invInertiaWorld.M22 + x6 * this.body2.invInertiaWorld.M32;
				FP y6 = x4 * this.body2.invInertiaWorld.M13 + x5 * this.body2.invInertiaWorld.M23 + x6 * this.body2.invInertiaWorld.M33;
				RigidBody expr_4EC_cp_0_cp_0 = this.body2;
				expr_4EC_cp_0_cp_0.angularVelocity.x = expr_4EC_cp_0_cp_0.angularVelocity.x + y4;
				RigidBody expr_50E_cp_0_cp_0 = this.body2;
				expr_50E_cp_0_cp_0.angularVelocity.y = expr_50E_cp_0_cp_0.angularVelocity.y + y5;
				RigidBody expr_530_cp_0_cp_0 = this.body2;
				expr_530_cp_0_cp_0.angularVelocity.z = expr_530_cp_0_cp_0.angularVelocity.z + y6;
			}
		}

		public void ApplyImpulse(TSVector impulse)
		{
			bool flag = !this.treatBody1AsStatic;
			if (flag)
			{
				RigidBody expr_22_cp_0_cp_0 = this.body1;
				expr_22_cp_0_cp_0.linearVelocity.x = expr_22_cp_0_cp_0.linearVelocity.x - impulse.x * this.body1.inverseMass;
				RigidBody expr_58_cp_0_cp_0 = this.body1;
				expr_58_cp_0_cp_0.linearVelocity.y = expr_58_cp_0_cp_0.linearVelocity.y - impulse.y * this.body1.inverseMass;
				RigidBody expr_8E_cp_0_cp_0 = this.body1;
				expr_8E_cp_0_cp_0.linearVelocity.z = expr_8E_cp_0_cp_0.linearVelocity.z - impulse.z * this.body1.inverseMass;
				FP x = this.relativePos1.y * impulse.z - this.relativePos1.z * impulse.y;
				FP x2 = this.relativePos1.z * impulse.x - this.relativePos1.x * impulse.z;
				FP x3 = this.relativePos1.x * impulse.y - this.relativePos1.y * impulse.x;
				FP y = x * this.body1.invInertiaWorld.M11 + x2 * this.body1.invInertiaWorld.M21 + x3 * this.body1.invInertiaWorld.M31;
				FP y2 = x * this.body1.invInertiaWorld.M12 + x2 * this.body1.invInertiaWorld.M22 + x3 * this.body1.invInertiaWorld.M32;
				FP y3 = x * this.body1.invInertiaWorld.M13 + x2 * this.body1.invInertiaWorld.M23 + x3 * this.body1.invInertiaWorld.M33;
				RigidBody expr_244_cp_0_cp_0 = this.body1;
				expr_244_cp_0_cp_0.angularVelocity.x = expr_244_cp_0_cp_0.angularVelocity.x - y;
				RigidBody expr_266_cp_0_cp_0 = this.body1;
				expr_266_cp_0_cp_0.angularVelocity.y = expr_266_cp_0_cp_0.angularVelocity.y - y2;
				RigidBody expr_288_cp_0_cp_0 = this.body1;
				expr_288_cp_0_cp_0.angularVelocity.z = expr_288_cp_0_cp_0.angularVelocity.z - y3;
			}
			bool flag2 = !this.treatBody2AsStatic;
			if (flag2)
			{
				RigidBody expr_2BE_cp_0_cp_0 = this.body2;
				expr_2BE_cp_0_cp_0.linearVelocity.x = expr_2BE_cp_0_cp_0.linearVelocity.x + impulse.x * this.body2.inverseMass;
				RigidBody expr_2F4_cp_0_cp_0 = this.body2;
				expr_2F4_cp_0_cp_0.linearVelocity.y = expr_2F4_cp_0_cp_0.linearVelocity.y + impulse.y * this.body2.inverseMass;
				RigidBody expr_32A_cp_0_cp_0 = this.body2;
				expr_32A_cp_0_cp_0.linearVelocity.z = expr_32A_cp_0_cp_0.linearVelocity.z + impulse.z * this.body2.inverseMass;
				FP x4 = this.relativePos2.y * impulse.z - this.relativePos2.z * impulse.y;
				FP x5 = this.relativePos2.z * impulse.x - this.relativePos2.x * impulse.z;
				FP x6 = this.relativePos2.x * impulse.y - this.relativePos2.y * impulse.x;
				FP y4 = x4 * this.body2.invInertiaWorld.M11 + x5 * this.body2.invInertiaWorld.M21 + x6 * this.body2.invInertiaWorld.M31;
				FP y5 = x4 * this.body2.invInertiaWorld.M12 + x5 * this.body2.invInertiaWorld.M22 + x6 * this.body2.invInertiaWorld.M32;
				FP y6 = x4 * this.body2.invInertiaWorld.M13 + x5 * this.body2.invInertiaWorld.M23 + x6 * this.body2.invInertiaWorld.M33;
				RigidBody expr_4EC_cp_0_cp_0 = this.body2;
				expr_4EC_cp_0_cp_0.angularVelocity.x = expr_4EC_cp_0_cp_0.angularVelocity.x + y4;
				RigidBody expr_50E_cp_0_cp_0 = this.body2;
				expr_50E_cp_0_cp_0.angularVelocity.y = expr_50E_cp_0_cp_0.angularVelocity.y + y5;
				RigidBody expr_530_cp_0_cp_0 = this.body2;
				expr_530_cp_0_cp_0.angularVelocity.z = expr_530_cp_0_cp_0.angularVelocity.z + y6;
			}
		}

		public void PrepareForIteration(FP timestep)
		{
			FP fP = this.body2.angularVelocity.y * this.relativePos2.z - this.body2.angularVelocity.z * this.relativePos2.y + this.body2.linearVelocity.x;
			FP fP2 = this.body2.angularVelocity.z * this.relativePos2.x - this.body2.angularVelocity.x * this.relativePos2.z + this.body2.linearVelocity.y;
			FP fP3 = this.body2.angularVelocity.x * this.relativePos2.y - this.body2.angularVelocity.y * this.relativePos2.x + this.body2.linearVelocity.z;
			fP = fP - this.body1.angularVelocity.y * this.relativePos1.z + this.body1.angularVelocity.z * this.relativePos1.y - this.body1.linearVelocity.x;
			fP2 = fP2 - this.body1.angularVelocity.z * this.relativePos1.x + this.body1.angularVelocity.x * this.relativePos1.z - this.body1.linearVelocity.y;
			fP3 = fP3 - this.body1.angularVelocity.x * this.relativePos1.y + this.body1.angularVelocity.y * this.relativePos1.x - this.body1.linearVelocity.z;
			FP fP4 = FP.Zero;
			TSVector zero = TSVector.zero;
			bool flag = !this.treatBody1AsStatic;
			if (flag)
			{
				fP4 += this.body1.inverseMass;
				bool flag2 = !this.body1IsMassPoint;
				if (flag2)
				{
					zero.x = this.relativePos1.y * this.normal.z - this.relativePos1.z * this.normal.y;
					zero.y = this.relativePos1.z * this.normal.x - this.relativePos1.x * this.normal.z;
					zero.z = this.relativePos1.x * this.normal.y - this.relativePos1.y * this.normal.x;
					FP x = zero.x * this.body1.invInertiaWorld.M11 + zero.y * this.body1.invInertiaWorld.M21 + zero.z * this.body1.invInertiaWorld.M31;
					FP y = zero.x * this.body1.invInertiaWorld.M12 + zero.y * this.body1.invInertiaWorld.M22 + zero.z * this.body1.invInertiaWorld.M32;
					FP z = zero.x * this.body1.invInertiaWorld.M13 + zero.y * this.body1.invInertiaWorld.M23 + zero.z * this.body1.invInertiaWorld.M33;
					zero.x = x;
					zero.y = y;
					zero.z = z;
					x = zero.y * this.relativePos1.z - zero.z * this.relativePos1.y;
					y = zero.z * this.relativePos1.x - zero.x * this.relativePos1.z;
					z = zero.x * this.relativePos1.y - zero.y * this.relativePos1.x;
					zero.x = x;
					zero.y = y;
					zero.z = z;
				}
			}
			TSVector zero2 = TSVector.zero;
			bool flag3 = !this.treatBody2AsStatic;
			if (flag3)
			{
				fP4 += this.body2.inverseMass;
				bool flag4 = !this.body2IsMassPoint;
				if (flag4)
				{
					zero2.x = this.relativePos2.y * this.normal.z - this.relativePos2.z * this.normal.y;
					zero2.y = this.relativePos2.z * this.normal.x - this.relativePos2.x * this.normal.z;
					zero2.z = this.relativePos2.x * this.normal.y - this.relativePos2.y * this.normal.x;
					FP x2 = zero2.x * this.body2.invInertiaWorld.M11 + zero2.y * this.body2.invInertiaWorld.M21 + zero2.z * this.body2.invInertiaWorld.M31;
					FP y2 = zero2.x * this.body2.invInertiaWorld.M12 + zero2.y * this.body2.invInertiaWorld.M22 + zero2.z * this.body2.invInertiaWorld.M32;
					FP z2 = zero2.x * this.body2.invInertiaWorld.M13 + zero2.y * this.body2.invInertiaWorld.M23 + zero2.z * this.body2.invInertiaWorld.M33;
					zero2.x = x2;
					zero2.y = y2;
					zero2.z = z2;
					x2 = zero2.y * this.relativePos2.z - zero2.z * this.relativePos2.y;
					y2 = zero2.z * this.relativePos2.x - zero2.x * this.relativePos2.z;
					z2 = zero2.x * this.relativePos2.y - zero2.y * this.relativePos2.x;
					zero2.x = x2;
					zero2.y = y2;
					zero2.z = z2;
				}
			}
			bool flag5 = !this.treatBody1AsStatic;
			if (flag5)
			{
				fP4 += zero.x * this.normal.x + zero.y * this.normal.y + zero.z * this.normal.z;
			}
			bool flag6 = !this.treatBody2AsStatic;
			if (flag6)
			{
				fP4 += zero2.x * this.normal.x + zero2.y * this.normal.y + zero2.z * this.normal.z;
			}
			this.massNormal = FP.One / fP4;
			FP fP5 = fP * this.normal.x + fP2 * this.normal.y + fP3 * this.normal.z;
			this.tangent.x = fP - this.normal.x * fP5;
			this.tangent.y = fP2 - this.normal.y * fP5;
			this.tangent.z = fP3 - this.normal.z * fP5;
			fP5 = this.tangent.x * this.tangent.x + this.tangent.y * this.tangent.y + this.tangent.z * this.tangent.z;
			bool flag7 = fP5 != FP.Zero;
			if (flag7)
			{
				fP5 = FP.Sqrt(fP5);
				this.tangent.x = this.tangent.x / fP5;
				this.tangent.y = this.tangent.y / fP5;
				this.tangent.z = this.tangent.z / fP5;
			}
			FP fP6 = FP.Zero;
			bool flag8 = this.treatBody1AsStatic;
			if (flag8)
			{
				zero.MakeZero();
			}
			else
			{
				fP6 += this.body1.inverseMass;
				bool flag9 = !this.body1IsMassPoint;
				if (flag9)
				{
					zero.x = this.relativePos1.y * this.tangent.z - this.relativePos1.z * this.tangent.y;
					zero.y = this.relativePos1.z * this.tangent.x - this.relativePos1.x * this.tangent.z;
					zero.z = this.relativePos1.x * this.tangent.y - this.relativePos1.y * this.tangent.x;
					FP x3 = zero.x * this.body1.invInertiaWorld.M11 + zero.y * this.body1.invInertiaWorld.M21 + zero.z * this.body1.invInertiaWorld.M31;
					FP y3 = zero.x * this.body1.invInertiaWorld.M12 + zero.y * this.body1.invInertiaWorld.M22 + zero.z * this.body1.invInertiaWorld.M32;
					FP z3 = zero.x * this.body1.invInertiaWorld.M13 + zero.y * this.body1.invInertiaWorld.M23 + zero.z * this.body1.invInertiaWorld.M33;
					zero.x = x3;
					zero.y = y3;
					zero.z = z3;
					x3 = zero.y * this.relativePos1.z - zero.z * this.relativePos1.y;
					y3 = zero.z * this.relativePos1.x - zero.x * this.relativePos1.z;
					z3 = zero.x * this.relativePos1.y - zero.y * this.relativePos1.x;
					zero.x = x3;
					zero.y = y3;
					zero.z = z3;
				}
			}
			bool flag10 = this.treatBody2AsStatic;
			if (flag10)
			{
				zero2.MakeZero();
			}
			else
			{
				fP6 += this.body2.inverseMass;
				bool flag11 = !this.body2IsMassPoint;
				if (flag11)
				{
					zero2.x = this.relativePos2.y * this.tangent.z - this.relativePos2.z * this.tangent.y;
					zero2.y = this.relativePos2.z * this.tangent.x - this.relativePos2.x * this.tangent.z;
					zero2.z = this.relativePos2.x * this.tangent.y - this.relativePos2.y * this.tangent.x;
					FP x4 = zero2.x * this.body2.invInertiaWorld.M11 + zero2.y * this.body2.invInertiaWorld.M21 + zero2.z * this.body2.invInertiaWorld.M31;
					FP y4 = zero2.x * this.body2.invInertiaWorld.M12 + zero2.y * this.body2.invInertiaWorld.M22 + zero2.z * this.body2.invInertiaWorld.M32;
					FP z4 = zero2.x * this.body2.invInertiaWorld.M13 + zero2.y * this.body2.invInertiaWorld.M23 + zero2.z * this.body2.invInertiaWorld.M33;
					zero2.x = x4;
					zero2.y = y4;
					zero2.z = z4;
					x4 = zero2.y * this.relativePos2.z - zero2.z * this.relativePos2.y;
					y4 = zero2.z * this.relativePos2.x - zero2.x * this.relativePos2.z;
					z4 = zero2.x * this.relativePos2.y - zero2.y * this.relativePos2.x;
					zero2.x = x4;
					zero2.y = y4;
					zero2.z = z4;
				}
			}
			bool flag12 = !this.treatBody1AsStatic;
			if (flag12)
			{
				fP6 += TSVector.Dot(ref zero, ref this.tangent);
			}
			bool flag13 = !this.treatBody2AsStatic;
			if (flag13)
			{
				fP6 += TSVector.Dot(ref zero2, ref this.tangent);
			}
			this.massTangent = FP.One / fP6;
			this.restitutionBias = this.lostSpeculativeBounce;
			this.speculativeVelocity = FP.Zero;
			FP y5 = this.normal.x * fP + this.normal.y * fP2 + this.normal.z * fP3;
			bool flag14 = this.Penetration > this.settings.allowedPenetration;
			if (flag14)
			{
				this.restitutionBias = this.settings.bias * (FP.One / timestep) * TSMath.Max(FP.Zero, this.Penetration - this.settings.allowedPenetration);
				this.restitutionBias = TSMath.Clamp(this.restitutionBias, FP.Zero, this.settings.maximumBias);
			}
			FP y6 = timestep / this.lastTimeStep;
			this.accumulatedNormalImpulse *= y6;
			this.accumulatedTangentImpulse *= y6;
			FP y7 = -(this.tangent.x * fP + this.tangent.y * fP2 + this.tangent.z * fP3);
			FP x5 = this.massTangent * y7;
			FP y8 = -this.staticFriction * this.accumulatedNormalImpulse;
			bool flag15 = x5 < y8;
			if (flag15)
			{
				this.friction = this.dynamicFriction;
			}
			else
			{
				this.friction = this.staticFriction;
			}
			this.restitutionBias = TSMath.Max(-this.restitution * y5, this.restitutionBias);
			bool flag16 = this.penetration < -this.settings.allowedPenetration;
			if (flag16)
			{
				this.speculativeVelocity = this.penetration / timestep;
				this.lostSpeculativeBounce = this.restitutionBias;
				this.restitutionBias = FP.Zero;
			}
			else
			{
				this.lostSpeculativeBounce = FP.Zero;
			}
			TSVector tSVector;
			tSVector.x = this.normal.x * this.accumulatedNormalImpulse + this.tangent.x * this.accumulatedTangentImpulse;
			tSVector.y = this.normal.y * this.accumulatedNormalImpulse + this.tangent.y * this.accumulatedTangentImpulse;
			tSVector.z = this.normal.z * this.accumulatedNormalImpulse + this.tangent.z * this.accumulatedTangentImpulse;
			bool flag17 = !this.treatBody1AsStatic;
			if (flag17)
			{
				RigidBody expr_13D4_cp_0_cp_0 = this.body1;
				expr_13D4_cp_0_cp_0.linearVelocity.x = expr_13D4_cp_0_cp_0.linearVelocity.x - tSVector.x * this.body1.inverseMass;
				RigidBody expr_140B_cp_0_cp_0 = this.body1;
				expr_140B_cp_0_cp_0.linearVelocity.y = expr_140B_cp_0_cp_0.linearVelocity.y - tSVector.y * this.body1.inverseMass;
				RigidBody expr_1442_cp_0_cp_0 = this.body1;
				expr_1442_cp_0_cp_0.linearVelocity.z = expr_1442_cp_0_cp_0.linearVelocity.z - tSVector.z * this.body1.inverseMass;
				bool flag18 = !this.body1IsMassPoint;
				if (flag18)
				{
					FP x6 = this.relativePos1.y * tSVector.z - this.relativePos1.z * tSVector.y;
					FP x7 = this.relativePos1.z * tSVector.x - this.relativePos1.x * tSVector.z;
					FP x8 = this.relativePos1.x * tSVector.y - this.relativePos1.y * tSVector.x;
					FP y9 = x6 * this.body1.invInertiaWorld.M11 + x7 * this.body1.invInertiaWorld.M21 + x8 * this.body1.invInertiaWorld.M31;
					FP y10 = x6 * this.body1.invInertiaWorld.M12 + x7 * this.body1.invInertiaWorld.M22 + x8 * this.body1.invInertiaWorld.M32;
					FP y11 = x6 * this.body1.invInertiaWorld.M13 + x7 * this.body1.invInertiaWorld.M23 + x8 * this.body1.invInertiaWorld.M33;
					RigidBody expr_161E_cp_0_cp_0 = this.body1;
					expr_161E_cp_0_cp_0.angularVelocity.x = expr_161E_cp_0_cp_0.angularVelocity.x - y9;
					RigidBody expr_1640_cp_0_cp_0 = this.body1;
					expr_1640_cp_0_cp_0.angularVelocity.y = expr_1640_cp_0_cp_0.angularVelocity.y - y10;
					RigidBody expr_1662_cp_0_cp_0 = this.body1;
					expr_1662_cp_0_cp_0.angularVelocity.z = expr_1662_cp_0_cp_0.angularVelocity.z - y11;
				}
			}
			bool flag19 = !this.treatBody2AsStatic;
			if (flag19)
			{
				RigidBody expr_1699_cp_0_cp_0 = this.body2;
				expr_1699_cp_0_cp_0.linearVelocity.x = expr_1699_cp_0_cp_0.linearVelocity.x + tSVector.x * this.body2.inverseMass;
				RigidBody expr_16D0_cp_0_cp_0 = this.body2;
				expr_16D0_cp_0_cp_0.linearVelocity.y = expr_16D0_cp_0_cp_0.linearVelocity.y + tSVector.y * this.body2.inverseMass;
				RigidBody expr_1707_cp_0_cp_0 = this.body2;
				expr_1707_cp_0_cp_0.linearVelocity.z = expr_1707_cp_0_cp_0.linearVelocity.z + tSVector.z * this.body2.inverseMass;
				bool flag20 = !this.body2IsMassPoint;
				if (flag20)
				{
					FP x9 = this.relativePos2.y * tSVector.z - this.relativePos2.z * tSVector.y;
					FP x10 = this.relativePos2.z * tSVector.x - this.relativePos2.x * tSVector.z;
					FP x11 = this.relativePos2.x * tSVector.y - this.relativePos2.y * tSVector.x;
					FP y12 = x9 * this.body2.invInertiaWorld.M11 + x10 * this.body2.invInertiaWorld.M21 + x11 * this.body2.invInertiaWorld.M31;
					FP y13 = x9 * this.body2.invInertiaWorld.M12 + x10 * this.body2.invInertiaWorld.M22 + x11 * this.body2.invInertiaWorld.M32;
					FP y14 = x9 * this.body2.invInertiaWorld.M13 + x10 * this.body2.invInertiaWorld.M23 + x11 * this.body2.invInertiaWorld.M33;
					RigidBody expr_18E3_cp_0_cp_0 = this.body2;
					expr_18E3_cp_0_cp_0.angularVelocity.x = expr_18E3_cp_0_cp_0.angularVelocity.x + y12;
					RigidBody expr_1905_cp_0_cp_0 = this.body2;
					expr_1905_cp_0_cp_0.angularVelocity.y = expr_1905_cp_0_cp_0.angularVelocity.y + y13;
					RigidBody expr_1927_cp_0_cp_0 = this.body2;
					expr_1927_cp_0_cp_0.angularVelocity.z = expr_1927_cp_0_cp_0.angularVelocity.z + y14;
				}
			}
			this.lastTimeStep = timestep;
			this.newContact = false;
		}

		public void TreatBodyAsStatic(RigidBodyIndex index)
		{
			bool flag = index == RigidBodyIndex.RigidBody1;
			if (flag)
			{
				this.treatBody1AsStatic = true;
			}
			else
			{
				this.treatBody2AsStatic = true;
			}
		}

		public void Initialize(RigidBody body1, RigidBody body2, ref TSVector point1, ref TSVector point2, ref TSVector n, FP penetration, bool newContact, ContactSettings settings)
		{
			this.body1 = body1;
			this.body2 = body2;
			this.normal = n;
			this.normal.Normalize();
			this.p1 = point1;
			this.p2 = point2;
			this.newContact = newContact;
			TSVector.Subtract(ref this.p1, ref body1.position, out this.relativePos1);
			TSVector.Subtract(ref this.p2, ref body2.position, out this.relativePos2);
			TSVector.Transform(ref this.relativePos1, ref body1.invOrientation, out this.realRelPos1);
			TSVector.Transform(ref this.relativePos2, ref body2.invOrientation, out this.realRelPos2);
			this.initialPen = penetration;
			this.penetration = penetration;
			this.body1IsMassPoint = body1.isParticle;
			this.body2IsMassPoint = body2.isParticle;
			if (newContact)
			{
				this.treatBody1AsStatic = body1.isStatic;
				this.treatBody2AsStatic = body2.isStatic;
				this.accumulatedNormalImpulse = FP.Zero;
				this.accumulatedTangentImpulse = FP.Zero;
				this.lostSpeculativeBounce = FP.Zero;
				switch (settings.MaterialCoefficientMixing)
				{
				case ContactSettings.MaterialCoefficientMixingType.TakeMaximum:
					this.staticFriction = TSMath.Max(body1.material.staticFriction, body2.material.staticFriction);
					this.dynamicFriction = TSMath.Max(body1.material.kineticFriction, body2.material.kineticFriction);
					this.restitution = TSMath.Max(body1.material.restitution, body2.material.restitution);
					break;
				case ContactSettings.MaterialCoefficientMixingType.TakeMinimum:
					this.staticFriction = TSMath.Min(body1.material.staticFriction, body2.material.staticFriction);
					this.dynamicFriction = TSMath.Min(body1.material.kineticFriction, body2.material.kineticFriction);
					this.restitution = TSMath.Min(body1.material.restitution, body2.material.restitution);
					break;
				case ContactSettings.MaterialCoefficientMixingType.UseAverage:
					this.staticFriction = (body1.material.staticFriction + body2.material.staticFriction) / (2 * FP.One);
					this.dynamicFriction = (body1.material.kineticFriction + body2.material.kineticFriction) / (2 * FP.One);
					this.restitution = (body1.material.restitution + body2.material.restitution) / (2 * FP.One);
					break;
				}
			}
			this.settings = settings;
		}
	}
}
