namespace TrueSync
{
    using TrueSync.Physics2D;

    public class ResourcePoolContactEdge2D : ResourcePool<ContactEdge>
    {
        protected override ContactEdge NewInstance()
        {
            return new ContactEdge();
        }
    }
}

