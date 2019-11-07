namespace Atma.Entities
{
    using System;

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class HasComponent : Attribute
    {
        public Type[] Type { get; private set; }
        public HasComponent(params Type[] type)
        {
            Type = type;
        }

    }

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class AnyComponent : Attribute
    {
        public Type[] Type { get; private set; }
        public AnyComponent(params Type[] type)
        {
            Type = type;
        }
    }


    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class IgnoreComponent : Attribute
    {
        public Type[] Type { get; private set; }
        public IgnoreComponent(params Type[] type)
        {
            Type = type;
        }
    }

}
