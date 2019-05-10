using System;

namespace TrueSync
{
	public abstract class ChecksumExtractor
	{
		private static WorldChecksumExtractor worldExtractor;

		protected IPhysicsManager physicsManager;

		protected abstract string GetChecksum();

		public ChecksumExtractor(IPhysicsManager physicsManager)
		{
			this.physicsManager = physicsManager;
		}

		public static void Init(IPhysicsManager physicsManager)
		{
			ChecksumExtractor.worldExtractor = new WorldChecksumExtractor(physicsManager);
		}

		public static string GetEncodedChecksum()
		{
			return Utils.GetMd5Sum(ChecksumExtractor.worldExtractor.GetChecksum());
		}
	}
}
