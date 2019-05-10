using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class MinkowskiSumShape : Shape
	{
		private TSVector shifted;

		private List<Shape> shapes = new List<Shape>();

		public MinkowskiSumShape(IEnumerable<Shape> shapes)
		{
			this.AddShapes(shapes);
		}

		public void AddShapes(IEnumerable<Shape> shapes)
		{
			foreach (Shape current in shapes)
			{
				bool flag = current is Multishape;
				if (flag)
				{
					throw new Exception("Multishapes not supported by MinkowskiSumShape.");
				}
				this.shapes.Add(current);
			}
			this.UpdateShape();
		}

		public void AddShape(Shape shape)
		{
			bool flag = shape is Multishape;
			if (flag)
			{
				throw new Exception("Multishapes not supported by MinkowskiSumShape.");
			}
			this.shapes.Add(shape);
			this.UpdateShape();
		}

		public bool Remove(Shape shape)
		{
			bool flag = this.shapes.Count == 1;
			if (flag)
			{
				throw new Exception("There must be at least one shape.");
			}
			bool result = this.shapes.Remove(shape);
			this.UpdateShape();
			return result;
		}

		public TSVector Shift()
		{
			return -1 * this.shifted;
		}

		public override void CalculateMassInertia()
		{
			this.mass = Shape.CalculateMassInertia(this, out this.shifted, out this.inertia);
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			TSVector zero = TSVector.zero;
			for (int i = 0; i < this.shapes.Count; i++)
			{
				TSVector tSVector;
				this.shapes[i].SupportMapping(ref direction, out tSVector);
				TSVector.Add(ref tSVector, ref zero, out zero);
			}
			TSVector.Subtract(ref zero, ref this.shifted, out result);
		}
	}
}
