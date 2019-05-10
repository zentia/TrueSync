using System;

namespace TrueSync
{
	public class TSRandom
	{
		private const int N = 624;

		private const int M = 397;

		private const uint MATRIX_A = 2567483615u;

		private const uint UPPER_MASK = 2147483648u;

		private const uint LOWER_MASK = 2147483647u;

		private const int MAX_RAND_INT = 2147483647;

		private uint[] mag01 = new uint[]
		{
			0u,
			2567483615u
		};

		private uint[] mt = new uint[624];

		private int mti = 625;

		public static TSRandom instance;

		public static int MaxRandomInt
		{
			get
			{
				return 2147483647;
			}
		}

		public static FP value
		{
			get
			{
				return TSRandom.instance.NextFP();
			}
		}

		public static TSVector insideUnitSphere
		{
			get
			{
				return new TSVector(TSRandom.value, TSRandom.value, TSRandom.value);
			}
		}

		internal static void Init()
		{
			TSRandom.instance = TSRandom.New(1);
		}

		public static TSRandom New(int seed)
		{
			TSRandom tSRandom = new TSRandom(seed);
			StateTracker.AddTracking(tSRandom, "mt");
			StateTracker.AddTracking(tSRandom, "mti");
			return tSRandom;
		}

		private TSRandom()
		{
			this.init_genrand((uint)DateTime.Now.Millisecond);
		}

		private TSRandom(int seed)
		{
			this.init_genrand((uint)seed);
		}

		private TSRandom(int[] init)
		{
			uint[] array = new uint[init.Length];
			for (int i = 0; i < init.Length; i++)
			{
				array[i] = (uint)init[i];
			}
			this.init_by_array(array, (uint)array.Length);
		}

		public int Next()
		{
			return this.genrand_int31();
		}

		public static int CallNext()
		{
			return TSRandom.instance.Next();
		}

		public int Next(int minValue, int maxValue)
		{
			bool flag = minValue > maxValue;
			if (flag)
			{
				int num = maxValue;
				maxValue = minValue;
				minValue = num;
			}
			int num2 = maxValue - minValue;
			return minValue + this.Next() % num2;
		}

		public FP Next(float minValue, float maxValue)
		{
			int num = (int)(minValue * 1000f);
			int num2 = (int)(maxValue * 1000f);
			bool flag = num > num2;
			if (flag)
			{
				int num3 = num2;
				num2 = num;
				num = num3;
			}
			return FP.Floor((num2 - num + 1) * this.NextFP() + num) / 1000;
		}

		public static int Range(int minValue, int maxValue)
		{
			return TSRandom.instance.Next(minValue, maxValue);
		}

		public static FP Range(float minValue, float maxValue)
		{
			return TSRandom.instance.Next(minValue, maxValue);
		}

		public FP NextFP()
		{
			return this.Next() / TSRandom.MaxRandomInt;
		}

		private float NextFloat()
		{
			return (float)this.genrand_real2();
		}

		private float NextFloat(bool includeOne)
		{
			float result;
			if (includeOne)
			{
				result = (float)this.genrand_real1();
			}
			else
			{
				result = (float)this.genrand_real2();
			}
			return result;
		}

		private float NextFloatPositive()
		{
			return (float)this.genrand_real3();
		}

		private double NextDouble()
		{
			return this.genrand_real2();
		}

		private double NextDouble(bool includeOne)
		{
			double result;
			if (includeOne)
			{
				result = this.genrand_real1();
			}
			else
			{
				result = this.genrand_real2();
			}
			return result;
		}

		private double NextDoublePositive()
		{
			return this.genrand_real3();
		}

		private double Next53BitRes()
		{
			return this.genrand_res53();
		}

		public void Initialize()
		{
			this.init_genrand((uint)DateTime.Now.Millisecond);
		}

		public void Initialize(int seed)
		{
			this.init_genrand((uint)seed);
		}

		public void Initialize(int[] init)
		{
			uint[] array = new uint[init.Length];
			for (int i = 0; i < init.Length; i++)
			{
				array[i] = (uint)init[i];
			}
			this.init_by_array(array, (uint)array.Length);
		}

		private void init_genrand(uint s)
		{
			this.mt[0] = (s & 4294967295u);
			this.mti = 1;
			while (this.mti < 624)
			{
				this.mt[this.mti] = (uint)((ulong)(1812433253u * (this.mt[this.mti - 1] ^ this.mt[this.mti - 1] >> 30)) + (ulong)((long)this.mti));
				this.mt[this.mti] &= 4294967295u;
				this.mti++;
			}
		}

		private void init_by_array(uint[] init_key, uint key_length)
		{
			this.init_genrand(19650218u);
			int num = 1;
			int num2 = 0;
			for (int i = (int)((624u > key_length) ? 624u : key_length); i > 0; i--)
			{
				this.mt[num] = (uint)((ulong)((this.mt[num] ^ (this.mt[num - 1] ^ this.mt[num - 1] >> 30) * 1664525u) + init_key[num2]) + (ulong)((long)num2));
				this.mt[num] &= 4294967295u;
				num++;
				num2++;
				bool flag = num >= 624;
				if (flag)
				{
					this.mt[0] = this.mt[623];
					num = 1;
				}
				bool flag2 = (long)num2 >= (long)((ulong)key_length);
				if (flag2)
				{
					num2 = 0;
				}
			}
			for (int i = 623; i > 0; i--)
			{
				this.mt[num] = (uint)((ulong)(this.mt[num] ^ (this.mt[num - 1] ^ this.mt[num - 1] >> 30) * 1566083941u) - (ulong)((long)num));
				this.mt[num] &= 4294967295u;
				num++;
				bool flag3 = num >= 624;
				if (flag3)
				{
					this.mt[0] = this.mt[623];
					num = 1;
				}
			}
			this.mt[0] = 2147483648u;
		}

		private uint genrand_int32()
		{
			bool flag = this.mti >= 624;
			uint num;
			if (flag)
			{
				bool flag2 = this.mti == 625;
				if (flag2)
				{
					this.init_genrand(5489u);
				}
				int i;
				for (i = 0; i < 227; i++)
				{
					num = ((this.mt[i] & 2147483648u) | (this.mt[i + 1] & 2147483647u));
					this.mt[i] = (this.mt[i + 397] ^ num >> 1 ^ this.mag01[(int)(num & 1u)]);
				}
				while (i < 623)
				{
					num = ((this.mt[i] & 2147483648u) | (this.mt[i + 1] & 2147483647u));
					this.mt[i] = (this.mt[i + -227] ^ num >> 1 ^ this.mag01[(int)(num & 1u)]);
					i++;
				}
				num = ((this.mt[623] & 2147483648u) | (this.mt[0] & 2147483647u));
				this.mt[623] = (this.mt[396] ^ num >> 1 ^ this.mag01[(int)(num & 1u)]);
				this.mti = 0;
			}
			uint[] arg_159_0 = this.mt;
			int num2 = this.mti;
			this.mti = num2 + 1;
			num = arg_159_0[num2];
			num ^= num >> 11;
			num ^= (num << 7 & 2636928640u);
			num ^= (num << 15 & 4022730752u);
			return num ^ num >> 18;
		}

		private int genrand_int31()
		{
			return (int)(this.genrand_int32() >> 1);
		}

		private FP genrand_FP()
		{
			return (long)((ulong)this.genrand_int32()) * (FP.One / (long)((ulong)-1));
		}

		private double genrand_real1()
		{
			return this.genrand_int32() * 2.3283064370807974E-10;
		}

		private double genrand_real2()
		{
			return this.genrand_int32() * 2.3283064365386963E-10;
		}

		private double genrand_real3()
		{
			return (this.genrand_int32() + 0.5) * 2.3283064365386963E-10;
		}

		private double genrand_res53()
		{
			uint num = this.genrand_int32() >> 5;
			uint num2 = this.genrand_int32() >> 6;
			return (num * 67108864.0 + num2) * 1.1102230246251565E-16;
		}
	}
}
