namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    public class TempPolygon
    {
        public int Count;
        public TSVector2[] Normals = new TSVector2[Settings.MaxPolygonVertices];
        public TSVector2[] Vertices = new TSVector2[Settings.MaxPolygonVertices];
    }
}

