using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public class GravityController : Controller
	{
		public FP MinRadius
		{
			get;
			set;
		}

		public FP MaxRadius
		{
			get;
			set;
		}

		public FP Strength
		{
			get;
			set;
		}

		public GravityType GravityType
		{
			get;
			set;
		}

		public List<Body> Bodies
		{
			get;
			set;
		}

		public List<TSVector2> Points
		{
			get;
			set;
		}

		public GravityController(FP strength) : base(ControllerType.GravityController)
		{
			this.Strength = strength;
			this.MaxRadius = FP.MaxValue;
			this.GravityType = GravityType.DistanceSquared;
			this.Points = new List<TSVector2>();
			this.Bodies = new List<Body>();
		}

		public GravityController(FP strength, FP maxRadius, FP minRadius) : base(ControllerType.GravityController)
		{
			this.MinRadius = minRadius;
			this.MaxRadius = maxRadius;
			this.Strength = strength;
			this.GravityType = GravityType.DistanceSquared;
			this.Points = new List<TSVector2>();
			this.Bodies = new List<Body>();
		}

		public override void Update(FP dt)
		{
			TSVector2 tSVector = TSVector2.zero;
			foreach (Body current in this.World.BodyList)
			{
				bool flag = !this.IsActiveOn(current);
				if (!flag)
				{
					foreach (Body current2 in this.Bodies)
					{
						bool flag2 = current == current2 || (current.IsStatic && current2.IsStatic) || !current2.Enabled;
						if (!flag2)
						{
							TSVector2 value = current2.Position - current.Position;
							FP fP = value.LengthSquared();
							bool flag3 = fP <= Settings.Epsilon || fP > this.MaxRadius * this.MaxRadius || fP < this.MinRadius * this.MinRadius;
							if (!flag3)
							{
								GravityType gravityType = this.GravityType;
								if (gravityType != GravityType.Linear)
								{
									if (gravityType == GravityType.DistanceSquared)
									{
										tSVector = this.Strength / fP * current.Mass * current2.Mass * value;
									}
								}
								else
								{
									tSVector = this.Strength / FP.Sqrt(fP) * current.Mass * current2.Mass * value;
								}
								current.ApplyForce(ref tSVector);
							}
						}
					}
					foreach (TSVector2 current3 in this.Points)
					{
						TSVector2 value2 = current3 - current.Position;
						FP fP2 = value2.LengthSquared();
						bool flag4 = fP2 <= Settings.Epsilon || fP2 > this.MaxRadius * this.MaxRadius || fP2 < this.MinRadius * this.MinRadius;
						if (!flag4)
						{
							GravityType gravityType2 = this.GravityType;
							if (gravityType2 != GravityType.Linear)
							{
								if (gravityType2 == GravityType.DistanceSquared)
								{
									tSVector = this.Strength / fP2 * current.Mass * value2;
								}
							}
							else
							{
								tSVector = this.Strength / FP.Sqrt(fP2) * current.Mass * value2;
							}
							current.ApplyForce(ref tSVector);
						}
					}
				}
			}
		}

		public void AddBody(Body body)
		{
			this.Bodies.Add(body);
		}

		public void AddPoint(TSVector2 point)
		{
			this.Points.Add(point);
		}
	}
}
