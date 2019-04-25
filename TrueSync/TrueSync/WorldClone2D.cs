namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync.Physics2D;

    public class WorldClone2D : IWorldClone
    {
        internal bool _worldHasNewFixture;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string <checksum>k__BackingField;
        private List<Body> bodiesToRemove = new List<Body>();
        internal int bodyCounter;
        internal Dictionary<int, BodyClone2D> clonedPhysics = new Dictionary<int, BodyClone2D>();
        private Dictionary<string, TrueSync.Physics2D.Contact> contactDic = new Dictionary<string, TrueSync.Physics2D.Contact>();
        private Dictionary<string, ContactEdge> contactEdgeDic = new Dictionary<string, ContactEdge>();
        internal List<ContactClone2D> contactsClone = new List<ContactClone2D>();
        internal DynamicTreeBroadPhaseClone2D dynamicTreeClone = new DynamicTreeBroadPhaseClone2D();
        internal int fixtureCounter;
        private int index;
        internal IslandClone2D islandClone = new IslandClone2D();
        private int length;
        internal static ResourcePoolContactClone2D poolContactClone = new ResourcePoolContactClone2D();
        internal static ResourcePoolContactEdge2D poolContactEdge = new ResourcePoolContactEdge2D();
        internal static ResourcePoolContactEdgeClone2D poolContactEdgeClone = new ResourcePoolContactEdgeClone2D();
        internal static ResourcePoolBodyClone2D poolRigidBodyClone = new ResourcePoolBodyClone2D();
        internal static ResourcePoolTreeFixtureProxy2D poolTreeFixtureProxy = new ResourcePoolTreeFixtureProxy2D();
        internal TOIClone2D toiClone = new TOIClone2D();

        public void Clone(IWorld iWorld)
        {
            this.Clone(iWorld, false);
        }

        public void Clone(IWorld iWorld, bool doChecksum)
        {
            TrueSync.Physics2D.World world = (TrueSync.Physics2D.World) iWorld;
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
                BodyClone2D cloned = poolRigidBodyClone.GetNew();
                cloned.Clone(body);
                this.clonedPhysics.Add(body.BodyId, cloned);
                this.index++;
            }
            this.contactsClone.Clear();
            this.index = 0;
            this.length = world.ContactList.Count;
            while (this.index < this.length)
            {
                TrueSync.Physics2D.Contact contact = world.ContactList[this.index];
                ContactClone2D item = poolContactClone.GetNew();
                item.Clone(contact);
                this.contactsClone.Add(item);
                this.index++;
            }
            this.islandClone.Clone(world.Island);
            this.toiClone.Clone(world._input);
            this.dynamicTreeClone.Clone((DynamicTreeBroadPhase) world.ContactManager.BroadPhase);
            this._worldHasNewFixture = world._worldHasNewFixture;
            this.bodyCounter = Body._bodyIdCounter;
            this.fixtureCounter = Fixture._fixtureIdCounter;
        }

        public void Reset()
        {
            foreach (BodyClone2D cloned in this.clonedPhysics.Values)
            {
                cloned.Reset();
                poolRigidBodyClone.GiveBack(cloned);
            }
            this.index = 0;
            this.length = this.contactsClone.Count;
            while (this.index < this.length)
            {
                ContactClone2D cloned2 = this.contactsClone[this.index];
                poolContactClone.GiveBack(cloned2);
                this.index++;
            }
        }

        public void Restore(IWorld iWorld)
        {
            TrueSync.Physics2D.World world = (TrueSync.Physics2D.World) iWorld;
            this.bodiesToRemove.Clear();
            this.index = 0;
            this.length = world.BodyList.Count;
            while (this.index < this.length)
            {
                Body item = world.BodyList[this.index];
                if (!this.clonedPhysics.ContainsKey(item.BodyId))
                {
                    this.bodiesToRemove.Add(item);
                }
                this.index++;
            }
            this.index = 0;
            this.length = this.bodiesToRemove.Count;
            while (this.index < this.length)
            {
                Body body = this.bodiesToRemove[this.index];
                world.RemoveBody(body);
                this.index++;
            }
            world.ProcessRemovedBodies();
            this.index = 0;
            this.length = world.BodyList.Count;
            while (this.index < this.length)
            {
                Body body3 = world.BodyList[this.index];
                if (this.clonedPhysics.ContainsKey(body3.BodyId))
                {
                    this.clonedPhysics[body3.BodyId].Restore(body3);
                }
                this.index++;
            }
            this.index = 0;
            this.length = world.ContactList.Count;
            while (this.index < this.length)
            {
                TrueSync.Physics2D.Contact contact = world.ContactList[this.index];
                world._contactPool.Enqueue(contact);
                this.index++;
            }
            world.ContactList.Clear();
            this.contactDic.Clear();
            this.index = 0;
            this.length = this.contactsClone.Count;
            while (this.index < this.length)
            {
                ContactClone2D cloned2 = this.contactsClone[this.index];
                TrueSync.Physics2D.Contact contact2 = null;
                if (world._contactPool.Count > 0)
                {
                    contact2 = world._contactPool.Dequeue();
                }
                else
                {
                    contact2 = new TrueSync.Physics2D.Contact();
                }
                cloned2.Restore(contact2);
                this.contactDic.Add(contact2.Key, contact2);
                world.ContactList.Add(contact2);
                this.index++;
            }
            this.contactEdgeDic.Clear();
            this.index = 0;
            this.length = this.contactsClone.Count;
            while (this.index < this.length)
            {
                ContactClone2D cloned3 = this.contactsClone[this.index];
                this.contactDic[cloned3.Key]._nodeA = cloned3._nodeA.Restore(false, this.contactDic, this.contactEdgeDic);
                this.contactDic[cloned3.Key]._nodeB = cloned3._nodeB.Restore(false, this.contactDic, this.contactEdgeDic);
                this.index++;
            }
            this.index = 0;
            this.length = this.contactsClone.Count;
            while (this.index < this.length)
            {
                ContactClone2D cloned4 = this.contactsClone[this.index];
                this.contactDic[cloned4.Key]._nodeA = cloned4._nodeA.Restore(true, this.contactDic, this.contactEdgeDic);
                this.contactDic[cloned4.Key]._nodeB = cloned4._nodeB.Restore(true, this.contactDic, this.contactEdgeDic);
                this.index++;
            }
            this.index = 0;
            this.length = world.BodyList.Count;
            while (this.index < this.length)
            {
                Body body4 = world.BodyList[this.index];
                if (this.clonedPhysics.ContainsKey(body4.BodyId))
                {
                    BodyClone2D cloned5 = this.clonedPhysics[body4.BodyId];
                    if (cloned5.contactEdgeClone > null)
                    {
                        cloned5.contactEdgeClone.Restore(false, this.contactDic, this.contactEdgeDic);
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
                if (this.clonedPhysics.ContainsKey(body5.BodyId))
                {
                    BodyClone2D cloned6 = this.clonedPhysics[body5.BodyId];
                    if (cloned6.contactEdgeClone > null)
                    {
                        body5.ContactList = cloned6.contactEdgeClone.Restore(true, this.contactDic, this.contactEdgeDic);
                    }
                }
                this.index++;
            }
            this.islandClone.Restore(world.Island, this.contactDic);
            this.toiClone.Restore(world._input);
            TreeNode<FixtureProxy>[] nodeArray = ((DynamicTreeBroadPhase) world.ContactManager.BroadPhase)._tree._nodes;
            this.index = 0;
            this.length = nodeArray.Length;
            while (this.index < this.length)
            {
                TreeNode<FixtureProxy> node = nodeArray[this.index];
                poolTreeFixtureProxy.GiveBack(node);
                this.index++;
            }
            this.dynamicTreeClone.Restore((DynamicTreeBroadPhase) world.ContactManager.BroadPhase);
            world._worldHasNewFixture = this._worldHasNewFixture;
            Body._bodyIdCounter = this.bodyCounter;
            Fixture._fixtureIdCounter = this.fixtureCounter;
        }

        public string checksum { get; private set; }
    }
}

