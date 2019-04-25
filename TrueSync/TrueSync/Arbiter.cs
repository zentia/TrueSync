namespace TrueSync
{
    using System;
    using System.Diagnostics;

    public class Arbiter : IComparable, ResourcePoolItem
    {
        internal RigidBody body1;
        internal RigidBody body2;
        internal TrueSync.ContactList contactList;
        public static ResourcePool<Arbiter> Pool = new ResourcePool<Arbiter>();

        public Arbiter()
        {
            this.contactList = new TrueSync.ContactList();
        }

        public Arbiter(RigidBody body1, RigidBody body2)
        {
            this.contactList = new TrueSync.ContactList();
            this.body1 = body1;
            this.body2 = body2;
        }

        public Contact AddContact(TSVector point1, TSVector point2, TSVector normal, FP penetration, ContactSettings contactSettings)
        {
            TSVector vector;
            TSVector.Subtract(ref point1, ref this.body1.position, out vector);
            TrueSync.ContactList contactList = this.contactList;
            lock (contactList)
            {
                int cacheEntry;
                if (this.contactList.Count == 4)
                {
                    cacheEntry = this.SortCachedPoints(ref vector, penetration);
                    this.ReplaceContact(ref point1, ref point2, ref normal, penetration, cacheEntry, contactSettings);
                    return null;
                }
                cacheEntry = this.GetCacheEntry(ref vector, contactSettings.breakThreshold);
                if (cacheEntry >= 0)
                {
                    this.ReplaceContact(ref point1, ref point2, ref normal, penetration, cacheEntry, contactSettings);
                    return null;
                }
                Contact item = Contact.Pool.GetNew();
                item.Initialize(this.body1, this.body2, ref point1, ref point2, ref normal, penetration, true, contactSettings);
                this.contactList.Add(item);
                return item;
            }
        }

        public void CleanUp()
        {
            this.contactList.Clear();
        }

        public int CompareTo(object obj)
        {
            if (obj is Arbiter)
            {
                long hashCode = ((Arbiter) obj).GetHashCode();
                long num2 = this.GetHashCode();
                long num3 = hashCode - num2;
                if (num3 < 0L)
                {
                    return 1;
                }
                if (num3 > 0L)
                {
                    return -1;
                }
            }
            return 0;
        }

        private int GetCacheEntry(ref TSVector realRelPos1, FP contactBreakThreshold)
        {
            FP fp = contactBreakThreshold * contactBreakThreshold;
            int count = this.contactList.Count;
            int num2 = -1;
            for (int i = 0; i < count; i++)
            {
                TSVector vector;
                TSVector.Subtract(ref this.contactList[i].relativePos1, ref realRelPos1, out vector);
                FP sqrMagnitude = vector.sqrMagnitude;
                if (sqrMagnitude < fp)
                {
                    fp = sqrMagnitude;
                    num2 = i;
                }
            }
            return num2;
        }

        public override int GetHashCode()
        {
            return (this.Body1.GetHashCode() + this.Body2.GetHashCode());
        }

        public void Invalidate()
        {
            this.contactList.Clear();
        }

        internal static int MaxAxis(FP x, FP y, FP z, FP w)
        {
            int num = -1;
            FP minValue = FP.MinValue;
            if (x > minValue)
            {
                num = 0;
                minValue = x;
            }
            if (y > minValue)
            {
                num = 1;
                minValue = y;
            }
            if (z > minValue)
            {
                num = 2;
                minValue = z;
            }
            if (w > minValue)
            {
                num = 3;
                minValue = w;
            }
            return num;
        }

        private void ReplaceContact(ref TSVector point1, ref TSVector point2, ref TSVector n, FP p, int index, ContactSettings contactSettings)
        {
            Contact contact = this.contactList[index];
            Debug.Assert(this.body1 == contact.body1, "Body1 and Body2 not consistent.");
            contact.Initialize(this.body1, this.body2, ref point1, ref point2, ref n, p, false, contactSettings);
        }

        private int SortCachedPoints(ref TSVector realRelPos1, FP pen)
        {
            int num = -1;
            FP penetration = pen;
            for (int i = 0; i < 4; i++)
            {
                if (this.contactList[i].penetration > penetration)
                {
                    num = i;
                    penetration = this.contactList[i].penetration;
                }
            }
            FP x = 0;
            FP y = 0;
            FP z = 0;
            FP w = 0;
            if (num > 0)
            {
                TSVector vector;
                TSVector vector2;
                TSVector vector3;
                TSVector.Subtract(ref realRelPos1, ref this.contactList[1].relativePos1, out vector);
                TSVector.Subtract(ref this.contactList[3].relativePos1, ref this.contactList[2].relativePos1, out vector2);
                TSVector.Cross(ref vector, ref vector2, out vector3);
                x = vector3.sqrMagnitude;
            }
            if (num != 1)
            {
                TSVector vector4;
                TSVector vector5;
                TSVector vector6;
                TSVector.Subtract(ref realRelPos1, ref this.contactList[0].relativePos1, out vector4);
                TSVector.Subtract(ref this.contactList[3].relativePos1, ref this.contactList[2].relativePos1, out vector5);
                TSVector.Cross(ref vector4, ref vector5, out vector6);
                y = vector6.sqrMagnitude;
            }
            if (num != 2)
            {
                TSVector vector7;
                TSVector vector8;
                TSVector vector9;
                TSVector.Subtract(ref realRelPos1, ref this.contactList[0].relativePos1, out vector7);
                TSVector.Subtract(ref this.contactList[3].relativePos1, ref this.contactList[1].relativePos1, out vector8);
                TSVector.Cross(ref vector7, ref vector8, out vector9);
                z = vector9.sqrMagnitude;
            }
            if (num != 3)
            {
                TSVector vector10;
                TSVector vector11;
                TSVector vector12;
                TSVector.Subtract(ref realRelPos1, ref this.contactList[0].relativePos1, out vector10);
                TSVector.Subtract(ref this.contactList[2].relativePos1, ref this.contactList[1].relativePos1, out vector11);
                TSVector.Cross(ref vector10, ref vector11, out vector12);
                w = vector12.sqrMagnitude;
            }
            return MaxAxis(x, y, z, w);
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

        public TrueSync.ContactList ContactList
        {
            get
            {
                return this.contactList;
            }
        }
    }
}

