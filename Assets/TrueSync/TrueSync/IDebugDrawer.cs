namespace TrueSync
{
    using System;

    public interface IDebugDrawer
    {
        void DrawLine(TSVector start, TSVector end);
        void DrawPoint(TSVector pos);
        void DrawTriangle(TSVector pos1, TSVector pos2, TSVector pos3);
    }
}

