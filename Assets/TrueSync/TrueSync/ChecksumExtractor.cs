namespace TrueSync
{
    using System;

    public abstract class ChecksumExtractor
    {
        protected IPhysicsManager physicsManager;
        private static WorldChecksumExtractor worldExtractor;

        public ChecksumExtractor(IPhysicsManager physicsManager)
        {
            this.physicsManager = physicsManager;
        }

        protected abstract string GetChecksum();
        public static string GetEncodedChecksum()
        {
            return Utils.GetMd5Sum(worldExtractor.GetChecksum());
        }

        public static void Init(IPhysicsManager physicsManager)
        {
            worldExtractor = new WorldChecksumExtractor(physicsManager);
        }
    }
}

