using System;

namespace TrueSync.Physics2D
{
	public delegate void AfterCollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse);
}
