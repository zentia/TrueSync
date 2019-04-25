namespace TrueSync
{
    internal class ResourcePoolContactClone2D : ResourcePool<ContactClone2D>
    {
        protected override ContactClone2D NewInstance()
        {
            return new ContactClone2D();
        }
    }
}

