// Decompiled with JetBrains decompiler
// Type: TrueSync.WorldClone2D
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

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
        internal Dictionary<int, BodyClone2D> clonedPhysics = new Dictionary<int, BodyClone2D>();
        internal List<ContactClone2D> contactsClone = new List<ContactClone2D>();
        internal IslandClone2D islandClone = new IslandClone2D();
        internal TOIClone2D toiClone = new TOIClone2D();
        internal DynamicTreeBroadPhaseClone2D dynamicTreeClone = new DynamicTreeBroadPhaseClone2D();
        private List<Body> bodiesToRemove = new List<Body>();
        private Dictionary<string, TrueSync.Physics2D.Contact> contactDic = new Dictionary<string, TrueSync.Physics2D.Contact>();
        private Dictionary<string, ContactEdge> contactEdgeDic = new Dictionary<string, ContactEdge>();
        internal int bodyCounter;
        internal int fixtureCounter;
        internal bool _worldHasNewFixture;
        private int index;
        private int length;

        public string checksum { get; private set; }

        public void Reset()
        {
            foreach (BodyClone2D bodyClone2D in this.clonedPhysics.Values)
            {
                bodyClone2D.Reset();
                WorldClone2D.poolRigidBodyClone.GiveBack(bodyClone2D);
            }
            this.index = 0;
            for (this.length = this.contactsClone.Count; this.index < this.length; this.index = this.index + 1)
            {
                ContactClone2D contactClone2D = this.contactsClone[this.index];
                WorldClone2D.poolContactClone.GiveBack(contactClone2D);
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
                this.checksum = ChecksumExtractor.GetEncodedChecksum();
            this.clonedPhysics.Clear();
            this.index = 0;
            for (this.length = world.BodyList.Count; this.index < this.length; this.index = this.index + 1)
            {
                Body body = world.BodyList[this.index];
                BodyClone2D bodyClone2D = WorldClone2D.poolRigidBodyClone.GetNew();
                bodyClone2D.Clone(body);
                this.clonedPhysics.Add(body.BodyId, bodyClone2D);
            }
            this.contactsClone.Clear();
            this.index = 0;
            for (this.length = world.ContactList.Count; this.index < this.length; this.index = this.index + 1)
            {
                TrueSync.Physics2D.Contact contact = world.ContactList[this.index];
                ContactClone2D contactClone2D = WorldClone2D.poolContactClone.GetNew();
                contactClone2D.Clone(contact);
                this.contactsClone.Add(contactClone2D);
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
            for (this.length = world.BodyList.Count; this.index < this.length; this.index = this.index + 1)
            {
                Body body = world.BodyList[this.index];
                if (!this.clonedPhysics.ContainsKey(body.BodyId))
                    this.bodiesToRemove.Add(body);
            }
            this.index = 0;
            for (this.length = this.bodiesToRemove.Count; this.index < this.length; this.index = this.index + 1)
            {
                Body body = this.bodiesToRemove[this.index];
                world.RemoveBody(body);
            }
            world.ProcessRemovedBodies();
            this.index = 0;
            for (this.length = world.BodyList.Count; this.index < this.length; this.index = this.index + 1)
            {
                Body body = world.BodyList[this.index];
                if (this.clonedPhysics.ContainsKey(body.BodyId))
                    this.clonedPhysics[body.BodyId].Restore(body);
            }
            this.index = 0;
            for (this.length = world.ContactList.Count; this.index < this.length; this.index = this.index + 1)
            {
                TrueSync.Physics2D.Contact contact = world.ContactList[this.index];
                world._contactPool.Enqueue(contact);
            }
            world.ContactList.Clear();
            this.contactDic.Clear();
            this.index = 0;
            for (this.length = this.contactsClone.Count; this.index < this.length; this.index = this.index + 1)
            {
                ContactClone2D contactClone2D = this.contactsClone[this.index];
                TrueSync.Physics2D.Contact contact = world._contactPool.Count <= 0 ? new TrueSync.Physics2D.Contact() : world._contactPool.Dequeue();
                contactClone2D.Restore(contact);
                this.contactDic.Add(contact.Key, contact);
                world.ContactList.Add(contact);
            }
            this.contactEdgeDic.Clear();
            this.index = 0;
            for (this.length = this.contactsClone.Count; this.index < this.length; this.index = this.index + 1)
            {
                ContactClone2D contactClone2D = this.contactsClone[this.index];
                this.contactDic[contactClone2D.Key]._nodeA = contactClone2D._nodeA.Restore(false, this.contactDic, this.contactEdgeDic);
                this.contactDic[contactClone2D.Key]._nodeB = contactClone2D._nodeB.Restore(false, this.contactDic, this.contactEdgeDic);
            }
            this.index = 0;
            for (this.length = this.contactsClone.Count; this.index < this.length; this.index = this.index + 1)
            {
                ContactClone2D contactClone2D = this.contactsClone[this.index];
                this.contactDic[contactClone2D.Key]._nodeA = contactClone2D._nodeA.Restore(true, this.contactDic, this.contactEdgeDic);
                this.contactDic[contactClone2D.Key]._nodeB = contactClone2D._nodeB.Restore(true, this.contactDic, this.contactEdgeDic);
            }
            this.index = 0;
            for (this.length = world.BodyList.Count; this.index < this.length; this.index = this.index + 1)
            {
                Body body = world.BodyList[this.index];
                if (this.clonedPhysics.ContainsKey(body.BodyId))
                {
                    BodyClone2D clonedPhysic = this.clonedPhysics[body.BodyId];
                    if (clonedPhysic.contactEdgeClone != null)
                        clonedPhysic.contactEdgeClone.Restore(false, this.contactDic, this.contactEdgeDic);
                    else
                        body.ContactList = (ContactEdge)null;
                }
            }
            this.index = 0;
            for (this.length = world.BodyList.Count; this.index < this.length; this.index = this.index + 1)
            {
                Body body = world.BodyList[this.index];
                if (this.clonedPhysics.ContainsKey(body.BodyId))
                {
                    BodyClone2D clonedPhysic = this.clonedPhysics[body.BodyId];
                    if (clonedPhysic.contactEdgeClone != null)
                        body.ContactList = clonedPhysic.contactEdgeClone.Restore(true, this.contactDic, this.contactEdgeDic);
                }
            }
            this.islandClone.Restore(world.Island, this.contactDic);
            this.toiClone.Restore(world._input);
            TreeNode<FixtureProxy>[] nodes = ((DynamicTreeBroadPhase)world.ContactManager.BroadPhase)._tree._nodes;
            this.index = 0;
            for (this.length = nodes.Length; this.index < this.length; this.index = this.index + 1)
            {
                TreeNode<FixtureProxy> treeNode = nodes[this.index];
                WorldClone2D.poolTreeFixtureProxy.GiveBack(treeNode);
            }
            this.dynamicTreeClone.Restore((DynamicTreeBroadPhase)world.ContactManager.BroadPhase);
            world._worldHasNewFixture = this._worldHasNewFixture;
            Body._bodyIdCounter = this.bodyCounter;
            Fixture._fixtureIdCounter = this.fixtureCounter;
        }
    }
}
