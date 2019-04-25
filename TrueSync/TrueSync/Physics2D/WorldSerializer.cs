namespace TrueSync.Physics2D
{
    using System;
    using System.IO;

    public static class WorldSerializer
    {
        public static World Deserialize(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                return WorldXmlDeserializer.Deserialize(stream);
            }
        }

        public static void Serialize(World world, string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                WorldXmlSerializer.Serialize(world, stream);
            }
        }
    }
}

