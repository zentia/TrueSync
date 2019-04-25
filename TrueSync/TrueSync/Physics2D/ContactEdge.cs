namespace TrueSync.Physics2D
{
    using System;

    public sealed class ContactEdge
    {
        public TrueSync.Physics2D.Contact Contact;
        public ContactEdge Next;
        public Body Other;
        public ContactEdge Prev;
    }
}

