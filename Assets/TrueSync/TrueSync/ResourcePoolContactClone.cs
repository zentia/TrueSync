namespace TrueSync
{
    public class ResourcePoolContactClone : ResourcePool<ContactClone>
    {
        protected override ContactClone NewInstance()
        {
            return new ContactClone();
        }
    }
}

