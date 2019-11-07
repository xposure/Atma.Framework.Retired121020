namespace Atma.Entities
{
    using System.Linq;

    public class EntitySpecification
    {
        public readonly ComponentType[] ComponentTypes;
        public readonly int EntitySize;

        public EntitySpecification(params ComponentType[] componentTypes)
        {
            ComponentTypes = componentTypes;
            EntitySize = ComponentTypes.Sum(x => x.Size);
        }
    }
}