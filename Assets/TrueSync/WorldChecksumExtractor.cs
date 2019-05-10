using System;
using System.Text;

namespace TrueSync
{
	public class WorldChecksumExtractor : ChecksumExtractor
	{
		public WorldChecksumExtractor(IPhysicsManager physicsManager) : base(physicsManager)
		{
		}

		protected override string GetChecksum()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (IBody current in this.physicsManager.GetWorld().Bodies())
			{
				stringBuilder.Append(current.Checkum());
				stringBuilder.Append("|");
			}
			return stringBuilder.ToString();
		}
	}
}
