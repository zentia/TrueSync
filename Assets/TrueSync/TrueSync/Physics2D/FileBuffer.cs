// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.FileBuffer
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System.IO;

namespace TrueSync.Physics2D
{
    internal class FileBuffer
    {
        public FileBuffer(Stream stream)
        {
            using (StreamReader streamReader = new StreamReader(stream))
                this.Buffer = streamReader.ReadToEnd();
            this.Position = 0;
        }

        public string Buffer { get; set; }

        public int Position { get; set; }

        private int Length
        {
            get
            {
                return this.Buffer.Length;
            }
        }

        public char Next
        {
            get
            {
                char ch = this.Buffer[this.Position];
                this.Position = this.Position + 1;
                return ch;
            }
        }

        public bool EndOfBuffer
        {
            get
            {
                return this.Position == this.Length;
            }
        }
    }
}
