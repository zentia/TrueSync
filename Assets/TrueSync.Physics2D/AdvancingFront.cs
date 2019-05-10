using System;
using System.Text;

namespace TrueSync.Physics2D
{
	internal class AdvancingFront
	{
		public AdvancingFrontNode Head;

		protected AdvancingFrontNode Search;

		public AdvancingFrontNode Tail;

		public AdvancingFront(AdvancingFrontNode head, AdvancingFrontNode tail)
		{
			this.Head = head;
			this.Tail = tail;
			this.Search = head;
			this.AddNode(head);
			this.AddNode(tail);
		}

		public void AddNode(AdvancingFrontNode node)
		{
		}

		public void RemoveNode(AdvancingFrontNode node)
		{
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (AdvancingFrontNode advancingFrontNode = this.Head; advancingFrontNode != this.Tail; advancingFrontNode = advancingFrontNode.Next)
			{
				stringBuilder.Append(advancingFrontNode.Point.X).Append("->");
			}
			stringBuilder.Append(this.Tail.Point.X);
			return stringBuilder.ToString();
		}

		private AdvancingFrontNode FindSearchNode(FP x)
		{
			return this.Search;
		}

		public AdvancingFrontNode LocateNode(TriangulationPoint point)
		{
			return this.LocateNode(point.X);
		}

		private AdvancingFrontNode LocateNode(FP x)
		{
			AdvancingFrontNode advancingFrontNode = this.FindSearchNode(x);
			bool flag = x < advancingFrontNode.Value;
			AdvancingFrontNode result;
			if (flag)
			{
				while ((advancingFrontNode = advancingFrontNode.Prev) != null)
				{
					bool flag2 = x >= advancingFrontNode.Value;
					if (flag2)
					{
						this.Search = advancingFrontNode;
						result = advancingFrontNode;
						return result;
					}
				}
			}
			else
			{
				while ((advancingFrontNode = advancingFrontNode.Next) != null)
				{
					bool flag3 = x < advancingFrontNode.Value;
					if (flag3)
					{
						this.Search = advancingFrontNode.Prev;
						result = advancingFrontNode.Prev;
						return result;
					}
				}
			}
			result = null;
			return result;
		}

		public AdvancingFrontNode LocatePoint(TriangulationPoint point)
		{
			FP x = point.X;
			AdvancingFrontNode advancingFrontNode = this.FindSearchNode(x);
			FP x2 = advancingFrontNode.Point.X;
			bool flag = x == x2;
			if (flag)
			{
				bool flag2 = point != advancingFrontNode.Point;
				if (flag2)
				{
					bool flag3 = point == advancingFrontNode.Prev.Point;
					if (flag3)
					{
						advancingFrontNode = advancingFrontNode.Prev;
					}
					else
					{
						bool flag4 = point == advancingFrontNode.Next.Point;
						if (!flag4)
						{
							throw new Exception("Failed to find Node for given afront point");
						}
						advancingFrontNode = advancingFrontNode.Next;
					}
				}
			}
			else
			{
				bool flag5 = x < x2;
				if (flag5)
				{
					while ((advancingFrontNode = advancingFrontNode.Prev) != null)
					{
						bool flag6 = point == advancingFrontNode.Point;
						if (flag6)
						{
							break;
						}
					}
				}
				else
				{
					while ((advancingFrontNode = advancingFrontNode.Next) != null)
					{
						bool flag7 = point == advancingFrontNode.Point;
						if (flag7)
						{
							break;
						}
					}
				}
			}
			this.Search = advancingFrontNode;
			return advancingFrontNode;
		}
	}
}
