using System;
using TrueSync;

namespace Microsoft.Xna.Framework
{
	public class Curve
	{
		private CurveKeyCollection keys;

		private CurveLoopType postLoop;

		private CurveLoopType preLoop;

		public bool IsConstant
		{
			get
			{
				return this.keys.Count <= 1;
			}
		}

		public CurveKeyCollection Keys
		{
			get
			{
				return this.keys;
			}
		}

		public CurveLoopType PostLoop
		{
			get
			{
				return this.postLoop;
			}
			set
			{
				this.postLoop = value;
			}
		}

		public CurveLoopType PreLoop
		{
			get
			{
				return this.preLoop;
			}
			set
			{
				this.preLoop = value;
			}
		}

		public Curve()
		{
			this.keys = new CurveKeyCollection();
		}

		public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
		{
			throw new NotImplementedException();
		}

		public void ComputeTangent(int keyIndex, CurveTangent tangentType)
		{
			this.ComputeTangent(keyIndex, tangentType, tangentType);
		}

		public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
		{
			throw new NotImplementedException();
		}

		public void ComputeTangents(CurveTangent tangentType)
		{
			this.ComputeTangents(tangentType, tangentType);
		}

		public Curve Clone()
		{
			return new Curve
			{
				keys = this.keys.Clone(),
				preLoop = this.preLoop,
				postLoop = this.postLoop
			};
		}

		public FP Evaluate(FP position)
		{
			CurveKey curveKey = this.keys[0];
			CurveKey curveKey2 = this.keys[this.keys.Count - 1];
			bool flag = position < curveKey.Position;
			FP result;
			if (flag)
			{
				switch (this.PreLoop)
				{
				case CurveLoopType.Constant:
					result = curveKey.Value;
					return result;
				case CurveLoopType.Cycle:
				{
					int numberOfCycle = this.GetNumberOfCycle(position);
					FP position2 = position - numberOfCycle * (curveKey2.Position - curveKey.Position);
					result = this.GetCurvePosition(position2);
					return result;
				}
				case CurveLoopType.CycleOffset:
				{
					int numberOfCycle = this.GetNumberOfCycle(position);
					FP position2 = position - numberOfCycle * (curveKey2.Position - curveKey.Position);
					result = this.GetCurvePosition(position2) + numberOfCycle * (curveKey2.Value - curveKey.Value);
					return result;
				}
				case CurveLoopType.Oscillate:
				{
					int numberOfCycle = this.GetNumberOfCycle(position);
					bool flag2 = 0f == (float)numberOfCycle % 2f;
					FP position2;
					if (flag2)
					{
						position2 = position - numberOfCycle * (curveKey2.Position - curveKey.Position);
					}
					else
					{
						position2 = curveKey2.Position - position + curveKey.Position + numberOfCycle * (curveKey2.Position - curveKey.Position);
					}
					result = this.GetCurvePosition(position2);
					return result;
				}
				case CurveLoopType.Linear:
					result = curveKey.Value - curveKey.TangentIn * (curveKey.Position - position);
					return result;
				}
			}
			else
			{
				bool flag3 = position > curveKey2.Position;
				if (flag3)
				{
					switch (this.PostLoop)
					{
					case CurveLoopType.Constant:
						result = curveKey2.Value;
						return result;
					case CurveLoopType.Cycle:
					{
						int numberOfCycle2 = this.GetNumberOfCycle(position);
						FP position3 = position - numberOfCycle2 * (curveKey2.Position - curveKey.Position);
						result = this.GetCurvePosition(position3);
						return result;
					}
					case CurveLoopType.CycleOffset:
					{
						int numberOfCycle2 = this.GetNumberOfCycle(position);
						FP position3 = position - numberOfCycle2 * (curveKey2.Position - curveKey.Position);
						result = this.GetCurvePosition(position3) + numberOfCycle2 * (curveKey2.Value - curveKey.Value);
						return result;
					}
					case CurveLoopType.Oscillate:
					{
						int numberOfCycle2 = this.GetNumberOfCycle(position);
						FP position3 = position - numberOfCycle2 * (curveKey2.Position - curveKey.Position);
						bool flag4 = 0f == (float)numberOfCycle2 % 2f;
						if (flag4)
						{
							position3 = position - numberOfCycle2 * (curveKey2.Position - curveKey.Position);
						}
						else
						{
							position3 = curveKey2.Position - position + curveKey.Position + numberOfCycle2 * (curveKey2.Position - curveKey.Position);
						}
						result = this.GetCurvePosition(position3);
						return result;
					}
					case CurveLoopType.Linear:
						result = curveKey2.Value + curveKey.TangentOut * (position - curveKey2.Position);
						return result;
					}
				}
			}
			result = this.GetCurvePosition(position);
			return result;
		}

		private int GetNumberOfCycle(FP position)
		{
			FP fP = (position - this.keys[0].Position) / (this.keys[this.keys.Count - 1].Position - this.keys[0].Position);
			bool flag = fP < 0f;
			if (flag)
			{
				fP -= 1;
			}
			return (int)((long)fP);
		}

		private FP GetCurvePosition(FP position)
		{
			CurveKey curveKey = this.keys[0];
			FP result;
			for (int i = 1; i < this.keys.Count; i++)
			{
				CurveKey curveKey2 = this.Keys[i];
				bool flag = curveKey2.Position >= position;
				if (flag)
				{
					bool flag2 = curveKey.Continuity == CurveContinuity.Step;
					if (flag2)
					{
						bool flag3 = position >= 1f;
						if (flag3)
						{
							result = curveKey2.Value;
						}
						else
						{
							result = curveKey.Value;
						}
					}
					else
					{
						FP fP = (position - curveKey.Position) / (curveKey2.Position - curveKey.Position);
						FP fP2 = fP * fP;
						FP fP3 = fP2 * fP;
						result = (2 * fP3 - 3 * fP2 + 1f) * curveKey.Value + (fP3 - 2 * fP2 + fP) * curveKey.TangentOut + (3 * fP2 - 2 * fP3) * curveKey2.Value + (fP3 - fP2) * curveKey2.TangentIn;
					}
					return result;
				}
				curveKey = curveKey2;
			}
			result = 0f;
			return result;
		}
	}
}
