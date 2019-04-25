namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public class WorldClone : IWorldClone
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string <checksum>k__BackingField;
        public List<Arbiter> arbiterToGiveBack = new List<Arbiter>();
        public CollisionSystemPersistentSAPClone cloneCollision = new CollisionSystemPersistentSAPClone();
        public List<ArbiterClone> clonedArbiters = new List<ArbiterClone>();
        public List<ArbiterClone> clonedArbitersTrigger = new List<ArbiterClone>();
        public List<OverlapPairContact> clonedInitialCollisions = new List<OverlapPairContact>();
        public List<OverlapPairContact> clonedInitialTriggers = new List<OverlapPairContact>();
        public Dictionary<int, RigidBodyClone> clonedPhysics = new Dictionary<int, RigidBodyClone>();
        public List<CollisionIslandClone> collisionIslands = new List<CollisionIslandClone>();
        public List<CollisionIsland> collisionIslandToGiveBack = new List<CollisionIsland>();
        public int constraintInstanceCount;
        public List<Contact> contactsToGiveBack = new List<Contact>();
        private int index;
        private int length;
        public static ResourcePoolArbiterClone poolArbiterClone = new ResourcePoolArbiterClone();
        public static ResourcePoolCollisionIslandClone poolCollisionIslandClone = new ResourcePoolCollisionIslandClone();
        public static ResourcePoolRigidBodyClone poolRigidBodyClone = new ResourcePoolRigidBodyClone();
        public int rigidBodyInstanceCount;

        public void Clone(IWorld iWorld)
        {
            this.Clone(iWorld, false);
        }

        public void Clone(IWorld iWorld, bool doChecksum)
        {
            World world = (World) iWorld;
            this.Reset();
            if (doChecksum)
            {
                this.checksum = ChecksumExtractor.GetEncodedChecksum();
            }
            this.clonedPhysics.Clear();
            foreach (RigidBody body in world.RigidBodies)
            {
                RigidBodyClone clone = poolRigidBodyClone.GetNew();
                clone.Clone(body);
                this.clonedPhysics.Add(body.GetInstance(), clone);
            }
            this.clonedArbiters.Clear();
            foreach (Arbiter arbiter in world.ArbiterMap.Arbiters)
            {
                ArbiterClone item = poolArbiterClone.GetNew();
                item.Clone(arbiter);
                this.clonedArbiters.Add(item);
            }
            this.clonedArbitersTrigger.Clear();
            foreach (Arbiter arbiter2 in world.ArbiterTriggerMap.Arbiters)
            {
                ArbiterClone clone3 = poolArbiterClone.GetNew();
                clone3.Clone(arbiter2);
                this.clonedArbitersTrigger.Add(clone3);
            }
            this.collisionIslands.Clear();
            this.index = 0;
            this.length = world.islands.Count;
            while (this.index < this.length)
            {
                CollisionIsland ci = world.islands[this.index];
                CollisionIslandClone clone4 = poolCollisionIslandClone.GetNew();
                clone4.Clone(ci);
                this.collisionIslands.Add(clone4);
                this.index++;
            }
            this.cloneCollision.Clone((CollisionSystemPersistentSAP) world.CollisionSystem);
            this.clonedInitialCollisions.Clear();
            this.clonedInitialCollisions.AddRange(world.initialCollisions);
            this.clonedInitialTriggers.Clear();
            this.clonedInitialTriggers.AddRange(world.initialTriggers);
            this.rigidBodyInstanceCount = RigidBody.instanceCount;
            this.constraintInstanceCount = Constraint.instanceCount;
        }

        public void Reset()
        {
            if (this.clonedPhysics > null)
            {
                foreach (RigidBodyClone clone in this.clonedPhysics.Values)
                {
                    clone.Reset();
                    poolRigidBodyClone.GiveBack(clone);
                }
            }
            if (this.collisionIslands > null)
            {
                this.index = 0;
                this.length = this.collisionIslands.Count;
                while (this.index < this.length)
                {
                    CollisionIslandClone clone2 = this.collisionIslands[this.index];
                    clone2.Reset();
                    poolCollisionIslandClone.GiveBack(clone2);
                    this.index++;
                }
            }
            if (this.cloneCollision > null)
            {
                this.cloneCollision.Reset();
            }
            if (this.clonedArbiters > null)
            {
                this.index = 0;
                this.length = this.clonedArbiters.Count;
                while (this.index < this.length)
                {
                    ArbiterClone clone3 = this.clonedArbiters[this.index];
                    clone3.Reset();
                    poolArbiterClone.GiveBack(clone3);
                    this.index++;
                }
            }
            if (this.clonedArbitersTrigger > null)
            {
                this.index = 0;
                this.length = this.clonedArbitersTrigger.Count;
                while (this.index < this.length)
                {
                    ArbiterClone clone4 = this.clonedArbitersTrigger[this.index];
                    clone4.Reset();
                    poolArbiterClone.GiveBack(clone4);
                    this.index++;
                }
            }
        }

        public void Restore(IWorld iWorld)
        {
            World world = (World) iWorld;
            List<RigidBody> list = new List<RigidBody>();
            foreach (RigidBody body in world.RigidBodies)
            {
                if (!this.clonedPhysics.ContainsKey(body.GetInstance()))
                {
                    list.Add(body);
                }
            }
            this.index = 0;
            this.length = list.Count;
            while (this.index < this.length)
            {
                RigidBody body2 = list[this.index];
                world.RemoveBody(body2);
                this.index++;
            }
            foreach (RigidBody body3 in world.RigidBodies)
            {
                if (this.clonedPhysics.ContainsKey(body3.GetInstance()))
                {
                    this.clonedPhysics[body3.GetInstance()].Restore(world, body3);
                    body3.island = null;
                    body3.arbiters.Clear();
                    body3.arbitersTrigger.Clear();
                }
            }
            foreach (Arbiter arbiter in world.ArbiterMap.Arbiters)
            {
                this.index = 0;
                this.length = arbiter.contactList.Count;
                while (this.index < this.length)
                {
                    Contact item = arbiter.contactList[this.index];
                    this.contactsToGiveBack.Add(item);
                    this.index++;
                }
                this.arbiterToGiveBack.Add(arbiter);
            }
            world.ArbiterMap.Clear();
            foreach (Arbiter arbiter2 in world.ArbiterTriggerMap.Arbiters)
            {
                foreach (Contact contact2 in arbiter2.contactList)
                {
                    this.contactsToGiveBack.Add(contact2);
                }
                this.arbiterToGiveBack.Add(arbiter2);
            }
            world.ArbiterTriggerMap.Clear();
            this.index = 0;
            this.length = world.islands.islands.Count;
            while (this.index < this.length)
            {
                CollisionIsland island = world.islands.islands[this.index];
                this.collisionIslandToGiveBack.Add(island);
                this.index++;
            }
            this.index = 0;
            this.length = this.clonedArbiters.Count;
            while (this.index < this.length)
            {
                ArbiterClone clone2 = this.clonedArbiters[this.index];
                Arbiter arb = Arbiter.Pool.GetNew();
                clone2.Restore(arb);
                arb.body1.arbiters.Add(arb);
                arb.body2.arbiters.Add(arb);
                world.ArbiterMap.Add(new ArbiterKey(arb.body1, arb.body2), arb);
                this.index++;
            }
            this.index = 0;
            this.length = this.clonedArbitersTrigger.Count;
            while (this.index < this.length)
            {
                ArbiterClone clone3 = this.clonedArbitersTrigger[this.index];
                Arbiter arbiter4 = Arbiter.Pool.GetNew();
                clone3.Restore(arbiter4);
                arbiter4.body1.arbitersTrigger.Add(arbiter4);
                arbiter4.body2.arbitersTrigger.Add(arbiter4);
                world.ArbiterTriggerMap.Add(new ArbiterKey(arbiter4.body1, arbiter4.body2), arbiter4);
                this.index++;
            }
            world.islands.islands.Clear();
            this.index = 0;
            this.length = this.collisionIslands.Count;
            while (this.index < this.length)
            {
                CollisionIslandClone clone4 = this.collisionIslands[this.index];
                CollisionIsland ci = IslandManager.Pool.GetNew();
                clone4.Restore(ci, world);
                world.islands.islands.Add(ci);
                this.index++;
            }
            this.cloneCollision.Restore((CollisionSystemPersistentSAP) world.CollisionSystem);
            world.initialCollisions.Clear();
            world.initialCollisions.AddRange(this.clonedInitialCollisions);
            world.initialTriggers.Clear();
            world.initialTriggers.AddRange(this.clonedInitialTriggers);
            RigidBody.instanceCount = this.rigidBodyInstanceCount;
            Constraint.instanceCount = this.constraintInstanceCount;
            this.index = 0;
            this.length = this.contactsToGiveBack.Count;
            while (this.index < this.length)
            {
                Contact contact3 = this.contactsToGiveBack[this.index];
                Contact.Pool.GiveBack(contact3);
                this.index++;
            }
            this.contactsToGiveBack.Clear();
            this.index = 0;
            this.length = this.arbiterToGiveBack.Count;
            while (this.index < this.length)
            {
                Arbiter arbiter5 = this.arbiterToGiveBack[this.index];
                Arbiter.Pool.GiveBack(arbiter5);
                this.index++;
            }
            this.arbiterToGiveBack.Clear();
            this.index = 0;
            this.length = this.collisionIslandToGiveBack.Count;
            while (this.index < this.length)
            {
                CollisionIsland island3 = this.collisionIslandToGiveBack[this.index];
                IslandManager.Pool.GiveBack(island3);
                this.index++;
            }
            this.collisionIslandToGiveBack.Clear();
        }

        public string checksum { get; private set; }
    }
}

