using System;
using System.Collections.Generic;
using TrueSync.Physics2D;

namespace TrueSync
{
	internal class IslandClone2D
	{
		private string[] _contactsKeys;

		public Body[] Bodies;

		public int BodyCount;

		public int ContactCount;

		public int JointCount;

		public Velocity[] _velocities;

		public Position[] _positions;

		public int BodyCapacity;

		public int ContactCapacity;

		public int JointCapacity;

		public void Clone(Island island)
		{
			this.Bodies = new Body[island.BodyCount];
			Array.Copy(island.Bodies, this.Bodies, this.Bodies.Length);
			this._velocities = new Velocity[island.BodyCount];
			Array.Copy(island._velocities, this._velocities, this._velocities.Length);
			this._positions = new Position[island.BodyCount];
			Array.Copy(island._positions, this._positions, this._positions.Length);
			this._contactsKeys = new string[island.ContactCount];
			for (int i = 0; i < this._contactsKeys.Length; i++)
			{
				bool flag = island._contacts[i] == null;
				if (flag)
				{
					break;
				}
				this._contactsKeys[i] = island._contacts[i].Key;
			}
			this.BodyCount = island.BodyCount;
			this.ContactCount = island.ContactCount;
			this.JointCount = island.JointCount;
			this.BodyCapacity = island.BodyCapacity;
			this.ContactCapacity = island.ContactCapacity;
			this.JointCapacity = island.JointCapacity;
		}

		public void Restore(Island island, Dictionary<string, TrueSync.Physics2D.Contact> contactDic)
		{
			island.Reset(this.BodyCapacity, this.ContactCapacity, this.JointCapacity, island._contactManager);
			Array.Copy(this.Bodies, island.Bodies, this.BodyCount);
			Array.Copy(this._velocities, island._velocities, this.BodyCount);
			Array.Copy(this._positions, island._positions, this.BodyCount);
			for (int i = 0; i < this._contactsKeys.Length; i++)
			{
				bool flag = this._contactsKeys[i] == null;
				if (flag)
				{
					break;
				}
				island._contacts[i] = contactDic[this._contactsKeys[i]];
			}
			island.BodyCount = this.BodyCount;
			island.ContactCount = this.ContactCount;
			island.JointCount = this.JointCount;
		}
	}
}
