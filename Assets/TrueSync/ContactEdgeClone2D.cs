using System;
using System.Collections.Generic;
using TrueSync.Physics2D;

namespace TrueSync
{
	internal class ContactEdgeClone2D
	{
		public string contactKey;

		public Body body;

		public string nextEdge;

		public string previousEdge;

		public void Clone(ContactEdge contactEdge)
		{
			this.contactKey = contactEdge.Contact.Key;
			this.body = contactEdge.Other;
			bool flag = contactEdge.Next != null;
			if (flag)
			{
				this.nextEdge = contactEdge.Next.Contact.Key + "_" + contactEdge.Next.Other.BodyId;
			}
			else
			{
				this.nextEdge = null;
			}
			bool flag2 = contactEdge.Prev != null;
			if (flag2)
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
			ContactEdge result;
			if (restoreLinks)
			{
				ContactEdge contactEdge = contactEdgeDic[key];
				bool flag = this.nextEdge != null;
				if (flag)
				{
					contactEdge.Next = contactEdgeDic[this.nextEdge];
				}
				bool flag2 = this.previousEdge != null;
				if (flag2)
				{
					contactEdge.Prev = contactEdgeDic[this.previousEdge];
				}
				result = contactEdge;
			}
			else
			{
				bool flag3 = contactEdgeDic.ContainsKey(key);
				if (flag3)
				{
					result = contactEdgeDic[key];
				}
				else
				{
					ContactEdge @new = WorldClone2D.poolContactEdge.GetNew();
					@new.Contact = contactDic[this.contactKey];
					@new.Other = this.body;
					contactEdgeDic[key] = @new;
					result = @new;
				}
			}
			return result;
		}
	}
}
