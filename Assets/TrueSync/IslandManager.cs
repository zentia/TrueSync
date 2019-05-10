using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync
{
	public class IslandManager : List<CollisionIsland>
	{
		public static ResourcePool<CollisionIsland> Pool = new ResourcePool<CollisionIsland>();

		public List<CollisionIsland> islands;

		public Stack<Arbiter> rmStackArb = new Stack<Arbiter>();

		public Stack<Constraint> rmStackCstr = new Stack<Constraint>();

		private Queue<RigidBody> leftSearchQueue = new Queue<RigidBody>();

		private Queue<RigidBody> rightSearchQueue = new Queue<RigidBody>();

		public List<RigidBody> visitedBodiesLeft = new List<RigidBody>();

		public List<RigidBody> visitedBodiesRight = new List<RigidBody>();

		public IslandManager() : base(new List<CollisionIsland>())
		{
			this.islands = this;
		}

		public void ArbiterCreated(Arbiter arbiter)
		{
			this.AddConnection(arbiter.body1, arbiter.body2);
			arbiter.body1.arbiters.Add(arbiter);
			arbiter.body2.arbiters.Add(arbiter);
			bool flag = arbiter.body1.island != null;
			if (flag)
			{
				arbiter.body1.island.arbiter.Add(arbiter);
			}
			else
			{
				bool flag2 = arbiter.body2.island != null;
				if (flag2)
				{
					arbiter.body2.island.arbiter.Add(arbiter);
				}
			}
		}

		public void ArbiterRemoved(Arbiter arbiter)
		{
			arbiter.body1.arbiters.Remove(arbiter);
			arbiter.body2.arbiters.Remove(arbiter);
			bool flag = arbiter.body1.island != null;
			if (flag)
			{
				arbiter.body1.island.arbiter.Remove(arbiter);
			}
			else
			{
				bool flag2 = arbiter.body2.island != null;
				if (flag2)
				{
					arbiter.body2.island.arbiter.Remove(arbiter);
				}
			}
			this.RemoveConnection(arbiter.body1, arbiter.body2);
		}

		public void ConstraintCreated(Constraint constraint)
		{
			this.AddConnection(constraint.body1, constraint.body2);
			constraint.body1.constraints.Add(constraint);
			bool flag = constraint.body2 != null;
			if (flag)
			{
				constraint.body2.constraints.Add(constraint);
			}
			bool flag2 = constraint.body1.island != null;
			if (flag2)
			{
				constraint.body1.island.constraints.Add(constraint);
			}
			else
			{
				bool flag3 = constraint.body2 != null && constraint.body2.island != null;
				if (flag3)
				{
					constraint.body2.island.constraints.Add(constraint);
				}
			}
		}

		public void ConstraintRemoved(Constraint constraint)
		{
			constraint.body1.constraints.Remove(constraint);
			bool flag = constraint.body2 != null;
			if (flag)
			{
				constraint.body2.constraints.Remove(constraint);
			}
			bool flag2 = constraint.body1.island != null;
			if (flag2)
			{
				constraint.body1.island.constraints.Remove(constraint);
			}
			else
			{
				bool flag3 = constraint.body2 != null && constraint.body2.island != null;
				if (flag3)
				{
					constraint.body2.island.constraints.Remove(constraint);
				}
			}
			this.RemoveConnection(constraint.body1, constraint.body2);
		}

		public void MakeBodyStatic(RigidBody body)
		{
			body.connections.Clear();
			bool flag = body.island != null;
			if (flag)
			{
				body.island.bodies.Remove(body);
				bool flag2 = body.island.bodies.Count == 0;
				if (flag2)
				{
					body.island.ClearLists();
					this.islands.Remove(body.island);
					IslandManager.Pool.GiveBack(body.island);
				}
			}
			body.island = null;
		}

		public void RemoveBody(RigidBody body)
		{
			foreach (Arbiter current in body.arbiters)
			{
				this.rmStackArb.Push(current);
			}
			while (this.rmStackArb.Count > 0)
			{
				this.ArbiterRemoved(this.rmStackArb.Pop());
			}
			foreach (Constraint current2 in body.constraints)
			{
				this.rmStackCstr.Push(current2);
			}
			while (this.rmStackCstr.Count > 0)
			{
				this.ConstraintRemoved(this.rmStackCstr.Pop());
			}
			body.arbiters.Clear();
			body.constraints.Clear();
			bool flag = body.island != null;
			if (flag)
			{
				Debug.Assert(body.island.islandManager == this, "IslandManager Inconsistency.", "IslandManager doesn't own the Island.");
				Debug.Assert(body.island.bodies.Count == 1, "IslandManager Inconsistency.", "Removed all connections of a body - body is still in a non single Island.");
				body.island.ClearLists();
				IslandManager.Pool.GiveBack(body.island);
				this.islands.Remove(body.island);
				body.island = null;
			}
		}

		public void RemoveAll()
		{
			foreach (CollisionIsland current in this.islands)
			{
				foreach (RigidBody current2 in current.bodies)
				{
					current2.arbiters.Clear();
					current2.constraints.Clear();
					current2.connections.Clear();
					current2.island = null;
				}
				current.ClearLists();
			}
			this.islands.Clear();
		}

		private void AddConnection(RigidBody body1, RigidBody body2)
		{
			Debug.Assert((body1 != null && !body1.isStatic) || (body2 != null && !body2.isStatic), "IslandManager Inconsistency.", "Arbiter detected between two static objects.");
			bool flag = body1 == null || body1.IsStaticNonKinematic;
			if (flag)
			{
				bool flag2 = body2 != null && body2.island == null;
				if (flag2)
				{
					CollisionIsland @new = IslandManager.Pool.GetNew();
					@new.islandManager = this;
					body2.island = @new;
					body2.island.bodies.Add(body2);
					this.islands.Add(@new);
				}
			}
			else
			{
				bool flag3 = body2 == null || body2.IsStaticNonKinematic;
				if (flag3)
				{
					bool flag4 = body1 != null && body1.island == null;
					if (flag4)
					{
						CollisionIsland new2 = IslandManager.Pool.GetNew();
						new2.islandManager = this;
						body1.island = new2;
						body1.island.bodies.Add(body1);
						this.islands.Add(new2);
					}
				}
				else
				{
					bool flag5 = body1 != null && body2 != null;
					if (flag5)
					{
						this.MergeIslands(body1, body2);
						body1.connections.Add(body2);
						body2.connections.Add(body1);
					}
				}
			}
		}

		private void RemoveConnection(RigidBody body1, RigidBody body2)
		{
			Debug.Assert(!body1.isStatic || !body2.isStatic, "IslandManager Inconsistency.", "Arbiter detected between two static objects.");
			bool isStaticNonKinematic = body1.IsStaticNonKinematic;
			if (isStaticNonKinematic)
			{
				body2.connections.Remove(body1);
			}
			else
			{
				bool flag = body2 == null || body2.IsStaticNonKinematic;
				if (flag)
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
		}

		private void SplitIslands(RigidBody body0, RigidBody body1)
		{
			Debug.Assert(body0.island != null && body0.island == body1.island, "Islands not the same or null.");
			this.leftSearchQueue.Enqueue(body0);
			this.rightSearchQueue.Enqueue(body1);
			this.visitedBodiesLeft.Add(body0);
			this.visitedBodiesRight.Add(body1);
			body0.marker = 1;
			body1.marker = 2;
			while (this.leftSearchQueue.Count > 0 && this.rightSearchQueue.Count > 0)
			{
				RigidBody rigidBody = this.leftSearchQueue.Dequeue();
				bool flag = !rigidBody.IsStaticNonKinematic;
				if (flag)
				{
					for (int i = 0; i < rigidBody.connections.Count; i++)
					{
						RigidBody rigidBody2 = rigidBody.connections[i];
						bool flag2 = rigidBody2.marker == 0;
						if (flag2)
						{
							this.leftSearchQueue.Enqueue(rigidBody2);
							this.visitedBodiesLeft.Add(rigidBody2);
							rigidBody2.marker = 1;
						}
						else
						{
							bool flag3 = rigidBody2.marker == 2;
							if (flag3)
							{
								this.leftSearchQueue.Clear();
								this.rightSearchQueue.Clear();
								goto IL_485;
							}
						}
					}
				}
				rigidBody = this.rightSearchQueue.Dequeue();
				bool flag4 = !rigidBody.IsStaticNonKinematic;
				if (flag4)
				{
					for (int j = 0; j < rigidBody.connections.Count; j++)
					{
						RigidBody rigidBody3 = rigidBody.connections[j];
						bool flag5 = rigidBody3.marker == 0;
						if (flag5)
						{
							this.rightSearchQueue.Enqueue(rigidBody3);
							this.visitedBodiesRight.Add(rigidBody3);
							rigidBody3.marker = 2;
						}
						else
						{
							bool flag6 = rigidBody3.marker == 1;
							if (flag6)
							{
								this.leftSearchQueue.Clear();
								this.rightSearchQueue.Clear();
								goto IL_485;
							}
						}
					}
				}
				continue;
				IL_485:
				for (int k = 0; k < this.visitedBodiesLeft.Count; k++)
				{
					this.visitedBodiesLeft[k].marker = 0;
				}
				for (int l = 0; l < this.visitedBodiesRight.Count; l++)
				{
					this.visitedBodiesRight[l].marker = 0;
				}
				this.visitedBodiesLeft.Clear();
				this.visitedBodiesRight.Clear();
				return;
			}
			CollisionIsland @new = IslandManager.Pool.GetNew();
			@new.islandManager = this;
			this.islands.Add(@new);
			bool flag7 = this.leftSearchQueue.Count == 0;
			if (flag7)
			{
				for (int m = 0; m < this.visitedBodiesLeft.Count; m++)
				{
					RigidBody rigidBody4 = this.visitedBodiesLeft[m];
					body1.island.bodies.Remove(rigidBody4);
					@new.bodies.Add(rigidBody4);
					rigidBody4.island = @new;
					foreach (Arbiter current in rigidBody4.arbiters)
					{
						body1.island.arbiter.Remove(current);
						@new.arbiter.Add(current);
					}
					foreach (Constraint current2 in rigidBody4.constraints)
					{
						body1.island.constraints.Remove(current2);
						@new.constraints.Add(current2);
					}
				}
				this.rightSearchQueue.Clear();
				goto IL_485;
			}
			bool flag8 = this.rightSearchQueue.Count == 0;
			if (flag8)
			{
				for (int n = 0; n < this.visitedBodiesRight.Count; n++)
				{
					RigidBody rigidBody5 = this.visitedBodiesRight[n];
					body0.island.bodies.Remove(rigidBody5);
					@new.bodies.Add(rigidBody5);
					rigidBody5.island = @new;
					foreach (Arbiter current3 in rigidBody5.arbiters)
					{
						body0.island.arbiter.Remove(current3);
						@new.arbiter.Add(current3);
					}
					foreach (Constraint current4 in rigidBody5.constraints)
					{
						body0.island.constraints.Remove(current4);
						@new.constraints.Add(current4);
					}
				}
				this.leftSearchQueue.Clear();
				goto IL_485;
			}
			goto IL_485;
		}

		private void MergeIslands(RigidBody body0, RigidBody body1)
		{
			bool flag = body0.island != body1.island;
			if (flag)
			{
				bool flag2 = body0.island == null;
				if (flag2)
				{
					body0.island = body1.island;
					body0.island.bodies.Add(body0);
				}
				else
				{
					bool flag3 = body1.island == null;
					if (flag3)
					{
						body1.island = body0.island;
						body1.island.bodies.Add(body1);
					}
					else
					{
						bool flag4 = body0.island.bodies.Count > body1.island.bodies.Count;
						RigidBody rigidBody;
						RigidBody rigidBody2;
						if (flag4)
						{
							rigidBody = body1;
							rigidBody2 = body0;
						}
						else
						{
							rigidBody = body0;
							rigidBody2 = body1;
						}
						CollisionIsland island = rigidBody.island;
						IslandManager.Pool.GiveBack(island);
						this.islands.Remove(island);
						foreach (RigidBody current in island.bodies)
						{
							current.island = rigidBody2.island;
							rigidBody2.island.bodies.Add(current);
						}
						foreach (Arbiter current2 in island.arbiter)
						{
							rigidBody2.island.arbiter.Add(current2);
						}
						foreach (Constraint current3 in island.constraints)
						{
							rigidBody2.island.constraints.Add(current3);
						}
						island.ClearLists();
					}
				}
			}
			else
			{
				bool flag5 = body0.island == null;
				if (flag5)
				{
					CollisionIsland @new = IslandManager.Pool.GetNew();
					@new.islandManager = this;
					body0.island = (body1.island = @new);
					body0.island.bodies.Add(body0);
					body0.island.bodies.Add(body1);
					this.islands.Add(@new);
				}
			}
		}
	}
}
