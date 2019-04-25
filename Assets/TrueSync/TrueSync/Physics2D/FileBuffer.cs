namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class FileBuffer
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string <Buffer>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <Position>k__BackingField;

        public FileBuffer(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                this.Buffer = reader.ReadToEnd();
            }
            this.Position = 0;
        }

        public string Buffer { get; set; }

        public bool EndOfBuffer
        {
            get
            {
                return (this.Position == this.Length);
            }
        }

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
                int position = this.Position;
                this.Position = position + 1;
                return ch;
            }
        }

        public int Position { get; set; }
    }
}

