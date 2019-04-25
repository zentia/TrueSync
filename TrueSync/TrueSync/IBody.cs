namespace TrueSync
{
    //表示到2D和3D实体的公共接口。
    public interface IBody
    {
        string Checkum();
        void TSUpdate();
        //如果是真的，身体就不会干涉物理模拟。
        bool TSDisabled { get; set; }
        //如果是真的，身体就不会因为碰撞而移动。
        bool TSIsStatic { get; set; }
    }
}

