namespace TrueSync
{
    using TrueSync.Physics2D;

    internal class ResourcePoolTreeFixtureProxy2D : ResourcePool<TreeNode<FixtureProxy>>
    {
        protected override TreeNode<FixtureProxy> NewInstance()
        {
            return new TreeNode<FixtureProxy>();
        }
    }
}

