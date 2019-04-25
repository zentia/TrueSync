namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;

    internal class QueryGraph
    {
        private Node _head;

        public QueryGraph(Node head)
        {
            this._head = head;
        }

        public void Case1(Sink sink, TrueSync.Physics2D.Edge edge, Trapezoid[] tList)
        {
            YNode lChild = new YNode(edge, Sink.Isink(tList[1]), Sink.Isink(tList[2]));
            XNode rChild = new XNode(edge.Q, lChild, Sink.Isink(tList[3]));
            XNode node = new XNode(edge.P, Sink.Isink(tList[0]), rChild);
            this.Replace(sink, node);
        }

        public void Case2(Sink sink, TrueSync.Physics2D.Edge edge, Trapezoid[] tList)
        {
            YNode rChild = new YNode(edge, Sink.Isink(tList[1]), Sink.Isink(tList[2]));
            XNode node = new XNode(edge.P, Sink.Isink(tList[0]), rChild);
            this.Replace(sink, node);
        }

        public void Case3(Sink sink, TrueSync.Physics2D.Edge edge, Trapezoid[] tList)
        {
            YNode node = new YNode(edge, Sink.Isink(tList[0]), Sink.Isink(tList[1]));
            this.Replace(sink, node);
        }

        public void Case4(Sink sink, TrueSync.Physics2D.Edge edge, Trapezoid[] tList)
        {
            YNode lChild = new YNode(edge, Sink.Isink(tList[0]), Sink.Isink(tList[1]));
            XNode node = new XNode(edge.Q, lChild, Sink.Isink(tList[2]));
            this.Replace(sink, node);
        }

        public List<Trapezoid> FollowEdge(TrueSync.Physics2D.Edge edge)
        {
            List<Trapezoid> list = new List<Trapezoid> {
                this.Locate(edge)
            };
            for (int i = 0; edge.Q.X > list[i].RightPoint.X; i++)
            {
                if (edge.IsAbove(list[i].RightPoint))
                {
                    list.Add(list[i].UpperRight);
                }
                else
                {
                    list.Add(list[i].LowerRight);
                }
            }
            return list;
        }

        private Trapezoid Locate(TrueSync.Physics2D.Edge edge)
        {
            return this._head.Locate(edge).Trapezoid;
        }

        private void Replace(Sink sink, Node node)
        {
            if (sink.ParentList.Count == 0)
            {
                this._head = node;
            }
            else
            {
                node.Replace(sink);
            }
        }
    }
}

