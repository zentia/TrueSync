using System;

namespace TrueSync.Physics2D
{
	public sealed class ContactEdge
	{
		public Contact Contact;

		public ContactEdge Next;

		public Body Other;

		public ContactEdge Prev;
	}
}
