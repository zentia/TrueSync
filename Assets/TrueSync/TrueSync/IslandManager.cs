namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class IslandManager : List<CollisionIsland>
    {
        public List<CollisionIsland> islands;
        private Queue<RigidBody> leftSearchQueue;
        public static ResourcePool<CollisionIsland> Pool = new ResourcePool<CollisionIsland>();
        private Queue<RigidBody> rightSearchQueue;
        public Stack<Arbiter> rmStackArb;
        public Stack<Constraint> rmStackCstr;
        public List<RigidBody> visitedBodiesLeft;
        public List<RigidBody> visitedBodiesRight;

        public IslandManager() : base(new List<CollisionIsland>())
        {
            this.rmStackArb = new Stack<Arbiter>();
            this.rmStackCstr = new Stack<Constraint>();
            this.leftSearchQueue = new Queue<RigidBody>();
            this.rightSearchQueue = new Queue<RigidBody>();
            this.visitedBodiesLeft = new List<RigidBody>();
            this.visitedBodiesRight = new List<RigidBody>();
            this.islands = this;
        }

        private void AddConnection(RigidBody body1, RigidBody body2)
        {
            Debug.Assert(((body1 != null) && !body1.isStatic) || ((body2 != null) && !body2.isStatic), "IslandManager Inconsistency.", "Arbiter detected between two static objects.");
            if ((body1 == null) || body1.IsStaticNonKinematic)
            {
                if ((body2 != null) && (body2.island == null))
                {
                    CollisionIsland item = Pool.GetNew();
                    item.islandManager = this;
                    body2.island = item;
                    body2.island.bodies.Add(body2);
                    this.islands.Add(item);
                }
            }
            else if ((body2 == null) || body2.IsStaticNonKinematic)
            {
                if ((body1 != null) && (body1.island == null))
                {
                    CollisionIsland island2 = Pool.GetNew();
                    island2.islandManager = this;
                    body1.island = island2;
                    body1.island.bodies.Add(body1);
                    this.islands.Add(island2);
                }
            }
            else if ((body1 != null) && (body2 > null))
            {
                this.MergeIslands(body1, body2);
                body1.connections.Add(body2);
                body2.connections.Add(body1);
            }
        }

        public void ArbiterCreated(Arbiter arbiter)
        {
            this.AddConnection(arbiter.body1, arbiter.body2);
            arbiter.body1.arbiters.Add(arbiter);
            arbiter.body2.arbiters.Add(arbiter);
            if (arbiter.body1.island > null)
            {
                arbiter.body1.island.arbiter.Add(arbiter);
            }
            else if (arbiter.body2.island > null)
            {
                arbiter.body2.island.arbiter.Add(arbiter);
            }
        }

        public void ArbiterRemoved(Arbiter arbiter)
        {
            arbiter.body1.arbiters.Remove(arbiter);
            arbiter.body2.arbiters.Remove(arbiter);
            if (arbiter.body1.island > null)
            {
                arbiter.body1.island.arbiter.Remove(arbiter);
            }
            else if (arbiter.body2.island > null)
            {
                arbiter.body2.island.arbiter.Remove(arbiter);
            }
            this.RemoveConnection(arbiter.body1, arbiter.body2);
        }

        public void ConstraintCreated(Constraint constraint)
        {
            this.AddConnection(constraint.body1, constraint.body2);
            constraint.body1.constraints.Add(constraint);
            if (constraint.body2 > null)
            {
                constraint.body2.constraints.Add(constraint);
            }
            if (constraint.body1.island > null)
            {
                constraint.body1.island.constraints.Add(constraint);
            }
            else if ((constraint.body2 != null) && (constraint.body2.island > null))
            {
                constraint.body2.island.constraints.Add(constraint);
            }
        }

        public void ConstraintRemoved(Constraint constraint)
        {
            constraint.body1.constraints.Remove(constraint);
            if (constraint.body2 > null)
            {
                constraint.body2.constraints.Remove(constraint);
            }
            if (constraint.body1.island > null)
            {
                constraint.body1.island.constraints.Remove(constraint);
            }
            else if ((constraint.body2 != null) && (constraint.body2.island > null))
            {
                constraint.body2.island.constraints.Remove(constraint);
            }
            this.RemoveConnection(constraint.body1, constraint.body2);
        }

        public void MakeBodyStatic(RigidBody body)
        {
            body.connections.Clear();
            if (body.island > null)
            {
                body.island.bodies.Remove(body);
                if (body.island.bodies.Count == 0)
                {
                    body.island.ClearLists();
                    this.islands.Remove(body.island);
                    Pool.GiveBack(body.island);
                }
            }
            body.island = null;
        }

        private void MergeIslands(RigidBody body0, RigidBody body1)
        {
            if (body0.island != body1.island)
            {
                if (body0.island == null)
                {
                    body0.island = body1.island;
                    body0.island.bodies.Add(body0);
                }
                else if (body1.island == null)
                {
                    body1.island = body0.island;
                    body1.island.bodies.Add(body1);
                }
                else
                {
                    RigidBody body;
                    RigidBody body2;
                    if (body0.island.bodies.Count > body1.island.bodies.Count)
                    {
                        body = body1;
                        body2 = body0;
                    }
                    else
                    {
                        body = body0;
                        body2 = body1;
                    }
                    CollisionIsland island = body.island;
                    Pool.GiveBack(island);
                    this.islands.Remove(island);
                    foreach (RigidBody body3 in island.bodies)
                    {
                        body3.island = body2.island;
                        body2.island.bodies.Add(body3);
                    }
                    foreach (Arbiter arbiter in island.arbiter)
                    {
                        body2.island.arbiter.Add(arbiter);
                    }
                    foreach (Constraint constraint in island.constraints)
                    {
                        body2.island.constraints.Add(constraint);
                    }
                    island.ClearLists();
                }
            }
            else if (body0.island == null)
            {
                CollisionIsland item = Pool.GetNew();
                item.islandManager = this;
                body0.island = body1.island = item;
                body0.island.bodies.Add(body0);
                body0.island.bodies.Add(body1);
                this.islands.Add(item);
            }
        }

        public void RemoveAll()
        {
            foreach (CollisionIsland island in this.islands)
            {
                foreach (RigidBody body in island.bodies)
                {
                    body.arbiters.Clear();
                    body.constraints.Clear();
                    body.connections.Clear();
                    body.island = null;
                }
                island.ClearLists();
            }
            this.islands.Clear();
        }

        public void RemoveBody(RigidBody body)
        {
            foreach (Arbiter arbiter in body.arbiters)
            {
                this.rmStackArb.Push(arbiter);
            }
            while (this.rmStackArb.Count > 0)
            {
                this.ArbiterRemoved(this.rmStackArb.Pop());
            }
            foreach (Constraint constraint in body.constraints)
            {
                this.rmStackCstr.Push(constraint);
            }
            while (this.rmStackCstr.Count > 0)
            {
                this.ConstraintRemoved(this.rmStackCstr.Pop());
            }
            body.arbiters.Clear();
            body.constraints.Clear();
            if (body.island > null)
            {
                Debug.Assert(body.island.islandManager == this, "IslandManager Inconsistency.", "IslandManager doesn't own the Island.");
                Debug.Assert(body.island.bodies.Count == 1, "IslandManager Inconsistency.", "Removed all connections of a body - body is still in a non single Island.");
                body.island.ClearLists();
                Pool.GiveBack(body.island);
                this.islands.Remove(body.island);
                body.island = null;
            }
        }

        private void RemoveConnection(RigidBody body1, RigidBody body2)
        {
            Debug.Assert(!body1.isStatic || !body2.isStatic, "IslandManager Inconsistency.", "Arbiter detected between two static objects.");
            if (body1.IsStaticNonKinematic)
            {
                body2.connections.Remove(body1);
            }
            else if ((body2 == null) || body2.IsStaticNonKinematic)
            {
                body1.connections.Remove(body2);
            }
            else
            {
                Debug.Assert(body1.island == body2.island, "IslandManager Inconsistency.", "Removing arbiter with different islands.");
                body1.connections.Remove(body2);
                body2.connections.Remove(body1);
                this.SplitIslands(body1, body2);
            }
        }

        private void SplitIslands(RigidBody body0, RigidBody body1)
        {
            int num5;
            Debug.Assert((body0.island != null) && (body0.island == body1.island), "Islands not the same or null.");
            this.leftSearchQueue.Enqueue(body0);
            this.rightSearchQueue.Enqueue(body1);
            this.visitedBodiesLeft.Add(body0);
            this.visitedBodiesRight.Add(body1);
            body0.marker = 1;
            body1.marker = 2;
            while ((this.leftSearchQueue.Count > 0) && (this.rightSearchQueue.Count > 0))
            {
                RigidBody body = this.leftSearchQueue.Dequeue();
                if (!body.IsStaticNonKinematic)
                {
                    for (int j = 0; j < body.connections.Count; j++)
                    {
                        RigidBody body2 = body.connections[j];
                        if (body2.marker == 0)
                        {
                            this.leftSearchQueue.Enqueue(body2);
                            this.visitedBodiesLeft.Add(body2);
                            body2.marker = 1;
                        }
                        else if (body2.marker == 2)
                        {
                            this.leftSearchQueue.Clear();
                            this.rightSearchQueue.Clear();
                            goto Label_0485;
                        }
                    }
                }
                body = this.rightSearchQueue.Dequeue();
                if (!body.IsStaticNonKinematic)
                {
                    for (int k = 0; k < body.connections.Count; k++)
                    {
                        RigidBody body3 = body.connections[k];
                        if (body3.marker == 0)
                        {
                            this.rightSearchQueue.Enqueue(body3);
                            this.visitedBodiesRight.Add(body3);
                            body3.marker = 2;
                        }
                        else if (body3.marker == 1)
                        {
                            this.leftSearchQueue.Clear();
                            this.rightSearchQueue.Clear();
                            goto Label_0485;
                        }
                    }
                }
            }
            CollisionIsland item = Pool.GetNew();
            item.islandManager = this;
            this.islands.Add(item);
            if (this.leftSearchQueue.Count == 0)
            {
                for (int m = 0; m < this.visitedBodiesLeft.Count; m++)
                {
                    RigidBody body4 = this.visitedBodiesLeft[m];
                    body1.island.bodies.Remove(body4);
                    item.bodies.Add(body4);
                    body4.island = item;
                    foreach (Arbiter arbiter in body4.arbiters)
                    {
                        body1.island.arbiter.Remove(arbiter);
                        item.arbiter.Add(arbiter);
                    }
                    foreach (Constraint constraint in body4.constraints)
                    {
                        body1.island.constraints.Remove(constraint);
                        item.constraints.Add(constraint);
                    }
                }
                this.rightSearchQueue.Clear();
            }
            else if (this.rightSearchQueue.Count == 0)
            {
                for (int n = 0; n < this.visitedBodiesRight.Count; n++)
                {
                    RigidBody body5 = this.visitedBodiesRight[n];
                    body0.island.bodies.Remove(body5);
                    item.bodies.Add(body5);
                    body5.island = item;
                    foreach (Arbiter arbiter2 in body5.arbiters)
                    {
                        body0.island.arbiter.Remove(arbiter2);
                        item.arbiter.Add(arbiter2);
                    }
                    foreach (Constraint constraint2 in body5.constraints)
                    {
                        body0.island.constraints.Remove(constraint2);
                        item.constraints.Add(constraint2);
                    }
                }
                this.leftSearchQueue.Clear();
            }
        Label_0485:
            num5 = 0;
            while (num5 < this.visitedBodiesLeft.Count)
            {
                this.visitedBodiesLeft[num5].marker = 0;
                num5++;
            }
            for (int i = 0; i < this.visitedBodiesRight.Count; i++)
            {
                this.visitedBodiesRight[i].marker = 0;
            }
            this.visitedBodiesLeft.Clear();
            this.visitedBodiesRight.Clear();
        }
    }
}

