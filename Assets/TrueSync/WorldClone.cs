using System;
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

		public int rigidBodyInstanceCount;

		public int constraintInstanceCount;

		public List<Contact> contactsToGiveBack = new List<Contact>();

		public List<Arbiter> arbiterToGiveBack = new List<Arbiter>();

		public List<CollisionIsland> collisionIslandToGiveBack = new List<CollisionIsland>();

		private int index;

		private int length;

		public string checksum
		{
			get;
			private set;
		}

		public void Reset()
		{
			bool flag = this.clonedPhysics != null;
			if (flag)
			{
				foreach (RigidBodyClone current in this.clonedPhysics.Values)
				{
					current.Reset();
					WorldClone.poolRigidBodyClone.GiveBack(current);
				}
			}
			bool flag2 = this.collisionIslands != null;
			if (flag2)
			{
				this.index = 0;
				this.length = this.collisionIslands.Count;
				while (this.index < this.length)
				{
					CollisionIslandClone collisionIslandClone = this.collisionIslands[this.index];
					collisionIslandClone.Reset();
					WorldClone.poolCollisionIslandClone.GiveBack(collisionIslandClone);
					this.index++;
				}
			}
			bool flag3 = this.cloneCollision != null;
			if (flag3)
			{
				this.cloneCollision.Reset();
			}
			bool flag4 = this.clonedArbiters != null;
			if (flag4)
			{
				this.index = 0;
				this.length = this.clonedArbiters.Count;
				while (this.index < this.length)
				{
					ArbiterClone arbiterClone = this.clonedArbiters[this.index];
					arbiterClone.Reset();
					WorldClone.poolArbiterClone.GiveBack(arbiterClone);
					this.index++;
				}
			}
			bool flag5 = this.clonedArbitersTrigger != null;
			if (flag5)
			{
				this.index = 0;
				this.length = this.clonedArbitersTrigger.Count;
				while (this.index < this.length)
				{
					ArbiterClone arbiterClone2 = this.clonedArbitersTrigger[this.index];
					arbiterClone2.Reset();
					WorldClone.poolArbiterClone.GiveBack(arbiterClone2);
					this.index++;
				}
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
			{
				this.checksum = ChecksumExtractor.GetEncodedChecksum();
			}
			this.clonedPhysics.Clear();
			foreach (RigidBody rigidBody in world.RigidBodies)
			{
				RigidBodyClone @new = WorldClone.poolRigidBodyClone.GetNew();
				@new.Clone(rigidBody);
				this.clonedPhysics.Add(rigidBody.GetInstance(), @new);
			}
			this.clonedArbiters.Clear();
			foreach (Arbiter current in world.ArbiterMap.Arbiters)
			{
				ArbiterClone new2 = WorldClone.poolArbiterClone.GetNew();
				new2.Clone(current);
				this.clonedArbiters.Add(new2);
			}
			this.clonedArbitersTrigger.Clear();
			foreach (Arbiter current2 in world.ArbiterTriggerMap.Arbiters)
			{
				ArbiterClone new3 = WorldClone.poolArbiterClone.GetNew();
				new3.Clone(current2);
				this.clonedArbitersTrigger.Add(new3);
			}
			this.collisionIslands.Clear();
			this.index = 0;
			this.length = world.islands.Count;
			while (this.index < this.length)
			{
				CollisionIsland ci = world.islands[this.index];
				CollisionIslandClone new4 = WorldClone.poolCollisionIslandClone.GetNew();
				new4.Clone(ci);
				this.collisionIslands.Add(new4);
				this.index++;
			}
			this.cloneCollision.Clone((CollisionSystemPersistentSAP)world.CollisionSystem);
			this.clonedInitialCollisions.Clear();
			this.clonedInitialCollisions.AddRange(world.initialCollisions);
			this.clonedInitialTriggers.Clear();
			this.clonedInitialTriggers.AddRange(world.initialTriggers);
			this.rigidBodyInstanceCount = RigidBody.instanceCount;
			this.constraintInstanceCount = Constraint.instanceCount;
		}

		public void Restore(IWorld iWorld)
		{
			World world = (World)iWorld;
			List<RigidBody> list = new List<RigidBody>();
			foreach (RigidBody rigidBody in world.RigidBodies)
			{
				bool flag = !this.clonedPhysics.ContainsKey(rigidBody.GetInstance());
				if (flag)
				{
					list.Add(rigidBody);
				}
			}
			this.index = 0;
			this.length = list.Count;
			while (this.index < this.length)
			{
				RigidBody body = list[this.index];
				world.RemoveBody(body);
				this.index++;
			}
			foreach (RigidBody rigidBody2 in world.RigidBodies)
			{
				bool flag2 = this.clonedPhysics.ContainsKey(rigidBody2.GetInstance());
				if (flag2)
				{
					RigidBodyClone rigidBodyClone = this.clonedPhysics[rigidBody2.GetInstance()];
					rigidBodyClone.Restore(world, rigidBody2);
					rigidBody2.island = null;
					rigidBody2.arbiters.Clear();
					rigidBody2.arbitersTrigger.Clear();
				}
			}
			foreach (Arbiter current in world.ArbiterMap.Arbiters)
			{
				this.index = 0;
				this.length = current.contactList.Count;
				while (this.index < this.length)
				{
					Contact item = current.contactList[this.index];
					this.contactsToGiveBack.Add(item);
					this.index++;
				}
				this.arbiterToGiveBack.Add(current);
			}
			world.ArbiterMap.Clear();
			foreach (Arbiter current2 in world.ArbiterTriggerMap.Arbiters)
			{
				foreach (Contact current3 in current2.contactList)
				{
					this.contactsToGiveBack.Add(current3);
				}
				this.arbiterToGiveBack.Add(current2);
			}
			world.ArbiterTriggerMap.Clear();
			this.index = 0;
			this.length = world.islands.islands.Count;
			while (this.index < this.length)
			{
				CollisionIsland item2 = world.islands.islands[this.index];
				this.collisionIslandToGiveBack.Add(item2);
				this.index++;
			}
			this.index = 0;
			this.length = this.clonedArbiters.Count;
			while (this.index < this.length)
			{
				ArbiterClone arbiterClone = this.clonedArbiters[this.index];
				Arbiter @new = Arbiter.Pool.GetNew();
				arbiterClone.Restore(@new);
				@new.body1.arbiters.Add(@new);
				@new.body2.arbiters.Add(@new);
				world.ArbiterMap.Add(new ArbiterKey(@new.body1, @new.body2), @new);
				this.index++;
			}
			this.index = 0;
			this.length = this.clonedArbitersTrigger.Count;
			while (this.index < this.length)
			{
				ArbiterClone arbiterClone2 = this.clonedArbitersTrigger[this.index];
				Arbiter new2 = Arbiter.Pool.GetNew();
				arbiterClone2.Restore(new2);
				new2.body1.arbitersTrigger.Add(new2);
				new2.body2.arbitersTrigger.Add(new2);
				world.ArbiterTriggerMap.Add(new ArbiterKey(new2.body1, new2.body2), new2);
				this.index++;
			}
			world.islands.islands.Clear();
			this.index = 0;
			this.length = this.collisionIslands.Count;
			while (this.index < this.length)
			{
				CollisionIslandClone collisionIslandClone = this.collisionIslands[this.index];
				CollisionIsland new3 = IslandManager.Pool.GetNew();
				collisionIslandClone.Restore(new3, world);
				world.islands.islands.Add(new3);
				this.index++;
			}
			this.cloneCollision.Restore((CollisionSystemPersistentSAP)world.CollisionSystem);
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
				Contact obj = this.contactsToGiveBack[this.index];
				Contact.Pool.GiveBack(obj);
				this.index++;
			}
			this.contactsToGiveBack.Clear();
			this.index = 0;
			this.length = this.arbiterToGiveBack.Count;
			while (this.index < this.length)
			{
				Arbiter obj2 = this.arbiterToGiveBack[this.index];
				Arbiter.Pool.GiveBack(obj2);
				this.index++;
			}
			this.arbiterToGiveBack.Clear();
			this.index = 0;
			this.length = this.collisionIslandToGiveBack.Count;
			while (this.index < this.length)
			{
				CollisionIsland obj3 = this.collisionIslandToGiveBack[this.index];
				IslandManager.Pool.GiveBack(obj3);
				this.index++;
			}
			this.collisionIslandToGiveBack.Clear();
		}
	}
}
