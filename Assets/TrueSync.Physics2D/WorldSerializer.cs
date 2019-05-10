using System;
using System.IO;

namespace TrueSync.Physics2D
{
	public static class WorldSerializer
	{
		public static void Serialize(World world, string filename)
		{
			using (FileStream fileStream = new FileStream(filename, FileMode.Create))
			{
				WorldXmlSerializer.Serialize(world, fileStream);
			}
		}

		public static World Deserialize(string filename)
		{
			World result;
			using (FileStream fileStream = new FileStream(filename, FileMode.Open))
			{
				result = WorldXmlDeserializer.Deserialize(fileStream);
			}
			return result;
		}
	}
}
