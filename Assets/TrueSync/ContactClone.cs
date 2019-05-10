using System;

namespace TrueSync
{
	public class ContactClone
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

		public FP accumulatedNormalImpulse;

		public FP accumulatedTangentImpulse;

		public FP penetration;

		public FP initialPen;

		public FP staticFriction;

		public FP dynamicFriction;

		public FP restitution;

		public FP friction;

		public FP massNormal;

		public FP massTangent;

		public FP restitutionBias;

		public bool newContact;

		public bool treatBody1AsStatic;

		public bool treatBody2AsStatic;

		public bool body1IsMassPoint;

		public bool body2IsMassPoint;

		public FP lostSpeculativeBounce;

		public FP speculativeVelocity;

		public FP lastTimeStep;

		public void Clone(Contact contact)
		{
			this.settings = contact.settings;
			this.body1 = contact.body1;
			this.body2 = contact.body2;
			this.normal = contact.normal;
			this.tangent = contact.tangent;
			this.realRelPos1 = contact.realRelPos1;
			this.realRelPos2 = contact.realRelPos2;
			this.relativePos1 = contact.relativePos1;
			this.relativePos2 = contact.relativePos2;
			this.p1 = contact.p1;
			this.p2 = contact.p2;
			this.accumulatedNormalImpulse = contact.accumulatedNormalImpulse;
			this.accumulatedTangentImpulse = contact.accumulatedTangentImpulse;
			this.penetration = contact.penetration;
			this.initialPen = contact.initialPen;
			this.staticFriction = contact.staticFriction;
			this.dynamicFriction = contact.dynamicFriction;
			this.restitution = contact.restitution;
			this.friction = contact.friction;
			this.massNormal = contact.massNormal;
			this.massTangent = contact.massTangent;
			this.restitutionBias = contact.restitutionBias;
			this.newContact = contact.newContact;
			this.treatBody1AsStatic = contact.treatBody1AsStatic;
			this.treatBody2AsStatic = contact.treatBody2AsStatic;
			this.body1IsMassPoint = contact.body1IsMassPoint;
			this.body2IsMassPoint = contact.body2IsMassPoint;
			this.lostSpeculativeBounce = contact.lostSpeculativeBounce;
			this.speculativeVelocity = contact.speculativeVelocity;
			this.lastTimeStep = contact.lastTimeStep;
		}

		public void Restore(Contact contact)
		{
			contact.settings = this.settings;
			contact.body1 = this.body1;
			contact.body2 = this.body2;
			contact.normal = this.normal;
			contact.tangent = this.tangent;
			contact.realRelPos1 = this.realRelPos1;
			contact.realRelPos2 = this.realRelPos2;
			contact.relativePos1 = this.relativePos1;
			contact.relativePos2 = this.relativePos2;
			contact.p1 = this.p1;
			contact.p2 = this.p2;
			contact.accumulatedNormalImpulse = this.accumulatedNormalImpulse;
			contact.accumulatedTangentImpulse = this.accumulatedTangentImpulse;
			contact.penetration = this.penetration;
			contact.initialPen = this.initialPen;
			contact.staticFriction = this.staticFriction;
			contact.dynamicFriction = this.dynamicFriction;
			contact.restitution = this.restitution;
			contact.friction = this.friction;
			contact.massNormal = this.massNormal;
			contact.massTangent = this.massTangent;
			contact.restitutionBias = this.restitutionBias;
			contact.newContact = this.newContact;
			contact.treatBody1AsStatic = this.treatBody1AsStatic;
			contact.treatBody2AsStatic = this.treatBody2AsStatic;
			contact.body1IsMassPoint = this.body1IsMassPoint;
			contact.body2IsMassPoint = this.body2IsMassPoint;
			contact.lostSpeculativeBounce = this.lostSpeculativeBounce;
			contact.speculativeVelocity = this.speculativeVelocity;
			contact.lastTimeStep = this.lastTimeStep;
		}
	}
}
