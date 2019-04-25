namespace TrueSync
{
    public class ResourcePoolCollisionIslandClone : ResourcePool<CollisionIslandClone>
    {
        protected override CollisionIslandClone NewInstance()
        {
            return new CollisionIslandClone();
        }
    }
}

