namespace TrueSync.Physics2D
{
    using System;
    using System.Text;
    using TrueSync;

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

        private AdvancingFrontNode FindSearchNode(FP x)
        {
            return this.Search;
        }

        private AdvancingFrontNode LocateNode(FP x)
        {
            AdvancingFrontNode node = this.FindSearchNode(x);
            if (x >= node.Value)
            {
                while ((node = node.Next) > null)
                {
                    if (x < node.Value)
                    {
                        this.Search = node.Prev;
                        return node.Prev;
                    }
                }
            }
            else
            {
                while ((node = node.Prev) > null)
                {
                    if (x >= node.Value)
                    {
                        this.Search = node;
                        return node;
                    }
                }
            }
            return null;
        }

        public AdvancingFrontNode LocateNode(TriangulationPoint point)
        {
            return this.LocateNode(point.X);
        }

        public AdvancingFrontNode LocatePoint(TriangulationPoint point)
        {
            FP x = point.X;
            AdvancingFrontNode next = this.FindSearchNode(x);
            FP fp2 = next.Point.X;
            if (x == fp2)
            {
                if (point != next.Point)
                {
                    if (point != next.Prev.Point)
                    {
                        if (point != next.Next.Point)
                        {
                            throw new Exception("Failed to find Node for given afront point");
                        }
                        next = next.Next;
                    }
                    else
                    {
                        next = next.Prev;
                    }
                }
            }
            else if (x >= fp2)
            {
                while ((next = next.Next) > null)
                {
                    if (point == next.Point)
                    {
                        break;
                    }
                }
            }
            else
            {
                while ((next = next.Prev) > null)
                {
                    if (point == next.Point)
                    {
                        break;
                    }
                }
            }
            this.Search = next;
            return next;
        }

        public void RemoveNode(AdvancingFrontNode node)
        {
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (AdvancingFrontNode node = this.Head; node != this.Tail; node = node.Next)
            {
                builder.Append(node.Point.X).Append("->");
            }
            builder.Append(this.Tail.Point.X);
            return builder.ToString();
        }
    }
}

