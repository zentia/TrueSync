using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class TrueSyncInput
	{
		private static List<InputData> currentAllInputsData = new List<InputData>();

		private static InputData currentInputData;

		private static InputData currentSimulationData;

		public static InputData CurrentInputData
		{
			set
			{
				TrueSyncInput.currentInputData = value;
			}
		}

		public static InputData CurrentSimulationData
		{
			get
			{
				return TrueSyncInput.currentSimulationData;
			}
			set
			{
				TrueSyncInput.currentSimulationData = value;
			}
		}

		public static void SetString(byte key, string value)
		{
			bool flag = TrueSyncInput.currentInputData != null;
			if (flag)
			{
				TrueSyncInput.currentInputData.AddString(key, value);
			}
		}

		public static void SetByte(byte key, byte value)
		{
			bool flag = TrueSyncInput.currentInputData != null;
			if (flag)
			{
				TrueSyncInput.currentInputData.AddByte(key, value);
			}
		}

		public static void SetInt(byte key, int value)
		{
			bool flag = TrueSyncInput.currentInputData != null;
			if (flag)
			{
				TrueSyncInput.currentInputData.AddInt(key, value);
			}
		}

		public static void SetFP(byte key, FP value)
		{
			bool flag = TrueSyncInput.currentInputData != null;
			if (flag)
			{
				TrueSyncInput.currentInputData.AddFP(key, value);
			}
		}

		public static void SetTSVector(byte key, TSVector value)
		{
			bool flag = TrueSyncInput.currentInputData != null;
			if (flag)
			{
				TrueSyncInput.currentInputData.AddTSVector(key, value);
			}
		}

		public static List<InputData> GetAllInputs()
		{
			return TrueSyncInput.currentAllInputsData;
		}

		public static string GetString(byte key)
		{
			return TrueSyncInput.currentSimulationData.GetString(key);
		}

		public static byte GetByte(byte key)
		{
			return TrueSyncInput.currentSimulationData.GetByte(key);
		}

		public static int GetInt(byte key)
		{
			return TrueSyncInput.currentSimulationData.GetInt(key);
		}

		public static FP GetFP(byte key)
		{
			return TrueSyncInput.currentSimulationData.GetFP(key);
		}

		public static TSVector GetTSVector(byte key)
		{
			return TrueSyncInput.currentSimulationData.GetTSVector(key);
		}

		public static bool HasString(byte key)
		{
			return TrueSyncInput.currentSimulationData.HasString(key);
		}

		public static bool HasByte(byte key)
		{
			return TrueSyncInput.currentSimulationData.HasByte(key);
		}

		public static bool HasInt(byte key)
		{
			return TrueSyncInput.currentSimulationData.HasInt(key);
		}

		public static bool HasFP(byte key)
		{
			return TrueSyncInput.currentSimulationData.HasFP(key);
		}

		public static bool HasTSVector(byte key)
		{
			return TrueSyncInput.currentSimulationData.HasTSVector(key);
		}
	}
}
