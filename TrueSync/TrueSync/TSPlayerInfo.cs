namespace TrueSync
{
    using System;
    using UnityEngine;

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
                return this.id;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

