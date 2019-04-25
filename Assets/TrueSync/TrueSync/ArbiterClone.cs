namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public class ArbiterClone
    {
        public RigidBody body1;
        public RigidBody body2;
        public List<ContactClone> contactList = new List<ContactClone>();
        private int index;
        private int length;
        public static ResourcePoolContactClone poolContactClone = new ResourcePoolContactClone();

        public void Clone(Arbiter arb)
        {
            this.body1 = arb.body1;
            this.body2 = arb.body2;
            this.contactList.Clear();
            this.index = 0;
            this.length = arb.contactList.Count;
            while (this.index < this.length)
            {
                ContactClone item = poolContactClone.GetNew();
                item.Clone(arb.contactList[this.index]);
                this.contactList.Add(item);
                this.index++;
            }
        }

        public void Reset()
        {
            this.index = 0;
            this.length = this.contactList.Count;
            while (this.index < this.length)
            {
                poolContactClone.GiveBack(this.contactList[this.index]);
                this.index++;
            }
        }

        public void Restore(Arbiter arb)
        {
            arb.body1 = this.body1;
            arb.body2 = this.body2;
            arb.contactList.Clear();
            this.index = 0;
            this.length = this.contactList.Count;
            while (this.index < this.length)
            {
                ContactClone clone = this.contactList[this.index];
                Contact contact = Contact.Pool.GetNew();
                clone.Restore(contact);
                arb.contactList.Add(contact);
                this.index++;
            }
        }
    }
}

