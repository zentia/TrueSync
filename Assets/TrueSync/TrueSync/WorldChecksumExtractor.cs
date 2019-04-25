namespace TrueSync
{
    using System;
    using System.Text;

    public class WorldChecksumExtractor : ChecksumExtractor
    {
        public WorldChecksumExtractor(IPhysicsManager physicsManager) : base(physicsManager)
        {
        }

        protected override string GetChecksum()
        {
            StringBuilder builder = new StringBuilder();
            foreach (IBody body in base.physicsManager.GetWorld().Bodies())
            {
                builder.Append(body.Checkum());
                builder.Append("|");
            }
            return builder.ToString();
        }
    }
}

