namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void AfterCollisionEventHandler(Fixture fixtureA, Fixture fixtureB, Contact contact, ContactVelocityConstraint impulse);
}

