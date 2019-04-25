namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using TrueSync.Physics2D;

    internal class IslandClone2D
    {
        private string[] _contactsKeys;
        public Position[] _positions;
        public Velocity[] _velocities;
        public Body[] Bodies;
        public int BodyCapacity;
        public int BodyCount;
        public int ContactCapacity;
        public int ContactCount;
        public int JointCapacity;
        public int JointCount;

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
                if (island._contacts[i] == null)
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
                if (this._contactsKeys[i] == null)
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

