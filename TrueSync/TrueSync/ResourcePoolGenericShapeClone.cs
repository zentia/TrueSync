namespace TrueSync
{
    public class ResourcePoolGenericShapeClone : ResourcePool<GenericShapeClone>
    {
        protected override GenericShapeClone NewInstance()
        {
            return new GenericShapeClone();
        }
    }
}

