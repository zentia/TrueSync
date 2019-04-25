// Decompiled with JetBrains decompiler
// Type: TrueSync.WorldClone
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System.Collections.Generic;

namespace TrueSync
{
    public class WorldClone : IWorldClone
    {
        public static ResourcePoolRigidBodyClone poolRigidBodyClone = new ResourcePoolRigidBodyClone();
        public static ResourcePoolArbiterClone poolArbiterClone = new ResourcePoolArbiterClone();
        public static ResourcePoolCollisionIslandClone poolCollisionIslandClone = new ResourcePoolCollisionIslandClone();
        public Dictionary<int, RigidBodyClone> clonedPhysics = new Dictionary<int, RigidBodyClone>();
        public List<CollisionIslandClone> collisionIslands = new List<CollisionIslandClone>();
        public CollisionSystemPersistentSAPClone cloneCollision = new CollisionSystemPersistentSAPClone();
        public List<ArbiterClone> clonedArbiters = new List<ArbiterClone>();
        public List<OverlapPairContact> clonedInitialCollisions = new List<OverlapPairContact>();
        public List<ArbiterClone> clonedArbitersTrigger = new List<ArbiterClone>();
        public List<OverlapPairContact> clonedInitialTriggers = new List<OverlapPairContact>();
        public List<Contact> contactsToGiveBack = new List<Contact>();
        public List<Arbiter> arbiterToGiveBack = new List<Arbiter>();
        public List<CollisionIsland> collisionIslandToGiveBack = new List<CollisionIsland>();
        public int rigidBodyInstanceCount;
        public int constraintInstanceCount;
        private int index;
        private int length;

        public string checksum { get; private set; }

        public void Reset()
        {
            if (this.clonedPhysics != null)
            {
                foreach (RigidBodyClone rigidBodyClone in this.clonedPhysics.Values)
                {
                    rigidBodyClone.Reset();
                    WorldClone.poolRigidBodyClone.GiveBack(rigidBodyClone);
                }
            }
            if (this.collisionIslands != null)
            {
                this.index = 0;
                for (this.length = this.collisionIslands.Count; this.index < this.length; this.index = this.index + 1)
                {
                    CollisionIslandClone collisionIsland = this.collisionIslands[this.index];
                    collisionIsland.Reset();
                    WorldClone.poolCollisionIslandClone.GiveBack(collisionIsland);
                }
            }
            if (this.cloneCollision != null)
                this.cloneCollision.Reset();
            if (this.clonedArbiters != null)
            {
                this.index = 0;
                for (this.length = this.clonedArbiters.Count; this.index < this.length; this.index = this.index + 1)
                {
                    ArbiterClone clonedArbiter = this.clonedArbiters[this.index];
                    clonedArbiter.Reset();
                    WorldClone.poolArbiterClone.GiveBack(clonedArbiter);
                }
            }
            if (this.clonedArbitersTrigger == null)
                return;
            this.index = 0;
            for (this.length = this.clonedArbitersTrigger.Count; this.index < this.length; this.index = this.index + 1)
            {
                ArbiterClone arbiterClone = this.clonedArbitersTrigger[this.index];
                arbiterClone.Reset();
                WorldClone.poolArbiterClone.GiveBack(arbiterClone);
            }
        }

        public void Clone(IWorld iWorld)
        {
            this.Clone(iWorld, false);
        }

        public void Clone(IWorld iWorld, bool doChecksum)
        {
            World world = (World)iWorld;
            this.Reset();
            if (doChecksum)
                this.checksum = ChecksumExtractor.GetEncodedChecksum();
            this.clonedPhysics.Clear();
            foreach (RigidBody rigidBody in world.RigidBodies)
            {
                RigidBodyClone rigidBodyClone = WorldClone.poolRigidBodyClone.GetNew();
                rigidBodyClone.Clone(rigidBody);
                this.clonedPhysics.Add(rigidBody.GetInstance(), rigidBodyClone);
            }
            this.clonedArbiters.Clear();
            foreach (Arbiter arbiter in world.ArbiterMap.Arbiters)
            {
                ArbiterClone arbiterClone = WorldClone.poolArbiterClone.GetNew();
                arbiterClone.Clone(arbiter);
                this.clonedArbiters.Add(arbiterClone);
            }
            this.clonedArbitersTrigger.Clear();
            foreach (Arbiter arbiter in world.ArbiterTriggerMap.Arbiters)
            {
                ArbiterClone arbiterClone = WorldClone.poolArbiterClone.GetNew();
                arbiterClone.Clone(arbiter);
                this.clonedArbitersTrigger.Add(arbiterClone);
            }
            this.collisionIslands.Clear();
            this.index = 0;
            for (this.length = world.islands.Count; this.index < this.length; this.index = this.index + 1)
            {
                CollisionIsland island = world.islands[this.index];
                CollisionIslandClone collisionIslandClone = WorldClone.poolCollisionIslandClone.GetNew();
                collisionIslandClone.Clone(island);
                this.collisionIslands.Add(collisionIslandClone);
            }
            this.cloneCollision.Clone((CollisionSystemPersistentSAP)world.CollisionSystem);
            this.clonedInitialCollisions.Clear();
            this.clonedInitialCollisions.AddRange((IEnumerable<OverlapPairContact>)world.initialCollisions);
            this.clonedInitialTriggers.Clear();
            this.clonedInitialTriggers.AddRange((IEnumerable<OverlapPairContact>)world.initialTriggers);
            this.rigidBodyInstanceCount = RigidBody.instanceCount;
            this.constraintInstanceCount = Constraint.instanceCount;
        }

        public void Restore(IWorld iWorld)
        {
            World world = (World)iWorld;
            List<RigidBody> rigidBodyList = new List<RigidBody>();
            foreach (RigidBody rigidBody in world.RigidBodies)
            {
                if (!this.clonedPhysics.ContainsKey(rigidBody.GetInstance()))
                    rigidBodyList.Add(rigidBody);
            }
            this.index = 0;
            for (this.length = rigidBodyList.Count; this.index < this.length; this.index = this.index + 1)
            {
                RigidBody body = rigidBodyList[this.index];
                world.RemoveBody(body);
            }
            foreach (RigidBody rigidBody in world.RigidBodies)
            {
                if (this.clonedPhysics.ContainsKey(rigidBody.GetInstance()))
                {
                    this.clonedPhysics[rigidBody.GetInstance()].Restore(world, rigidBody);
                    rigidBody.island = (CollisionIsland)null;
                    rigidBody.arbiters.Clear();
                    rigidBody.arbitersTrigger.Clear();
                }
            }
            foreach (Arbiter arbiter in world.ArbiterMap.Arbiters)
            {
                this.index = 0;
                for (this.length = arbiter.contactList.Count; this.index < this.length; this.index = this.index + 1)
                    this.contactsToGiveBack.Add(arbiter.contactList[this.index]);
                this.arbiterToGiveBack.Add(arbiter);
            }
            world.ArbiterMap.Clear();
            foreach (Arbiter arbiter in world.ArbiterTriggerMap.Arbiters)
            {
                foreach (Contact contact in (List<Contact>)arbiter.contactList)
                    this.contactsToGiveBack.Add(contact);
                this.arbiterToGiveBack.Add(arbiter);
            }
            world.ArbiterTriggerMap.Clear();
            this.index = 0;
            for (this.length = world.islands.islands.Count; this.index < this.length; this.index = this.index + 1)
                this.collisionIslandToGiveBack.Add(world.islands.islands[this.index]);
            this.index = 0;
            for (this.length = this.clonedArbiters.Count; this.index < this.length; this.index = this.index + 1)
            {
                ArbiterClone clonedArbiter = this.clonedArbiters[this.index];
                Arbiter arbiter = Arbiter.Pool.GetNew();
                clonedArbiter.Restore(arbiter);
                arbiter.body1.arbiters.Add(arbiter);
                arbiter.body2.arbiters.Add(arbiter);
                world.ArbiterMap.Add(new ArbiterKey(arbiter.body1, arbiter.body2), arbiter);
            }
            this.index = 0;
            for (this.length = this.clonedArbitersTrigger.Count; this.index < this.length; this.index = this.index + 1)
            {
                ArbiterClone arbiterClone = this.clonedArbitersTrigger[this.index];
                Arbiter arbiter = Arbiter.Pool.GetNew();
                arbiterClone.Restore(arbiter);
                arbiter.body1.arbitersTrigger.Add(arbiter);
                arbiter.body2.arbitersTrigger.Add(arbiter);
                world.ArbiterTriggerMap.Add(new ArbiterKey(arbiter.body1, arbiter.body2), arbiter);
            }
            world.islands.islands.Clear();
            this.index = 0;
            for (this.length = this.collisionIslands.Count; this.index < this.length; this.index = this.index + 1)
            {
                CollisionIslandClone collisionIsland = this.collisionIslands[this.index];
                CollisionIsland ci = IslandManager.Pool.GetNew();
                collisionIsland.Restore(ci, world);
                world.islands.islands.Add(ci);
            }
            this.cloneCollision.Restore((CollisionSystemPersistentSAP)world.CollisionSystem);
            world.initialCollisions.Clear();
            world.initialCollisions.AddRange((IEnumerable<OverlapPairContact>)this.clonedInitialCollisions);
            world.initialTriggers.Clear();
            world.initialTriggers.AddRange((IEnumerable<OverlapPairContact>)this.clonedInitialTriggers);
            RigidBody.instanceCount = this.rigidBodyInstanceCount;
            Constraint.instanceCount = this.constraintInstanceCount;
            this.index = 0;
            for (this.length = this.contactsToGiveBack.Count; this.index < this.length; this.index = this.index + 1)
            {
                Contact contact = this.contactsToGiveBack[this.index];
                Contact.Pool.GiveBack(contact);
            }
            this.contactsToGiveBack.Clear();
            this.index = 0;
            for (this.length = this.arbiterToGiveBack.Count; this.index < this.length; this.index = this.index + 1)
            {
                Arbiter arbiter = this.arbiterToGiveBack[this.index];
                Arbiter.Pool.GiveBack(arbiter);
            }
            this.arbiterToGiveBack.Clear();
            this.index = 0;
            for (this.length = this.collisionIslandToGiveBack.Count; this.index < this.length; this.index = this.index + 1)
            {
                CollisionIsland collisionIsland = this.collisionIslandToGiveBack[this.index];
                IslandManager.Pool.GiveBack(collisionIsland);
            }
            this.collisionIslandToGiveBack.Clear();
        }
    }
}
