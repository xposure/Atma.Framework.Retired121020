namespace Atma.Entities
{
    public abstract class DependencyResolver
    {
        public abstract void Resolve(ComponentSystemList list);
    }
}
