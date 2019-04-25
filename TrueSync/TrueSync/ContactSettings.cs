namespace TrueSync
{
    using System;

    public class ContactSettings
    {
        internal FP allowedPenetration = FP.EN2;
        internal FP bias = (0x19 * FP.EN2);
        internal FP breakThreshold = FP.EN2;
        internal MaterialCoefficientMixingType materialMode = MaterialCoefficientMixingType.UseAverage;
        internal FP maximumBias = 10;
        internal FP minVelocity = FP.EN3;

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

        public MaterialCoefficientMixingType MaterialCoefficientMixing
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

        public enum MaterialCoefficientMixingType
        {
            TakeMaximum,
            TakeMinimum,
            UseAverage
        }
    }
}

