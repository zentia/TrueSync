using System;

namespace TrueSync
{
	public interface IBody
	{
		bool TSDisabled
		{
			get;
			set;
		}

		bool TSIsStatic
		{
			get;
			set;
		}

		string Checkum();

		void TSUpdate();
	}
}
