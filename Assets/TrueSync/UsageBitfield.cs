using System;

namespace TrueSync
{
	public class UsageBitfield
	{
		private bool _usedVertexA;

		private bool _usedVertexB;

		private bool _usedVertexC;

		private bool _usedVertexD;

		public bool UsedVertexA
		{
			get
			{
				return this._usedVertexA;
			}
			set
			{
				this._usedVertexA = value;
			}
		}

		public bool UsedVertexB
		{
			get
			{
				return this._usedVertexB;
			}
			set
			{
				this._usedVertexB = value;
			}
		}

		public bool UsedVertexC
		{
			get
			{
				return this._usedVertexC;
			}
			set
			{
				this._usedVertexC = value;
			}
		}

		public bool UsedVertexD
		{
			get
			{
				return this._usedVertexD;
			}
			set
			{
				this._usedVertexD = value;
			}
		}

		public void Reset()
		{
			this._usedVertexA = (this._usedVertexB = (this._usedVertexC = (this._usedVertexD = false)));
		}
	}
}
