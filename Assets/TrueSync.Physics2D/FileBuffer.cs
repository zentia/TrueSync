using System;
using System.IO;

namespace TrueSync.Physics2D
{
	internal class FileBuffer
	{
		public string Buffer
		{
			get;
			set;
		}

		public int Position
		{
			get;
			set;
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
				char result = this.Buffer[this.Position];
				int position = this.Position;
				this.Position = position + 1;
				return result;
			}
		}

		public bool EndOfBuffer
		{
			get
			{
				return this.Position == this.Length;
			}
		}

		public FileBuffer(Stream stream)
		{
			using (StreamReader streamReader = new StreamReader(stream))
			{
				this.Buffer = streamReader.ReadToEnd();
			}
			this.Position = 0;
		}
	}
}
