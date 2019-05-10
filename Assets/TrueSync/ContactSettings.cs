using System;

namespace TrueSync
{
	public class ContactSettings
	{
		public enum MaterialCoefficientMixingType
		{
			TakeMaximum,
			TakeMinimum,
			UseAverage
		}

		internal FP maximumBias = 10;

		internal FP bias = 25 * FP.EN2;

		internal FP minVelocity = FP.EN3;

		internal FP allowedPenetration = FP.EN2;

		internal FP breakThreshold = FP.EN2;

		internal ContactSettings.MaterialCoefficientMixingType materialMode = ContactSettings.MaterialCoefficientMixingType.UseAverage;

		public FP MaximumBias
		{
			get
			{
				return this.maximumBias;
			}
			set
			{
				this.maximumBias = value;
			}
		}

		public FP BiasFactor
		{
			get
			{
				return this.bias;
			}
			set
			{
				this.bias = value;
			}
		}

		public FP MinimumVelocity
		{
			get
			{
				return this.minVelocity;
			}
			set
			{
				this.minVelocity = value;
			}
		}

		public FP AllowedPenetration
		{
			get
			{
				return this.allowedPenetration;
			}
			set
			{
				this.allowedPenetration = value;
			}
		}

		public FP BreakThreshold
		{
			get
			{
				return this.breakThreshold;
			}
			set
			{
				this.breakThreshold = value;
			}
		}

		public ContactSettings.MaterialCoefficientMixingType MaterialCoefficientMixing
		{
			get
			{
				return this.materialMode;
			}
			set
			{
				this.materialMode = value;
			}
		}
	}
}
