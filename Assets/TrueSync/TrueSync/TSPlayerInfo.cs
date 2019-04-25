namespace TrueSync
{
    using System;
    using UnityEngine;
    //玩家信息,保存2个属性
    [Serializable]
    public class TSPlayerInfo
    {
        [SerializeField]
        internal byte id;
        [SerializeField]
        internal string name;

        public TSPlayerInfo(byte id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public byte Id
        {
            get
            {
                return id;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }
    }
}

