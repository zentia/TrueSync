namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using TrueSync.Physics2D;

    internal class ContactEdgeClone2D
    {
        public Body body;
        public string contactKey;
        public string nextEdge;
        public string previousEdge;

        public void Clone(ContactEdge contactEdge)
        {
            this.contactKey = contactEdge.Contact.Key;
            this.body = contactEdge.Other;
            if (contactEdge.Next > null)
            {
                this.nextEdge = contactEdge.Next.Contact.Key + "_" + contactEdge.Next.Other.BodyId;
            }
            else
            {
                this.nextEdge = null;
            }
            if (contactEdge.Prev > null)
            {
                this.previousEdge = contactEdge.Prev.Contact.Key + "_" + contactEdge.Prev.Other.BodyId;
            }
            else
            {
                this.previousEdge = null;
            }
        }

        public ContactEdge Restore(bool restoreLinks, Dictionary<string, TrueSync.Physics2D.Contact> contactDic, Dictionary<string, ContactEdge> contactEdgeDic)
        {
            string key = this.contactKey + "_" + this.body.BodyId;
            if (restoreLinks)
            {
                ContactEdge edge = contactEdgeDic[key];
                if (this.nextEdge > null)
                {
                    edge.Next = contactEdgeDic[this.nextEdge];
                }
                if (this.previousEdge > null)
                {
                    edge.Prev = contactEdgeDic[this.previousEdge];
                }
                return edge;
            }
            if (contactEdgeDic.ContainsKey(key))
            {
                return contactEdgeDic[key];
            }
            ContactEdge edge3 = WorldClone2D.poolContactEdge.GetNew();
            edge3.Contact = contactDic[this.contactKey];
            edge3.Other = this.body;
            contactEdgeDic[key] = edge3;
            return edge3;
        }
    }
}

