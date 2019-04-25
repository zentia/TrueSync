namespace TrueSync
{
    using System;

    public class TSRandom
    {
        public static TSRandom instance;
        private const uint LOWER_MASK = 0x7fffffff;
        private const int M = 0x18d;
        private uint[] mag01;
        private const uint MATRIX_A = 0x9908b0df;
        private const int MAX_RAND_INT = 0x7fffffff;
        private uint[] mt;
        private int mti;
        private const int N = 0x270;
        private const uint UPPER_MASK = 0x80000000;

        private TSRandom()
        {
            uint[] numArray1 = new uint[2];
            numArray1[1] = 0x9908b0df;
            this.mag01 = numArray1;
            this.mt = new uint[0x270];
            this.mti = 0x271;
            this.init_genrand((uint) DateTime.Now.Millisecond);
        }

        private TSRandom(int seed)
        {
            uint[] numArray1 = new uint[2];
            numArray1[1] = 0x9908b0df;
            this.mag01 = numArray1;
            this.mt = new uint[0x270];
            this.mti = 0x271;
            this.init_genrand((uint) seed);
        }

        private TSRandom(int[] init)
        {
            uint[] numArray1 = new uint[2];
            numArray1[1] = 0x9908b0df;
            this.mag01 = numArray1;
            this.mt = new uint[0x270];
            this.mti = 0x271;
            uint[] numArray = new uint[init.Length];
            for (int i = 0; i < init.Length; i++)
            {
                numArray[i] = (uint) init[i];
            }
            this.init_by_array(numArray, (uint) numArray.Length);
        }

        public static int CallNext()
        {
            return instance.Next();
        }

        private FP genrand_FP()
        {
            return (FP) (this.genrand_int32() * (FP.One / 0xffffffffL));
        }

        private int genrand_int31()
        {
            return (int) (this.genrand_int32() >> 1);
        }

        private uint genrand_int32()
        {
            uint num;
            if (this.mti >= 0x270)
            {
                if (this.mti == 0x271)
                {
                    this.init_genrand(0x1571);
                }
                int index = 0;
                while (index < 0xe3)
                {
                    num = (this.mt[index] & 0x80000000) | (this.mt[index + 1] & 0x7fffffff);
                    this.mt[index] = (this.mt[index + 0x18d] ^ (num >> 1)) ^ this.mag01[((int) num) & 1];
                    index++;
                }
                while (index < 0x26f)
                {
                    num = (this.mt[index] & 0x80000000) | (this.mt[index + 1] & 0x7fffffff);
                    this.mt[index] = (this.mt[index + -227] ^ (num >> 1)) ^ this.mag01[((int) num) & 1];
                    index++;
                }
                num = (this.mt[0x26f] & 0x80000000) | (this.mt[0] & 0x7fffffff);
                this.mt[0x26f] = (this.mt[0x18c] ^ (num >> 1)) ^ this.mag01[((int) num) & 1];
                this.mti = 0;
            }
            int mti = this.mti;
            this.mti = mti + 1;
            num = this.mt[mti];
            num ^= num >> 11;
            num ^= (num << 7) & 0x9d2c5680;
            num ^= (num << 15) & 0xefc60000;
            return (num ^ (num >> 0x12));
        }

        private double genrand_real1()
        {
            return (this.genrand_int32() * 2.3283064370807974E-10);
        }

        private double genrand_real2()
        {
            return (this.genrand_int32() * 2.3283064365386963E-10);
        }

        private double genrand_real3()
        {
            return ((this.genrand_int32() + 0.5) * 2.3283064365386963E-10);
        }

        private double genrand_res53()
        {
            uint num = this.genrand_int32() >> 5;
            uint num2 = this.genrand_int32() >> 6;
            return (((num * 67108864.0) + num2) * 1.1102230246251565E-16);
        }

        internal static void Init()
        {
            instance = New(1);
        }

        private void init_by_array(uint[] init_key, uint key_length)
        {
            int num3;
            this.init_genrand(0x12bd6aa);
            int index = 1;
            int num2 = 0;
            for (num3 = (0x270 > key_length) ? 0x270 : ((int) key_length); num3 > 0; num3--)
            {
                this.mt[index] = ((this.mt[index] ^ ((this.mt[index - 1] ^ (this.mt[index - 1] >> 30)) * 0x19660d)) + init_key[num2]) + ((uint) num2);
                this.mt[index] &= uint.MaxValue;
                index++;
                num2++;
                if (index >= 0x270)
                {
                    this.mt[0] = this.mt[0x26f];
                    index = 1;
                }
                if (num2 >= key_length)
                {
                    num2 = 0;
                }
            }
            for (num3 = 0x26f; num3 > 0; num3--)
            {
                this.mt[index] = (this.mt[index] ^ ((this.mt[index - 1] ^ (this.mt[index - 1] >> 30)) * 0x5d588b65)) - ((uint) index);
                this.mt[index] &= uint.MaxValue;
                index++;
                if (index >= 0x270)
                {
                    this.mt[0] = this.mt[0x26f];
                    index = 1;
                }
            }
            this.mt[0] = 0x80000000;
        }

        private void init_genrand(uint s)
        {
            this.mt[0] = s & uint.MaxValue;
            this.mti = 1;
            while (this.mti < 0x270)
            {
                this.mt[this.mti] = (0x6c078965 * (this.mt[this.mti - 1] ^ (this.mt[this.mti - 1] >> 30))) + ((uint) this.mti);
                this.mt[this.mti] &= uint.MaxValue;
                this.mti++;
            }
        }

        public void Initialize()
        {
            this.init_genrand((uint) DateTime.Now.Millisecond);
        }

        public void Initialize(int seed)
        {
            this.init_genrand((uint) seed);
        }

        public void Initialize(int[] init)
        {
            uint[] numArray = new uint[init.Length];
            for (int i = 0; i < init.Length; i++)
            {
                numArray[i] = (uint) init[i];
            }
            this.init_by_array(numArray, (uint) numArray.Length);
        }

        public static TSRandom New(int seed)
        {
            TSRandom random = new TSRandom(seed);
            StateTracker.AddTracking(random, "mt");
            StateTracker.AddTracking(random, "mti");
            return random;
        }

        public int Next()
        {
            return this.genrand_int31();
        }

        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                int num2 = maxValue;
                maxValue = minValue;
                minValue = num2;
            }
            int num = maxValue - minValue;
            return (minValue + (this.Next() % num));
        }

        public FP Next(float minValue, float maxValue)
        {
            int num = (int) (minValue * 1000f);
            int num2 = (int) (maxValue * 1000f);
            if (num > num2)
            {
                int num3 = num2;
                num2 = num;
                num = num3;
            }
            return (FP.Floor((((num2 - num) + 1) * this.NextFP()) + num) / 0x3e8);
        }

        private double Next53BitRes()
        {
            return this.genrand_res53();
        }

        private double NextDouble()
        {
            return this.genrand_real2();
        }

        private double NextDouble(bool includeOne)
        {
            if (includeOne)
            {
                return this.genrand_real1();
            }
            return this.genrand_real2();
        }

        private double NextDoublePositive()
        {
            return this.genrand_real3();
        }

        private float NextFloat()
        {
            return (float) this.genrand_real2();
        }

        private float NextFloat(bool includeOne)
        {
            if (includeOne)
            {
                return (float) this.genrand_real1();
            }
            return (float) this.genrand_real2();
        }

        private float NextFloatPositive()
        {
            return (float) this.genrand_real3();
        }

        public FP NextFP()
        {
            return (this.Next() / MaxRandomInt);
        }

        public static int Range(int minValue, int maxValue)
        {
            return instance.Next(minValue, maxValue);
        }

        public static FP Range(float minValue, float maxValue)
        {
            return instance.Next(minValue, maxValue);
        }

        public static TSVector insideUnitSphere
        {
            get
            {
                return new TSVector(value, value, value);
            }
        }

        public static int MaxRandomInt
        {
            get
            {
                return 0x7fffffff;
            }
        }

        public static FP value
        {
            get
            {
                return instance.NextFP();
            }
        }
    }
}

