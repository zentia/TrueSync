using System;
using System.Diagnostics;

namespace TrueSync
{
	public class Arbiter : IComparable, ResourcePoolItem
	{
		public static ResourcePool<Arbiter> Pool = new ResourcePool<Arbiter>();

		internal RigidBody body1;

		internal RigidBody body2;

		internal ContactList contactList;

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

		public ContactList ContactList
		{
			get
			{
				return this.contactList;
			}
		}

		public int CompareTo(object obj)
		{
			bool flag = obj is Arbiter;
			int result;
			if (flag)
			{
				long num = (long)((Arbiter)obj).GetHashCode();
				long num2 = (long)this.GetHashCode();
				long num3 = num - num2;
				bool flag2 = num3 < 0L;
				if (flag2)
				{
					result = 1;
					return result;
				}
				bool flag3 = num3 > 0L;
				if (flag3)
				{
					result = -1;
					return result;
				}
			}
			result = 0;
			return result;
		}

		public override int GetHashCode()
		{
			return this.Body1.GetHashCode() + this.Body2.GetHashCode();
		}

		public void CleanUp()
		{
			this.contactList.Clear();
		}

		public Arbiter(RigidBody body1, RigidBody body2)
		{
			this.contactList = new ContactList();
			this.body1 = body1;
			this.body2 = body2;
		}

		public Arbiter()
		{
			this.contactList = new ContactList();
		}

		public void Invalidate()
		{
			this.contactList.Clear();
		}

		public Contact AddContact(TSVector point1, TSVector point2, TSVector normal, FP penetration, ContactSettings contactSettings)
		{
			TSVector tSVector;
			TSVector.Subtract(ref point1, ref this.body1.position, out tSVector);
			ContactList obj = this.contactList;
			Contact result;
			lock (obj)
			{
				bool flag = this.contactList.Count == 4;
				if (flag)
				{
					int num = this.SortCachedPoints(ref tSVector, penetration);
					this.ReplaceContact(ref point1, ref point2, ref normal, penetration, num, contactSettings);
					result = null;
				}
				else
				{
					int num = this.GetCacheEntry(ref tSVector, contactSettings.breakThreshold);
					bool flag2 = num >= 0;
					if (flag2)
					{
						this.ReplaceContact(ref point1, ref point2, ref normal, penetration, num, contactSettings);
						result = null;
					}
					else
					{
						Contact @new = Contact.Pool.GetNew();
						@new.Initialize(this.body1, this.body2, ref point1, ref point2, ref normal, penetration, true, contactSettings);
						this.contactList.Add(@new);
						result = @new;
					}
				}
			}
			return result;
		}

		private void ReplaceContact(ref TSVector point1, ref TSVector point2, ref TSVector n, FP p, int index, ContactSettings contactSettings)
		{
			Contact contact = this.contactList[index];
			Debug.Assert(this.body1 == contact.body1, "Body1 and Body2 not consistent.");
			contact.Initialize(this.body1, this.body2, ref point1, ref point2, ref n, p, false, contactSettings);
		}

		private int GetCacheEntry(ref TSVector realRelPos1, FP contactBreakThreshold)
		{
			FP y = contactBreakThreshold * contactBreakThreshold;
			int count = this.contactList.Count;
			int result = -1;
			for (int i = 0; i < count; i++)
			{
				TSVector tSVector;
				TSVector.Subtract(ref this.contactList[i].relativePos1, ref realRelPos1, out tSVector);
				FP sqrMagnitude = tSVector.sqrMagnitude;
				bool flag = sqrMagnitude < y;
				if (flag)
				{
					y = sqrMagnitude;
					result = i;
				}
			}
			return result;
		}

		private int SortCachedPoints(ref TSVector realRelPos1, FP pen)
		{
			int num = -1;
			FP y = pen;
			for (int i = 0; i < 4; i++)
			{
				bool flag = this.contactList[i].penetration > y;
				if (flag)
				{
					num = i;
					y = this.contactList[i].penetration;
				}
			}
			FP x = 0;
			FP y2 = 0;
			FP z = 0;
			FP w = 0;
			bool flag2 = num != 0;
			if (flag2)
			{
				TSVector tSVector;
				TSVector.Subtract(ref realRelPos1, ref this.contactList[1].relativePos1, out tSVector);
				TSVector tSVector2;
				TSVector.Subtract(ref this.contactList[3].relativePos1, ref this.contactList[2].relativePos1, out tSVector2);
				TSVector tSVector3;
				TSVector.Cross(ref tSVector, ref tSVector2, out tSVector3);
				x = tSVector3.sqrMagnitude;
			}
			bool flag3 = num != 1;
			if (flag3)
			{
				TSVector tSVector4;
				TSVector.Subtract(ref realRelPos1, ref this.contactList[0].relativePos1, out tSVector4);
				TSVector tSVector5;
				TSVector.Subtract(ref this.contactList[3].relativePos1, ref this.contactList[2].relativePos1, out tSVector5);
				TSVector tSVector6;
				TSVector.Cross(ref tSVector4, ref tSVector5, out tSVector6);
				y2 = tSVector6.sqrMagnitude;
			}
			bool flag4 = num != 2;
			if (flag4)
			{
				TSVector tSVector7;
				TSVector.Subtract(ref realRelPos1, ref this.contactList[0].relativePos1, out tSVector7);
				TSVector tSVector8;
				TSVector.Subtract(ref this.contactList[3].relativePos1, ref this.contactList[1].relativePos1, out tSVector8);
				TSVector tSVector9;
				TSVector.Cross(ref tSVector7, ref tSVector8, out tSVector9);
				z = tSVector9.sqrMagnitude;
			}
			bool flag5 = num != 3;
			if (flag5)
			{
				TSVector tSVector10;
				TSVector.Subtract(ref realRelPos1, ref this.contactList[0].relativePos1, out tSVector10);
				TSVector tSVector11;
				TSVector.Subtract(ref this.contactList[2].relativePos1, ref this.contactList[1].relativePos1, out tSVector11);
				TSVector tSVector12;
				TSVector.Cross(ref tSVector10, ref tSVector11, out tSVector12);
				w = tSVector12.sqrMagnitude;
			}
			return Arbiter.MaxAxis(x, y2, z, w);
		}

		internal static int MaxAxis(FP x, FP y, FP z, FP w)
		{
			int result = -1;
			FP y2 = FP.MinValue;
			bool flag = x > y2;
			if (flag)
			{
				result = 0;
				y2 = x;
			}
			bool flag2 = y > y2;
			if (flag2)
			{
				result = 1;
				y2 = y;
			}
			bool flag3 = z > y2;
			if (flag3)
			{
				result = 2;
				y2 = z;
			}
			bool flag4 = w > y2;
			if (flag4)
			{
				result = 3;
			}
			return result;
		}
	}
}
