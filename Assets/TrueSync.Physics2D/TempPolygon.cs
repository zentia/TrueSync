using System;

namespace TrueSync.Physics2D
{
	public class TempPolygon
	{
		public TSVector2[] Vertices = new TSVector2[Settings.MaxPolygonVertices];

		public TSVector2[] Normals = new TSVector2[Settings.MaxPolygonVertices];

		public int Count;
	}
}
