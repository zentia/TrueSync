namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public class TrueSyncInput
    {
        private static List<InputData> currentAllInputsData = new List<InputData>();
        private static InputData currentInputData;
        private static InputData currentSimulationData;

        public static List<InputData> GetAllInputs()
        {
            return currentAllInputsData;
        }

        public static byte GetByte(byte key)
        {
            return currentSimulationData.GetByte(key);
        }

        public static FP GetFP(byte key)
        {
            return currentSimulationData.GetFP(key);
        }

        public static int GetInt(byte key)
        {
            return currentSimulationData.GetInt(key);
        }

        public static string GetString(byte key)
        {
            return currentSimulationData.GetString(key);
        }

        public static TSVector GetTSVector(byte key)
        {
            return currentSimulationData.GetTSVector(key);
        }

        public static bool HasByte(byte key)
        {
            return currentSimulationData.HasByte(key);
        }

        public static bool HasFP(byte key)
        {
            return currentSimulationData.HasFP(key);
        }

        public static bool HasInt(byte key)
        {
            return currentSimulationData.HasInt(key);
        }

        public static bool HasString(byte key)
        {
            return currentSimulationData.HasString(key);
        }

        public static bool HasTSVector(byte key)
        {
            return currentSimulationData.HasTSVector(key);
        }

        public static void SetByte(byte key, byte value)
        {
            if (currentInputData > null)
            {
                currentInputData.AddByte(key, value);
            }
        }

        public static void SetFP(byte key, FP value)
        {
            if (currentInputData > null)
            {
                currentInputData.AddFP(key, value);
            }
        }

        public static void SetInt(byte key, int value)
        {
            if (currentInputData > null)
            {
                currentInputData.AddInt(key, value);
            }
        }

        public static void SetString(byte key, string value)
        {
            if (currentInputData > null)
            {
                currentInputData.AddString(key, value);
            }
        }

        public static void SetTSVector(byte key, TSVector value)
        {
            if (currentInputData > null)
            {
                currentInputData.AddTSVector(key, value);
            }
        }

        public static InputData CurrentInputData
        {
            set
            {
                currentInputData = value;
            }
        }

        public static InputData CurrentSimulationData
        {
            get
            {
                return currentSimulationData;
            }
            set
            {
                currentSimulationData = value;
            }
        }
    }
}

