using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class QueryGraph
	{
		private Node _head;

		public QueryGraph(Node head)
		{
			this._head = head;
		}

		private Trapezoid Locate(Edge edge)
		{
			return this._head.Locate(edge).Trapezoid;
		}

		public List<Trapezoid> FollowEdge(Edge edge)
		{
			List<Trapezoid> list = new List<Trapezoid>();
			list.Add(this.Locate(edge));
			int num = 0;
			while (edge.Q.X > list[num].RightPoint.X)
			{
				bool flag = edge.IsAbove(list[num].RightPoint);
				if (flag)
				{
					list.Add(list[num].UpperRight);
				}
				else
				{
					list.Add(list[num].LowerRight);
				}
				num++;
			}
			return list;
		}

		private void Replace(Sink sink, Node node)
		{
			bool flag = sink.ParentList.Count == 0;
			if (flag)
			{
				this._head = node;
			}
			else
			{
				node.Replace(sink);
			}
		}

		public void Case1(Sink sink, Edge edge, Trapezoid[] tList)
		{
			YNode lChild = new YNode(edge, Sink.Isink(tList[1]), Sink.Isink(tList[2]));
			XNode rChild = new XNode(edge.Q, lChild, Sink.Isink(tList[3]));
			XNode node = new XNode(edge.P, Sink.Isink(tList[0]), rChild);
			this.Replace(sink, node);
		}

		public void Case2(Sink sink, Edge edge, Trapezoid[] tList)
		{
			YNode rChild = new YNode(edge, Sink.Isink(tList[1]), Sink.Isink(tList[2]));
			XNode node = new XNode(edge.P, Sink.Isink(tList[0]), rChild);
			this.Replace(sink, node);
		}

		public void Case3(Sink sink, Edge edge, Trapezoid[] tList)
		{
			YNode node = new YNode(edge, Sink.Isink(tList[0]), Sink.Isink(tList[1]));
			this.Replace(sink, node);
		}

		public void Case4(Sink sink, Edge edge, Trapezoid[] tList)
		{
			YNode lChild = new YNode(edge, Sink.Isink(tList[0]), Sink.Isink(tList[1]));
			XNode node = new XNode(edge.Q, lChild, Sink.Isink(tList[2]));
			this.Replace(sink, node);
		}
	}
}
