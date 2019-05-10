using System;
using System.Collections.Generic;
using TrueSync.Physics2D;

namespace TrueSync
{
	public class WorldClone2D : IWorldClone
	{
		internal static ResourcePoolBodyClone2D poolRigidBodyClone = new ResourcePoolBodyClone2D();

		internal static ResourcePoolContactClone2D poolContactClone = new ResourcePoolContactClone2D();

		internal static ResourcePoolContactEdgeClone2D poolContactEdgeClone = new ResourcePoolContactEdgeClone2D();

		internal static ResourcePoolContactEdge2D poolContactEdge = new ResourcePoolContactEdge2D();

		internal static ResourcePoolTreeFixtureProxy2D poolTreeFixtureProxy = new ResourcePoolTreeFixtureProxy2D();

		internal int bodyCounter;

		internal int fixtureCounter;

		internal Dictionary<int, BodyClone2D> clonedPhysics = new Dictionary<int, BodyClone2D>();

		internal bool _worldHasNewFixture;

		internal List<ContactClone2D> contactsClone = new List<ContactClone2D>();

		internal IslandClone2D islandClone = new IslandClone2D();

		internal TOIClone2D toiClone = new TOIClone2D();

		internal DynamicTreeBroadPhaseClone2D dynamicTreeClone = new DynamicTreeBroadPhaseClone2D();

		private List<Body> bodiesToRemove = new List<Body>();

		private Dictionary<string, TrueSync.Physics2D.Contact> contactDic = new Dictionary<string, TrueSync.Physics2D.Contact>();

		private Dictionary<string, ContactEdge> contactEdgeDic = new Dictionary<string, ContactEdge>();

		private int index;

		private int length;

		public string checksum
		{
			get;
			private set;
		}

		public void Reset()
		{
			foreach (BodyClone2D current in this.clonedPhysics.Values)
			{
				current.Reset();
				WorldClone2D.poolRigidBodyClone.GiveBack(current);
			}
			this.index = 0;
			this.length = this.contactsClone.Count;
			while (this.index < this.length)
			{
				ContactClone2D obj = this.contactsClone[this.index];
				WorldClone2D.poolContactClone.GiveBack(obj);
				this.index++;
			}
		}

		public void Clone(IWorld iWorld)
		{
			this.Clone(iWorld, false);
		}

		public void Clone(IWorld iWorld, bool doChecksum)
		{
			TrueSync.Physics2D.World world = (TrueSync.Physics2D.World)iWorld;
			this.Reset();
			if (doChecksum)
			{
				this.checksum = ChecksumExtractor.GetEncodedChecksum();
			}
			this.clonedPhysics.Clear();
			this.index = 0;
			this.length = world.BodyList.Count;
			while (this.index < this.length)
			{
				Body body = world.BodyList[this.index];
				BodyClone2D @new = WorldClone2D.poolRigidBodyClone.GetNew();
				@new.Clone(body);
				this.clonedPhysics.Add(body.BodyId, @new);
				this.index++;
			}
			this.contactsClone.Clear();
			this.index = 0;
			this.length = world.ContactList.Count;
			while (this.index < this.length)
			{
				TrueSync.Physics2D.Contact contact = world.ContactList[this.index];
				ContactClone2D new2 = WorldClone2D.poolContactClone.GetNew();
				new2.Clone(contact);
				this.contactsClone.Add(new2);
				this.index++;
			}
			this.islandClone.Clone(world.Island);
			this.toiClone.Clone(world._input);
			this.dynamicTreeClone.Clone((DynamicTreeBroadPhase)world.ContactManager.BroadPhase);
			this._worldHasNewFixture = world._worldHasNewFixture;
			this.bodyCounter = Body._bodyIdCounter;
			this.fixtureCounter = Fixture._fixtureIdCounter;
		}

		public void Restore(IWorld iWorld)
		{
			TrueSync.Physics2D.World world = (TrueSync.Physics2D.World)iWorld;
			this.bodiesToRemove.Clear();
			this.index = 0;
			this.length = world.BodyList.Count;
			while (this.index < this.length)
			{
				Body body = world.BodyList[this.index];
				bool flag = !this.clonedPhysics.ContainsKey(body.BodyId);
				if (flag)
				{
					this.bodiesToRemove.Add(body);
				}
				this.index++;
			}
			this.index = 0;
			this.length = this.bodiesToRemove.Count;
			while (this.index < this.length)
			{
				Body body2 = this.bodiesToRemove[this.index];
				world.RemoveBody(body2);
				this.index++;
			}
			world.ProcessRemovedBodies();
			this.index = 0;
			this.length = world.BodyList.Count;
			while (this.index < this.length)
			{
				Body body3 = world.BodyList[this.index];
				bool flag2 = this.clonedPhysics.ContainsKey(body3.BodyId);
				if (flag2)
				{
					BodyClone2D bodyClone2D = this.clonedPhysics[body3.BodyId];
					bodyClone2D.Restore(body3);
				}
				this.index++;
			}
			this.index = 0;
			this.length = world.ContactList.Count;
			while (this.index < this.length)
			{
				TrueSync.Physics2D.Contact item = world.ContactList[this.index];
				world._contactPool.Enqueue(item);
				this.index++;
			}
			world.ContactList.Clear();
			this.contactDic.Clear();
			this.index = 0;
			this.length = this.contactsClone.Count;
			while (this.index < this.length)
			{
				ContactClone2D contactClone2D = this.contactsClone[this.index];
				bool flag3 = world._contactPool.Count > 0;
				TrueSync.Physics2D.Contact contact;
				if (flag3)
				{
					contact = world._contactPool.Dequeue();
				}
				else
				{
					contact = new TrueSync.Physics2D.Contact();
				}
				contactClone2D.Restore(contact);
				this.contactDic.Add(contact.Key, contact);
				world.ContactList.Add(contact);
				this.index++;
			}
			this.contactEdgeDic.Clear();
			this.index = 0;
			this.length = this.contactsClone.Count;
			while (this.index < this.length)
			{
				ContactClone2D contactClone2D2 = this.contactsClone[this.index];
				this.contactDic[contactClone2D2.Key]._nodeA = contactClone2D2._nodeA.Restore(false, this.contactDic, this.contactEdgeDic);
				this.contactDic[contactClone2D2.Key]._nodeB = contactClone2D2._nodeB.Restore(false, this.contactDic, this.contactEdgeDic);
				this.index++;
			}
			this.index = 0;
			this.length = this.contactsClone.Count;
			while (this.index < this.length)
			{
				ContactClone2D contactClone2D3 = this.contactsClone[this.index];
				this.contactDic[contactClone2D3.Key]._nodeA = contactClone2D3._nodeA.Restore(true, this.contactDic, this.contactEdgeDic);
				this.contactDic[contactClone2D3.Key]._nodeB = contactClone2D3._nodeB.Restore(true, this.contactDic, this.contactEdgeDic);
				this.index++;
			}
			this.index = 0;
			this.length = world.BodyList.Count;
			while (this.index < this.length)
			{
				Body body4 = world.BodyList[this.index];
				bool flag4 = this.clonedPhysics.ContainsKey(body4.BodyId);
				if (flag4)
				{
					BodyClone2D bodyClone2D2 = this.clonedPhysics[body4.BodyId];
					bool flag5 = bodyClone2D2.contactEdgeClone != null;
					if (flag5)
					{
						bodyClone2D2.contactEdgeClone.Restore(false, this.contactDic, this.contactEdgeDic);
					}
					else
					{
						body4.ContactList = null;
					}
				}
				this.index++;
			}
			this.index = 0;
			this.length = world.BodyList.Count;
			while (this.index < this.length)
			{
				Body body5 = world.BodyList[this.index];
				bool flag6 = this.clonedPhysics.ContainsKey(body5.BodyId);
				if (flag6)
				{
					BodyClone2D bodyClone2D3 = this.clonedPhysics[body5.BodyId];
					bool flag7 = bodyClone2D3.contactEdgeClone != null;
					if (flag7)
					{
						body5.ContactList = bodyClone2D3.contactEdgeClone.Restore(true, this.contactDic, this.contactEdgeDic);
					}
				}
				this.index++;
			}
			this.islandClone.Restore(world.Island, this.contactDic);
			this.toiClone.Restore(world._input);
			TreeNode<FixtureProxy>[] nodes = ((DynamicTreeBroadPhase)world.ContactManager.BroadPhase)._tree._nodes;
			this.index = 0;
			this.length = nodes.Length;
			while (this.index < this.length)
			{
				TreeNode<FixtureProxy> obj = nodes[this.index];
				WorldClone2D.poolTreeFixtureProxy.GiveBack(obj);
				this.index++;
			}
			this.dynamicTreeClone.Restore((DynamicTreeBroadPhase)world.ContactManager.BroadPhase);
			world._worldHasNewFixture = this._worldHasNewFixture;
			Body._bodyIdCounter = this.bodyCounter;
			Fixture._fixtureIdCounter = this.fixtureCounter;
		}
	}
}
